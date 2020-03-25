﻿// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using Dapplo.Windows.Ten.Internal;
using Dapplo.Windows.Ten.Native;
using Color = Windows.UI.Color;

namespace Dapplo.Windows.Ten
{
    /// <summary>
    /// This uses the Share from Windows 10 to make the capture available to apps.
    /// </summary>
    public class Win10ShareDestination : AbstractDestination
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Win10ShareDestination));

        public override string Designation { get; } = "Windows10Share";
        public override string Description { get; } = "Windows 10 share";

        /// <summary>
        /// Icon for the App-share, the icon was found via: http://help4windows.com/windows_8_shell32_dll.shtml
        /// </summary>
        public override Image DisplayIcon => PluginUtils.GetCachedExeIcon(FilenameHelper.FillCmdVariables(@"%windir%\system32\shell32.dll"), 238);

        /// <summary>
        /// Share the screenshot with a windows app
        /// </summary>
        /// <param name="manuallyInitiated">bool</param>
        /// <param name="surface">ISurface</param>
        /// <param name="captureDetails">ICaptureDetails</param>
        /// <returns>ExportInformation</returns>
		public override ExportInformation ExportCapture(bool manuallyInitiated, ISurface surface, ICaptureDetails captureDetails)
        {
            var exportInformation = new ExportInformation(Designation, Description);
            try
            {
                var triggerWindow = new Window
                {
                    WindowState = WindowState.Normal,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    WindowStyle = WindowStyle.None,
                    // TODO: Define right size
                    Width = 400,
                    Height = 400,
                    AllowsTransparency = true,
                    Background = new SolidColorBrush(Colors.Transparent)
                };
                var shareInfo = new ShareInfo();

                triggerWindow.Show();

                var focusMonitor = new WindowsOpenCloseMonitor();
                var windowHandle = new WindowInteropHelper(triggerWindow).Handle;

                // This is a bad trick, but don't know how else to do it.
                // Wait for the focus to return, and depending on the state close the window!
                focusMonitor.WindowOpenCloseChangeEvent += e =>
                {

                    if (e.IsOpen)
                    {
                        if ("Windows Shell Experience Host" == e.Title)
                        {
                            shareInfo.SharingHwnd = e.HWnd;
                        }
                        return;
                    }

                    if (e.HWnd == shareInfo.SharingHwnd)
                    {
                        if (shareInfo.ApplicationName != null)
                        {
                            return;
                        }
                        shareInfo.ShareTask.TrySetResult(false);
                    }
                };

                Share(shareInfo, windowHandle, surface, captureDetails).GetAwaiter().GetResult();
                Log.Debug("Sharing finished, closing window.");
                triggerWindow.Close();
                focusMonitor.Dispose();
                if (string.IsNullOrWhiteSpace(shareInfo.ApplicationName))
                {
                    exportInformation.ExportMade = false;
                }
                else
                {
                    exportInformation.ExportMade = true;
                    exportInformation.DestinationDescription = shareInfo.ApplicationName;
                }
            }
            catch (Exception ex)
            {
                exportInformation.ExportMade = false;
                exportInformation.ErrorMessage = ex.Message;
            }

            ProcessExport(exportInformation, surface);
            return exportInformation;

        }
        /// <summary>
        /// Share the surface by using the Share-UI of Windows 10
        /// </summary>
        /// <param name="shareInfo">ShareInfo</param>
        /// <param name="handle">IntPtr with the handle for the hosting window</param>
        /// <param name="surface">ISurface with the bitmap to share</param>
        /// <param name="captureDetails">ICaptureDetails</param>
        /// <returns>Task with string, which describes the application which was used to share with</returns>
        private async Task Share(ShareInfo shareInfo, IntPtr handle, ISurface surface, ICaptureDetails captureDetails)
        {
            using var imageStream = new MemoryRandomAccessStream();
            using var logoStream = new MemoryRandomAccessStream();
            using var thumbnailStream = new MemoryRandomAccessStream();
            var outputSettings = new SurfaceOutputSettings();
            outputSettings.PreventGreenshotFormat();

            // Create capture for export
            ImageOutput.SaveToStream(surface, imageStream, outputSettings);
            imageStream.Position = 0;
            Log.Debug("Created RandomAccessStreamReference for the image");
            var imageRandomAccessStreamReference = RandomAccessStreamReference.CreateFromStream(imageStream);

            // Create thumbnail
            RandomAccessStreamReference thumbnailRandomAccessStreamReference;
            using (var tmpImageForThumbnail = surface.GetImageForExport())
            using (var thumbnail = ImageHelper.CreateThumbnail(tmpImageForThumbnail, 240, 160))
            {
                ImageOutput.SaveToStream(thumbnail, null, thumbnailStream, outputSettings);
                thumbnailStream.Position = 0;
                thumbnailRandomAccessStreamReference = RandomAccessStreamReference.CreateFromStream(thumbnailStream);
                Log.Debug("Created RandomAccessStreamReference for the thumbnail");
            }

            // Create logo
            RandomAccessStreamReference logoRandomAccessStreamReference;
            using (var logo = GreenshotResources.GetGreenshotIcon().ToBitmap())
            using (var logoThumbnail = ImageHelper.CreateThumbnail(logo, 30, 30))
            {
                ImageOutput.SaveToStream(logoThumbnail, null, logoStream, outputSettings);
                logoStream.Position = 0;
                logoRandomAccessStreamReference = RandomAccessStreamReference.CreateFromStream(logoStream);
                Log.Info("Created RandomAccessStreamReference for the logo");
            }

            var dataTransferManagerHelper = new DataTransferManagerHelper(handle);
            dataTransferManagerHelper.DataTransferManager.ShareProvidersRequested += (sender, args) =>
            {
                shareInfo.AreShareProvidersRequested = true;
                Log.DebugFormat("Share providers requested: {0}", string.Join(",", args.Providers.Select(p => p.Title)));
            };
            dataTransferManagerHelper.DataTransferManager.TargetApplicationChosen += (dtm, args) =>
            {
                shareInfo.ApplicationName = args.ApplicationName;
                Log.DebugFormat("TargetApplicationChosen: {0}", args.ApplicationName);
            };
            var filename = FilenameHelper.GetFilename(OutputFormat.png, captureDetails);
            var storageFile = await StorageFile.CreateStreamedFileAsync(filename, async streamedFileDataRequest =>
            {
                shareInfo.IsDeferredFileCreated = true;
                    // Information on the "how" was found here: https://socialeboladev.wordpress.com/2013/03/15/how-to-use-createstreamedfileasync/
                    Log.DebugFormat("Creating deferred file {0}", filename);
                try
                {
                    using (var deferredStream = streamedFileDataRequest.AsStreamForWrite())
                    {
                        await imageStream.CopyToAsync(deferredStream).ConfigureAwait(false);
                        await imageStream.FlushAsync().ConfigureAwait(false);
                    }
                    // Signal that the stream is ready
                    streamedFileDataRequest.Dispose();
                    // Signal that the action is ready, bitmap was exported
                    shareInfo.ShareTask.TrySetResult(true);
                }
                catch (Exception)
                {
                    streamedFileDataRequest.FailAndClose(StreamedFileFailureMode.Incomplete);
                }
            }, imageRandomAccessStreamReference).AsTask().ConfigureAwait(false);

            dataTransferManagerHelper.DataTransferManager.DataRequested += (dataTransferManager, dataRequestedEventArgs) =>
            {
                var deferral = dataRequestedEventArgs.Request.GetDeferral();
                try
                {
                    shareInfo.IsDataRequested = true;
                    Log.DebugFormat("DataRequested with operation {0}", dataRequestedEventArgs.Request.Data.RequestedOperation);
                    var dataPackage = dataRequestedEventArgs.Request.Data;
                    dataPackage.OperationCompleted += (dp, eventArgs) =>
                    {
                        Log.DebugFormat("OperationCompleted: {0}, shared with", eventArgs.Operation);
                        shareInfo.CompletedWithOperation = eventArgs.Operation;
                        shareInfo.AcceptedFormat = eventArgs.AcceptedFormatId;

                        shareInfo.ShareTask.TrySetResult(true);
                    };
                    dataPackage.Destroyed += (dp, o) =>
                    {
                        shareInfo.IsDestroyed = true;
                        Log.Debug("Destroyed");
                        shareInfo.ShareTask.TrySetResult(true);
                    };
                    dataPackage.ShareCompleted += (dp, shareCompletedEventArgs) =>
                    {
                        shareInfo.IsShareCompleted = true;
                        Log.Debug("ShareCompleted");
                        shareInfo.ShareTask.TrySetResult(true);
                    };
                    dataPackage.Properties.Title = captureDetails.Title;
                    dataPackage.Properties.ApplicationName = "Greenshot";
                    dataPackage.Properties.Description = "Share a screenshot";
                    dataPackage.Properties.Thumbnail = thumbnailRandomAccessStreamReference;
                    dataPackage.Properties.Square30x30Logo = logoRandomAccessStreamReference;
                    dataPackage.Properties.LogoBackgroundColor = Color.FromArgb(0xff, 0x3d, 0x3d, 0x3d);
                    dataPackage.SetStorageItems(new[] { storageFile });
                    dataPackage.SetBitmap(imageRandomAccessStreamReference);
                }
                finally
                {
                    deferral.Complete();
                    Log.Debug("Called deferral.Complete()");
                }
            };
            dataTransferManagerHelper.ShowShareUi();
            Log.Debug("ShowShareUi finished.");
            await shareInfo.ShareTask.Task.ConfigureAwait(false);
        }
    }
}

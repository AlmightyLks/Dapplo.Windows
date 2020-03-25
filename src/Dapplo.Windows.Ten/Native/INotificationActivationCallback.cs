﻿// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System.Runtime.InteropServices;
using Dapplo.Windows.Ten.Native.Structs;

namespace Dapplo.Windows.Ten.Native
{
    /// <summary>
    /// This is the interface which allows your notifications to be clicked, which active the application
    /// </summary>
    [ComImport, Guid("53E31837-6600-4A81-9395-75CFFE746F94"),
     ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface INotificationActivationCallback
    {
        void Activate(
            [In, MarshalAs(UnmanagedType.LPWStr)]
            string appUserModelId,
            [In, MarshalAs(UnmanagedType.LPWStr)]
            string invokedArgs,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]
            NotificationUserInputData[] data,
            [In, MarshalAs(UnmanagedType.U4)]
            uint dataCount);
    }
}

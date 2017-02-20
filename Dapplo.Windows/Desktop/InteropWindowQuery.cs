﻿#region Dapplo 2017 - GNU Lesser General Public License

// Dapplo - building blocks for .NET applications
// Copyright (C) 2017 Dapplo
// 
// For more information see: http://dapplo.net/
// Dapplo repositories are hosted on GitHub: https://github.com/dapplo
// 
// This file is part of Dapplo.Windows
// 
// Dapplo.Windows is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Dapplo.Windows is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have a copy of the GNU Lesser General Public License
// along with Dapplo.Windows. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dapplo.Windows.App;
using Dapplo.Windows.Enums;
using Dapplo.Windows.Native;

#endregion

namespace Dapplo.Windows.Desktop
{
	/// <summary>
	///     Query for native windows
	/// </summary>
	public static class InteropWindowQuery
	{
		/// <summary>
		///     Window classes which can be ignored
		/// </summary>
		public static IList<string> IgnoreClasses { get; } = new List<string>(new[] {"Progman", "Button", "Dwm"}); //"MS-SDIa"

		/// <summary>
		///     Check if the window is a top level window.
		///     This method will retrieve all information, and fill it to the interopWindow, it needs to make the decision.
		/// </summary>
		/// <param name="interopWindow">InteropWindow</param>
		/// <param name="ignoreKnowClasses">true (default) to ignore classes from the IgnoreClasses list</param>
		/// <returns>bool</returns>
		public static bool IsTopLevel(this IInteropWindow interopWindow, bool ignoreKnowClasses = true)
		{
			if (ignoreKnowClasses && IgnoreClasses.Contains(interopWindow.GetClassname()))
			{
				return false;
			}
			// Ignore windows without title
			if (interopWindow.GetCaption().Length == 0)
			{
				return false;
			}
			// Windows without size
			if (interopWindow.GetBounds().IsEmpty)
			{
				return false;
			}
			if (interopWindow.GetParent() != IntPtr.Zero)
			{
				return false;
			}
			var exWindowStyle = interopWindow.GetExtendedStyle();
			if ((exWindowStyle & ExtendedWindowStyleFlags.WS_EX_TOOLWINDOW) != 0)
			{
				return false;
			}
			// Skip everything which is not rendered "normally"
			if (!interopWindow.IsWin8App() && (exWindowStyle & ExtendedWindowStyleFlags.WS_EX_NOREDIRECTIONBITMAP) != 0)
			{
				return false;
			}
			// Skip preview windows, like the one from Firefox
			if ((interopWindow.GetStyle() & WindowStyleFlags.WS_VISIBLE) == 0)
			{
				return false;
			}
			return !interopWindow.IsMinimized();
		}

		/// <summary>
		///     Iterate the Top level windows, from top to bottom
		/// </summary>
		/// <param name="ignoreKnownClasses">true to ignore windows with certain known classes</param>
		/// <returns>IEnumerable with all the top level windows</returns>
		public static IEnumerable<IInteropWindow> GetTopLevelWindows(bool ignoreKnownClasses = true)
		{
			foreach (var possibleTopLevel in GetTopWindows())
			{
				if (possibleTopLevel.IsTopLevel(ignoreKnownClasses))
				{
					yield return possibleTopLevel;
				}
			}
		}

		/// <summary>
		///     Iterate the windows, from top to bottom
		/// </summary>
		/// <param name="parent">InteropWindow as the parent, to iterate over the children, or null for all</param>
		/// <returns>IEnumerable with all the top level windows</returns>
		public static IEnumerable<IInteropWindow> GetTopWindows(IInteropWindow parent = null)
		{
			// TODO: Guard against looping
			IntPtr windowPtr = parent == null ? User32.GetTopWindow(IntPtr.Zero) : User32.GetWindow(parent.Handle, GetWindowCommands.GW_CHILD);
			do
			{
				yield return InteropWindowFactory.CreateFor(windowPtr);
				windowPtr = User32.GetWindow(windowPtr, GetWindowCommands.GW_HWNDNEXT);
			} while (windowPtr != IntPtr.Zero);
		}

		/// <summary>
		///     Get the currently active window
		/// </summary>
		/// <returns>InteropWindow</returns>
		public static IInteropWindow GetActiveWindow()
		{
			return InteropWindowFactory.CreateFor(User32.GetForegroundWindow());
		}

		/// <summary>
		/// Gets the Desktop window
		/// </summary>
		/// <returns>InteropWindow for the desktop window</returns>
		public static IInteropWindow GetDesktopWindow()
		{
			return InteropWindowFactory.CreateFor(User32.GetDesktopWindow());
		}

		/// <summary>
		/// Find windows belonging to the same process (thread) as the supplied window.
		/// </summary>
		/// <param name="windowToLinkTo">InteropWindow</param>
		/// <returns>IEnumerable with InteropWindow</returns>
		public static IEnumerable<IInteropWindow> GetLinkedWindows(this IInteropWindow windowToLinkTo)
		{
			int processIdSelectedWindow = windowToLinkTo.GetProcessId();

			using (var process = Process.GetProcessById(processIdSelectedWindow))
			{
				foreach (ProcessThread thread in process.Threads)
				{
					var handles = new List<IntPtr>();
					try
					{
						User32.EnumThreadWindows(thread.Id, (hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);
					}
					finally
					{
						thread?.Dispose();
					}
					foreach (var handle in handles)
					{
						yield return InteropWindowFactory.CreateFor(handle);
					}
				}
			}
		}
	}
}
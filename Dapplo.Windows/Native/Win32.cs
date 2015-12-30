﻿/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) Dapplo 2015-2016
 * 
 * For more information see: http://dapplo.net/
 * dapplo repositories are hosted on GitHub: https://github.com/dapplo
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 1 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using Dapplo.Windows.Enums;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Dapplo.Windows.Native
{
	public static class Win32
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		private static extern uint FormatMessage(uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, [Out] StringBuilder lpBuffer, int nSize, IntPtr Arguments);

		[DllImport("kernel32.dll")]
		public static extern void SetLastError(uint dwErrCode);

		public static Win32Error GetLastErrorCode()
		{
			return (Win32Error)Marshal.GetLastWin32Error();
		}

		public static long GetHResult(Win32Error errorCode)
		{
			int error = (int)errorCode;

			if ((error & 0x80000000) == 0x80000000)
			{
				return (long)error;
			}

			return (long)(0x80070000 | (uint)(error & 0xffff));
		}

		public static string GetMessage(Win32Error errorCode)
		{
			StringBuilder buffer = new StringBuilder(0x100);

			if (FormatMessage(0x3200, IntPtr.Zero, (uint)errorCode, 0, buffer, buffer.Capacity, IntPtr.Zero) == 0)
			{
				return "Unknown error (0x" + ((int)errorCode).ToString("x") + ")";
			}

			StringBuilder result = new StringBuilder();
			int i = 0;

			while (i < buffer.Length)
			{
				if (!char.IsLetterOrDigit(buffer[i]) && !char.IsPunctuation(buffer[i]) && !char.IsSymbol(buffer[i]) && !char.IsWhiteSpace(buffer[i]))
				{
					break;
				}

				result.Append(buffer[i]);
				i++;
			}

			return result.ToString().Replace("\r\n", "");
		}
	}
}

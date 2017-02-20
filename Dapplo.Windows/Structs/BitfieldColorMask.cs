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

using System.Runtime.InteropServices;

#endregion

namespace Dapplo.Windows.Structs
{
	[StructLayout(LayoutKind.Sequential)]
	public struct BitfieldColorMask
	{
		public uint blue;
		public uint green;
		public uint red;

		public void InitValues()
		{
			red = (uint) 255 << 8;
			green = (uint) 255 << 16;
			blue = (uint) 255 << 24;
		}
	}
}
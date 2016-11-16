﻿//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016 Dapplo
// 
//  For more information see: http://dapplo.net/
//  Dapplo repositories are hosted on GitHub: https://github.com/dapplo
// 
//  This file is part of Dapplo.Windows
// 
//  Dapplo.Windows is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  Dapplo.Windows is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
// 
//  You should have a copy of the GNU Lesser General Public License
//  along with Dapplo.Windows. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

namespace Dapplo.Windows.Enums
{
	/// <summary>
	///     Get/Set WindowLong Enum See: http://msdn.microsoft.com/en-us/library/ms633591.aspx
	/// </summary>
	public enum WindowLongIndex
	{
		GWL_EXSTYLE = -20, // Sets a new extended window style.
		GWL_HINSTANCE = -6, // Sets a new application instance handle.
		GWL_ID = -12, // Sets a new identifier of the child window. The window cannot be a top-level window.
		GWL_STYLE = -16, // Sets a new window style.
		GWL_USERDATA = -21, // Sets the user data associated with the window. This data is intended for use by the application that created the window. Its value is initially zero.
		GWL_WNDPROC = -4 // Sets a new address for the window procedure. You cannot change this attribute if the window does not belong to the same process as the calling thread.
	}
}
﻿//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2017-2019  Dapplo
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

using System;

namespace Dapplo.Windows.Desktop
{
    /// <summary>
    ///     Factory for InteropWindows
    /// </summary>
    public static class InteropWindowFactory
    {
        /// <summary>
        ///     Factory method to create a InteropWindow for the supplied handle
        /// </summary>
        /// <param name="handle">int</param>
        /// <returns>InteropWindow</returns>
        public static InteropWindow CreateFor(int handle)
        {
            return CreateFor(new IntPtr(handle));
        }

        /// <summary>
        ///     Factory method to create a InteropWindow for the supplied handle
        /// </summary>
        /// <param name="handle">IntPtr</param>
        /// <returns>InteropWindow</returns>
        public static InteropWindow CreateFor(IntPtr handle)
        {
            return new InteropWindow(handle);
        }
    }
}
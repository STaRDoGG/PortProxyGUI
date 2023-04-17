﻿using System.Drawing;

namespace PortProxyGooey.Utils
{
    public class InterfaceUtil
    {
        /// <summary>
        /// Compatibility between .NET Framework and .NET Core.
        /// <see href="https://docs.microsoft.com/en-us/dotnet/core/compatibility/winforms" />
        /// </summary>
        public static readonly Font UiFont = new(new FontFamily("Microsoft Sans Serif"), 8f);

    }
}


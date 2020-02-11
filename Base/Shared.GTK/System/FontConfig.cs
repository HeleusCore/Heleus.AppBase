using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Heleus.Apps.Shared
{
    public static class FontConfig
    {
        const string _libfontconfig = "libfontconfig.so.1";

		[DllImport(_libfontconfig)]
        public static extern IntPtr FcConfigGetCurrent();
		[DllImport(_libfontconfig)]
		public static extern int FcConfigAppFontAddFile(IntPtr config, [MarshalAs(UnmanagedType.LPStr)]string fontPath);

		public static bool AddFontFromFile(FileInfo fontFile)
		{
			if (!fontFile.Exists)
				return false;

			var currentConfig = FcConfigGetCurrent();
			return FcConfigAppFontAddFile(currentConfig, fontFile.FullName) > 0;
		}
    }
}


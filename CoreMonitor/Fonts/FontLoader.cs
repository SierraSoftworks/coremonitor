using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;

namespace CoreMonitor.Fonts
{
    /// <summary>
    /// This class provides functions to load font files from
    /// embeded resources at runtime.
    /// </summary>
    /// <remarks>
    /// These functions are useful should proprietary fonts be
    /// in use and can be used to remove the need for fonts to
    /// be installed by the user prior to running the application.
    /// </remarks>
    public static class FontLoader
    {
        public static PrivateFontCollection Fonts { get; set; }

        /// <summary>
        /// Loads a set of fonts into temporary memory at runtime.
        /// </summary>
        /// <param name="resourceNames">
        /// An array of resource names that specify the fonts that
        /// should be loaded
        /// </param>
        public static void LoadFonts(string[] resourceNames)
        {
            Fonts = new PrivateFontCollection();

            foreach (string res in resourceNames)
            {
                Stream resStream = Assembly.GetCallingAssembly().GetManifestResourceStream(res);
                if (resStream == null)
                    return;
                byte[] resData = new byte[resStream.Length];
                resStream.Read(resData, 0, resData.Length);
                resStream.Close();

                IntPtr memLoc = Marshal.AllocCoTaskMem(resData.Length);
                Marshal.Copy(resData, 0, memLoc, resData.Length);

                Fonts.AddMemoryFont(memLoc, resData.Length);
                Marshal.FreeCoTaskMem(memLoc);
            }
        }

        /// <summary>
        /// Loads a set of fonts into temporary memory at runtime from
        /// the current assembly's resources.
        /// </summary>
        /// <remarks>
        /// This function loads any resources who's file names end with
        /// .ttf
        /// </remarks>
        public static void LoadFonts()
        {
            string[] resources = Assembly.GetCallingAssembly().GetManifestResourceNames();

            List<string> fontResources = new List<string>();
            foreach (string str in resources)
                if (str.EndsWith(".ttf"))
                    fontResources.Add(str);

            LoadFonts(fontResources.ToArray());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Citrullia.Library
{
    /// <summary>
    /// Utility for files.
    /// </summary>
    internal static class FileUtilities
    {
        /// <summary>The number format to be used.</summary>
        internal static NumberFormatInfo NumberFormat = new NumberFormatInfo() { NumberDecimalSeparator = ".", NumberGroupSeparator = "," };
        
        /// <summary>
        /// Generate a filename for the file. 
        /// </summary>
        /// <param name="directory">The directory of the file.</param>
        /// <param name="baseFilename">The base filename.</param>
        /// <param name="extension">The file extension without the period (.).</param>
        /// <returns>The unique generated filename.</returns>
        internal static string GenerateFileName(string directory, string baseFilename, string extension)
        {
            string filename = Path.Combine(directory, baseFilename);
            if (File.Exists(string.Format("{0}.{1}", filename, extension)))
            {
                int i = 1;
                while (File.Exists(string.Format("{0}.{1}", filename, extension)))
                {
                    filename = string.Format("{0}_{1}", filename, i);
                    i++;
                }
            }

            return string.Format("{0}.{1}", filename, extension);
        }

    }
}

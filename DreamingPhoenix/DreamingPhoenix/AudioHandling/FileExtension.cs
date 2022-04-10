using System;
using System.Collections.Generic;
using System.Text;

namespace WizHat.DreamingPhoenix.AudioHandling
{
    public class FileExtension
    {
        /// <summary>
        /// Extension name without the dot
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Creates a new file extension
        /// </summary>
        /// <param name="extension"></param>
        public FileExtension(string extension)
        {
            Extension = extension;
        }

        /// <summary>
        /// Get FileDialog extension filter text
        /// </summary>
        /// <param name="extensions">All extensions</param>
        /// <returns>FileDialog extenions filter</returns>
        public static string GetDialogExtensions(List<FileExtension> extensions)
        {
            string extensionStr = "";
            for (int i = 0; i < extensions.Count; i++)
            {
                extensionStr += string.Format("*.{0}", extensions[i].Extension);
                if (i < extensions.Count - 1)
                    extensionStr += ";";
            }

            return extensionStr;
        }

        /// <summary>
        /// Check if the path ends with a valid file extension
        /// </summary>
        /// <param name="extensions">All extensions</param>
        /// <param name="filePath">The file path</param>
        /// <returns>if true, the path ends with a valid extension</returns>
        public static bool EndsWith(List<FileExtension> extensions, string filePath)
        {
            foreach (FileExtension item in extensions)
            {
                if (filePath.EndsWith(item.Extension))
                    return true;
            }

            return false;
        }
    }
}

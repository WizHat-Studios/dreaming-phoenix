using System;
using System.Collections.Generic;
using System.Text;

namespace DreamingPhoenix.AudioHandling
{
    public class FileExtension
    {
        public string Extension { get; set; }

        public FileExtension(string extension)
        {
            Extension = extension;
        }

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

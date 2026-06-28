using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web;

namespace edocs.helper
{
    public static class PathSecurityHelper
    {

        /// <summary>
        /// Sanitizes a path component (filename or folder name) to prevent path traversal attacks.
        /// Strips directory information and removes invalid filename characters.
        /// </summary>
        public static string SanitizePathComponent(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            // Remove any directory paths - keep only the last segment
            input = Path.GetFileName(input);
            // Remove invalid filename characters
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                input = input.Replace(c.ToString(), "");
            }
            // Prevent "." and ".." as filenames
            if (input == "." || input == "..")
                input = "_invalid";
            return input;
        }
    }
}
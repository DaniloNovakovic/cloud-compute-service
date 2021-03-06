﻿using System.IO;

namespace Compute
{
    internal class FileIO : IFileIO
    {
        /// <summary>
        /// Copies folder from sourceFileName to destinationFileName (if folder is missing in
        /// destination file name then it will be created)
        /// </summary>
        /// <exception cref="IOException"></exception>
        /// <exception cref="System.UnauthorizedAccessException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public void CopyFile(string sourceFileName, string destFileName)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destFileName));
            File.Copy(sourceFileName, destFileName, overwrite: true);
        }

        /// <summary>
        /// Recursively deletes all files and folders inside the specified folder path
        /// </summary>
        /// <exception cref="IOException"></exception>
        /// <exception cref="System.UnauthorizedAccessException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public void ClearFolder(string path)
        {
            var root = new DirectoryInfo(path);
            foreach (var file in root.EnumerateFiles())
            {
                file.Delete();
            }
            foreach (var dir in root.EnumerateDirectories())
            {
                dir.Delete();
            }
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }
    }
}
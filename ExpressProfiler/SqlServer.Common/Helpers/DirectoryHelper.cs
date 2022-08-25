using System;
using System.IO;
using Express.Resources;

namespace Express.Helpers
{
    public static class DirectoryHelper
    {
        /// <summary>
        /// Creates directory path
        /// </summary>
        /// <param name="path"></param>
        /// <remarks>https://docs.microsoft.com/en-us/dotnet/api/system.io.directory.createdirectory?view=net-5.0</remarks>
        public static DirectoryInfo CreateDirectory(string path)
        {
            // Determine whether the directory exists.
            if (Directory.Exists(path))
            {
                return new DirectoryInfo(path);
            }

            // Try to create the directory.
            try
            {
                return Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return new DirectoryInfo(Path.GetTempPath());
        }

        /// <summary>
        /// This method returns resources files
        /// </summary>
        public static string GetResourcesFileContent(string fileName)
        {
            string resourcesFileName;
            if (ResourcesFileExists(fileName, out resourcesFileName))
            {
                return File.ReadAllText(resourcesFileName);
            }
            try
            {
                return ExpressResourceManager.ReadCommon(fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        /// <summary>
        /// This method returns resources files
        /// </summary>
        public static bool ResourcesFileExists(string fileName, out string fullName)
        {
            if (!Path.IsPathRooted(fileName))
            {
                var resourcefileFullName = GetResourceFileFullName(fileName);
                if (File.Exists(resourcefileFullName))
                {
                    FileInfo fi = new FileInfo(resourcefileFullName);
                    fullName = fi.FullName;
                    return true;
                }
                var fileFullName = GetFileFullName(fileName);
                if (File.Exists(fileFullName))
                {
                    FileInfo fi = new FileInfo(fileFullName);
                    fullName = fi.FullName;
                    return true;
                }
            }
            if (File.Exists(fileName))
            {
                FileInfo fi = new FileInfo(fileName);
                fullName = fi.FullName;
                return true;
            }
            fullName = null;
            return false;
        }

        public static string GetResourceFileFullName(string fileName)
        {
            if (fileName.StartsWith(@"Resources\"))
            {
                return GetFileFullName(fileName);
            }
            return GetFileFullName(Path.Combine("Resources", fileName));
        }

        public static string GetFileFullName(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }
    }
}

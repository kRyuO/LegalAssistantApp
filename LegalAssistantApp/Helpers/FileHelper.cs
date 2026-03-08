using System;
using System.IO;

namespace LegalAssistantApp.Helpers
{
    public static class FileHelper
    {
        public static string GetDocumentsRootFolder()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appData, "LegalAssistant", "Documents");

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            return appFolder;
        }

        public static string GetTempFolder()
        {
            var tempFolder = Path.Combine(Path.GetTempPath(), "LegalAssistant");

            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }

            return tempFolder;
        }

        public static string GetExportFolder()
        {
            var exportFolder = Path.Combine(GetDocumentsRootFolder(), "Exports");

            if (!Directory.Exists(exportFolder))
            {
                Directory.CreateDirectory(exportFolder);
            }

            return exportFolder;
        }

        public static bool IsValidFilePath(string path)
        {
            try
            {
                var fileName = Path.GetFileName(path);
                var directory = Path.GetDirectoryName(path);
                return !string.IsNullOrEmpty(fileName) && Directory.Exists(directory);
            }
            catch
            {
                return false;
            }
        }

        public static string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }
    }
}
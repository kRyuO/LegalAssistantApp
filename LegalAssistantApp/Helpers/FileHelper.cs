using System;
using System.IO;

namespace LegalAssistantApp.Helpers;

public static class FileHelper
{
    public static string GetDocumentsRootFolder()
    {
        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var root = Path.Combine(documents, "LegalAssistant");
        if (!Directory.Exists(root))
        {
            Directory.CreateDirectory(root);
        }

        return root;
    }

    public static string CombinePath(params string[] parts)
    {
        return Path.Combine(parts);
    }
}

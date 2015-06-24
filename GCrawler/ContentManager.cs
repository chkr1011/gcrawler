namespace GCrawler
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Web;

    internal static class ContentManager
    {
        private static readonly string ContentDirectory = AppDomain.CurrentDomain.BaseDirectory + "\\Content";
        private static readonly SHA1 _shaCalculator = SHA1.Create();
        
        static ContentManager()
        {
            if (!Directory.Exists(ContentManager.ContentDirectory))
            {
                Directory.CreateDirectory(ContentManager.ContentDirectory);
                Tracer.WriteInformation("Content directory created.");
            }
        }

        public static void SaveContent(Item item, Uri source)
        {
            // Check whether the content already exists with a different filename.
            string base64Hash;
            using (FileStream fileStream = File.OpenRead(item.TempFilename))
            {
                byte[] hash = ContentManager._shaCalculator.ComputeHash(fileStream);
                base64Hash = Convert.ToBase64String(hash);
                if (ContentManager.CheckHashIsInUse(base64Hash))
                {
                    return;
                }
            }

            // Generate a valid filename.
            string filename = "[" + base64Hash + "]_" + source.Segments.Last();
            filename = HttpUtility.UrlDecode(filename);
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(invalidChar, '_');
            }

            filename = Path.Combine(ContentManager.ContentDirectory, filename);

            try
            {
                Tracer.WriteVerbose("Saving content from '{0}' as '{1}'.", source, filename);
                File.Copy(item.TempFilename, filename);
                Tracer.WriteInformation("Content from '{0}' saved as '{1}'.", source, filename);
            }
            catch (Exception exception)
            {
                Tracer.WriteWarning("Could not save content from '{0}' as {1}'. {2}", source, filename, exception.Message);
            } 
        }

        private static bool CheckHashIsInUse(string base64Hash)
        {
            string pattern = string.Format("[{0}]*", base64Hash);
            return Directory.GetFiles(ContentManager.ContentDirectory, pattern, SearchOption.TopDirectoryOnly).Any();
        }
    }
}

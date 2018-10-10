using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SmartKioskBot.Helpers
{
    public class FileAsync
    {
        private const int DefaultBufferSize = 4096;
        
        private const FileOptions DefaultOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;

        public static Task<string> ReadAllTextAsync(string path)
        {
            return ReadAllTextAsync(path, Encoding.UTF8);
        }

        public static async Task<string> ReadAllTextAsync(string path, Encoding encoding)
        {
            var text = "";

            // Open the FileStream with the same FileMode, FileAccess
            // and FileShare as a call to File.OpenText would've done.
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultOptions))
            using (var reader = new StreamReader(stream, encoding))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    text += line;
                }
            }

            return text;
        }
    }
}
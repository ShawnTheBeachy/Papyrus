using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace Papyrus
{
    public static class EBookExtensions
    {
        public static async Task<bool> VerifyMimetypeAsync(this EBook ebook)
        {
            bool VerifyMimetypeString(string value) =>
                value == "application/epub+zip";

            if (ebook._rootFolder == null)                                      // Make sure a root folder was specified.
                return false;

            var mimetypeFile = await ebook._rootFolder.GetItemAsync("mimetype");

            if (mimetypeFile == null)                                           // Make sure file exists.
                return false;

            var fileContents = await FileIO.ReadTextAsync(mimetypeFile as StorageFile);

            if (!VerifyMimetypeString(fileContents))                         // Make sure file contents are correct.
                return false;

            return true;
        }
    }
}

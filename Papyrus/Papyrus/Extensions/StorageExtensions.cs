using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Papyrus
{
    internal static class StorageExtensions
    {
        internal static async Task<StorageFile> GetFileFromPathAsync(this StorageFolder folder, string path)
        {
            path = path.Replace('\\', '/');
            var pathParts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < pathParts.Length - 1; i++)
                folder = await folder.GetFolderAsync(pathParts[i]);

            return await folder.GetFileAsync(pathParts.Last());
        }
    }
}

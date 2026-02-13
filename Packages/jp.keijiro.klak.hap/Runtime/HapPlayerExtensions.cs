using UnityEngine;

namespace Klak.Hap
{
    /// <summary>
    /// Extension methods for HapPlayer to work with TextAsset (HAP files)
    /// </summary>
    public static class HapPlayerExtensions
    {
        /// <summary>
        /// Open a HAP video from a TextAsset (imported .hap file)
        /// This method writes the asset data to a temporary file and opens it
        /// </summary>
        /// <param name="player">The HapPlayer instance</param>
        /// <param name="hapAsset">The TextAsset containing the HAP video data</param>
        public static void Open(this HapPlayer player, TextAsset hapAsset)
        {
            if (hapAsset == null)
            {
                Debug.LogError("HAP TextAsset is null");
                return;
            }
            
            var tempFilePath = WriteToTemporaryFile(hapAsset);
            if (string.IsNullOrEmpty(tempFilePath))
            {
                Debug.LogError("Failed to write HAP TextAsset to temporary file");
                return;
            }
            
            player.Open(tempFilePath, HapPlayer.PathMode.LocalFileSystem);
        }
        
        /// <summary>
        /// Write the HAP TextAsset data to a temporary file and return the file path
        /// </summary>
        /// <param name="hapAsset">The TextAsset containing HAP data</param>
        /// <returns>Path to the temporary file, or null if failed</returns>
        public static string WriteToTemporaryFile(TextAsset hapAsset)
        {
            if (hapAsset == null || hapAsset.bytes == null || hapAsset.bytes.Length == 0)
                return null;
                
            var tempPath = System.IO.Path.Combine(Application.temporaryCachePath, hapAsset.name + ".mov");
            System.IO.File.WriteAllBytes(tempPath, hapAsset.bytes);
            return tempPath;
        }
    }
}
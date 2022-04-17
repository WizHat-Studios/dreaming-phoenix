using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WizHat.DreamingPhoenix.AudioHandling;
using WizHat.DreamingPhoenix.Data;

namespace WizHat.DreamingPhoenix.Cache
{
    public class CacheManager
    {
        private string IMAGE_CACHE_FOLDER_PATH { get; set; } = Path.Combine(AppContext.BaseDirectory, "/cache/images/");

        private static CacheManager instance;

        /// <summary>
        /// Instance of the Singleton Implementation
        /// </summary>
        public static CacheManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new CacheManager();

                return instance;
            }
        }

        public CacheManager()
        {
            Directory.CreateDirectory(IMAGE_CACHE_FOLDER_PATH);
        }

        public string GetNewCacheID()
        {
            string newID = Guid.NewGuid().ToString("n");
            return newID;
        }

        public void SaveImageToCache(BitmapSource source, string cacheID)
        {
            if (source == null)
                return;

            using (var fileStream = new FileStream(IMAGE_CACHE_FOLDER_PATH + cacheID, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(fileStream);
            }
        }

        public BitmapSource GetImageFromCache(string cacheID)
        {
            if (cacheID == null)
                return null;

            try
            {
                var uri = new Uri("file://" + IMAGE_CACHE_FOLDER_PATH + cacheID);
                return new BitmapImage(uri);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void CleanUpCacheID(string cacheID)
        {
            bool isCacheIDUsed = false;
            foreach (Scene scene in AppModel.Instance.SceneList)
            {
                if (scene.ImageCacheID == cacheID)
                    isCacheIDUsed = true;
            }

            if (!isCacheIDUsed)
            {
                try
                {
                    File.Delete(IMAGE_CACHE_FOLDER_PATH + cacheID);
                }
                catch (Exception) { }

            }
        }

        public void CleanUpCache()
        {
            string[] cachedFiles = Directory.GetFiles(IMAGE_CACHE_FOLDER_PATH);

            for (int i = 0; i < cachedFiles.Length; i++)
            {
                cachedFiles[i] = cachedFiles[i].Split('/').Last();
            }

            List<string> filesToDelete = cachedFiles.ToList();

            foreach (Scene scene in AppModel.Instance.SceneList)
            {
                filesToDelete.Remove(scene.ImageCacheID);
            }

            foreach (string file in filesToDelete)
            {
                try
                {
                    File.Delete(IMAGE_CACHE_FOLDER_PATH + file);
                }
                catch (Exception)
                {
                    // A file may be still in use or is blocked by something else idk
                }
            }
        }
    }
}

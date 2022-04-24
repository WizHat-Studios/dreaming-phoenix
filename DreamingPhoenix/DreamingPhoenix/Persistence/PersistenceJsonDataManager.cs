using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using WizHat.DreamingPhoenix.AudioHandling;
using WizHat.DreamingPhoenix.Data;

namespace WizHat.DreamingPhoenix.Persistence
{
    public class PersistenceJsonDataManager : IPersistenceDataManager
    {
        private readonly string configurationFileName = "DreamingPhoenix_UserData.json";
        private readonly string importedSceneFolderName = "imported_scene_audio";

        private string ConfigurationFilePath
        {
            get { return Path.Combine(AppContext.BaseDirectory, configurationFileName); }
        }

        private readonly JsonSerializerSettings jsonSerializerSettings = new() { TypeNameHandling = TypeNameHandling.Auto, PreserveReferencesHandling = PreserveReferencesHandling.Objects };

        public PersistentData Load()
        {
            if (!File.Exists(ConfigurationFilePath))
            {
                return new();
            }

            string fileContent = File.ReadAllText(ConfigurationFilePath);

            PersistentData data = new();

            try
            {
                data = JsonConvert.DeserializeObject<PersistentData>(fileContent, jsonSerializerSettings);
            }
            catch
            {
                Debug.WriteLine("Error occoured while reading JSON");
            }

            return data;
        }

        public bool Save(PersistentData dataToSave)
        {
            File.WriteAllText(ConfigurationFilePath, JsonConvert.SerializeObject(dataToSave, Formatting.Indented, jsonSerializerSettings));

            return true;
        }

        public async Task<bool> ExportScene(string exportFileNameWithPath, Scene sceneToExport)
        {
            Scene clonedScene = JsonConvert.DeserializeObject<Scene>(JsonConvert.SerializeObject(sceneToExport));

            List<string> filesToZip = new();

            if (clonedScene.SceneAudioTrack != null)
            {
                filesToZip.Add(clonedScene.SceneAudioTrack.AudioFile);
                clonedScene.SceneAudioTrack.AudioFile = Path.GetFileName(clonedScene.SceneAudioTrack.AudioFile);
            }

            foreach (SoundEffect sfx in clonedScene.SceneSoundEffects)
            {
                filesToZip.Add(sfx.AudioFile);
                sfx.AudioFile = Path.GetFileName(sfx.AudioFile);
            }

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        // Remove duplicates
                        filesToZip = filesToZip.Distinct().ToList();
                        foreach (var file in filesToZip)
                        {
                            var audioEntry = archive.CreateEntry(Path.GetFileName(file));

                            using (var entryStream = audioEntry.Open())
                            {
                                using (var fileStream = new FileStream(file, FileMode.Open))
                                {
                                    await fileStream.CopyToAsync(entryStream);
                                }
                            }
                        }

                        var sceneFile = archive.CreateEntry("scene.json");

                        using (var entryStream = sceneFile.Open())
                        using (var streamWriter = new StreamWriter(entryStream))
                        {
                            await streamWriter.WriteAsync(JsonConvert.SerializeObject(clonedScene, Formatting.Indented, jsonSerializerSettings));
                        }

                        if (sceneToExport.ImageSource != null)
                        {
                            using (MemoryStream imgStream = new MemoryStream())
                            {
                                BitmapEncoder encoder = new PngBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create((BitmapImage)sceneToExport.ImageSource));
                                encoder.Save(imgStream);
                                imgStream.Seek(0, SeekOrigin.Begin);

                                byte[] imageData = new byte[imgStream.Length];

                                var backgroundEntry = archive.CreateEntry("background.png");
                                imgStream.Read(imageData, 0, imageData.Length);
                                Stream bgStream = backgroundEntry.Open();
                                await bgStream.WriteAsync(imageData);

                            }
                        }
                    }

                    using (var fileStream = new FileStream(exportFileNameWithPath, FileMode.Create))
                    {
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        await memoryStream.CopyToAsync(fileStream);
                    }
                }

            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public async Task<Scene> PeekScene(string fileName)
        {
            Scene scene = null;

            using (var file = File.OpenRead(fileName))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
            {
                var sceneEntry = zip.GetEntry("scene.json");

                if (sceneEntry == null)
                    return null;

                StreamReader reader = new StreamReader(sceneEntry.Open());
                scene = JsonConvert.DeserializeObject<Scene>(reader.ReadToEnd());

                var backgroundEntry = zip.GetEntry("background.png");
                if (backgroundEntry != null)
                {
                    using (var zipStream = backgroundEntry.Open())
                    using (var memoryStream = new MemoryStream())
                    {
                        await zipStream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;

                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = memoryStream;
                        bitmap.EndInit();

                        scene.ImageSource = bitmap;
                    }
                }
            }


            return scene;
        }

        
        public async Task ImportScene(string packageFile, string saveDirectory)
        {
            Scene scene = null;

            if (saveDirectory == null)
            {
                saveDirectory = AppContext.BaseDirectory + "\\" + importedSceneFolderName;
                Directory.CreateDirectory(saveDirectory);
            }

            using (var file = File.OpenRead(packageFile))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
            {

                var sceneEntry = zip.GetEntry("scene.json");
                StreamReader reader = new StreamReader(sceneEntry.Open());
                scene = JsonConvert.DeserializeObject<Scene>(reader.ReadToEnd());

                var backgroundEntry = zip.GetEntry("background.png");
                if (backgroundEntry != null)
                {
                    using (var zipStream = backgroundEntry.Open())
                    using (var memoryStream = new MemoryStream())
                    {
                        await zipStream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;

                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = memoryStream;
                        bitmap.EndInit();

                        scene.ImageSource = bitmap;
                    }
                }

                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    // Background and scene don't need to be unpacked
                    if (entry.Name != "background.png" && entry.Name != "scene.json")
                    {
                        using (Stream zipStream = entry.Open())
                        using (FileStream fileStream = new FileStream(Path.Combine(saveDirectory, entry.Name), FileMode.Create))
                        {
                            await zipStream.CopyToAsync(fileStream);
                        }
                    }
                }

                // Change path of audios to match relative directory
                scene.SceneAudioTrack.AudioFile = saveDirectory + "\\" + scene.SceneAudioTrack.AudioFile;

                if (scene.SceneAudioTrack.AudioFile.StartsWith(AppContext.BaseDirectory))
                    scene.SceneAudioTrack.AudioFile = Path.GetRelativePath(AppContext.BaseDirectory, scene.SceneAudioTrack.AudioFile);


                AppModel.Instance.AudioList.Add(scene.SceneAudioTrack);

                foreach (SoundEffect sfx in scene.SceneSoundEffects)
                {
                    sfx.AudioFile = saveDirectory + "\\" + sfx.AudioFile;

                    if (sfx.AudioFile.StartsWith(AppContext.BaseDirectory))
                        sfx.AudioFile = Path.GetRelativePath(AppContext.BaseDirectory, sfx.AudioFile);
                    
                    AppModel.Instance.AudioList.Add(sfx);
                }
            }

            scene.ImageCacheID = Cache.CacheManager.Instance.GetNewCacheID();
            Cache.CacheManager.Instance.SaveImageToCache((BitmapImage)scene.ImageSource, scene.ImageCacheID);
            AppModel.Instance.SceneList.Add(scene);
        }
    }
}

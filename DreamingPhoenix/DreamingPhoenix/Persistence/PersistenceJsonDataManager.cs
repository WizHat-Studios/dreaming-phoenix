using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace WizHat.DreamingPhoenix.Persistence
{
    public class PersistenceJsonDataManager : IPersistenceDataManager
    {
        private readonly string configurationFileName = "DreamingPhoenix_UserData.json";
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
    }
}

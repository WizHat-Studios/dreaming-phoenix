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
        private readonly string ConfigurationFileName = "DreamingPhoenix_UserData.json";
        private JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto, PreserveReferencesHandling = PreserveReferencesHandling.Objects };

        public PersistentData Load()
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string configFilePath = assemblyPath + "\\" + ConfigurationFileName;

            if (!File.Exists(configFilePath))
            {
                return new PersistentData();
            }

            string fileContent = File.ReadAllText(configFilePath);

            PersistentData data = new PersistentData();

            try
            {
                data = JsonConvert.DeserializeObject<PersistentData>(fileContent, JsonSerializerSettings);
            }
            catch
            {
                Debug.WriteLine("Error occoured while reading JSON");
            }

            return data;
        }

        public bool Save(PersistentData dataToSave)
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            File.WriteAllText(assemblyPath + "\\" + ConfigurationFileName, JsonConvert.SerializeObject(dataToSave, Formatting.Indented, JsonSerializerSettings));

            return true;
        }
    }
}

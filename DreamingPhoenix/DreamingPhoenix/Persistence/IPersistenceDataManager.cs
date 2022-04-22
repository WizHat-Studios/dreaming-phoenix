using System;
using System.Collections.Generic;
using System.Text;
using WizHat.DreamingPhoenix.AudioHandling;

namespace WizHat.DreamingPhoenix.Persistence
{
    public interface IPersistenceDataManager
    {
        public PersistentData Load();
        public bool Save(PersistentData dataToSave);
        public bool ExportScene(string exportFileName, Scene sceneToExport);
        public Scene PeekScene(string packageFile);
        public void ImportScene(string packageFile, string saveDirectory);
    }
}

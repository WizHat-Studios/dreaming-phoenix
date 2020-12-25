using System;
using System.Collections.Generic;
using System.Text;

namespace DreamingPhoenix.Persistence
{
    public interface IPersistenceDataManager
    {
        public PersistentData Load();
        public bool Save(PersistentData dataToSave);
    }
}

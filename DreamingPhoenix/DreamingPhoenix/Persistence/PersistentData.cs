using System;
using System.Collections.Generic;
using System.Text;
using WizHat.DreamingPhoenix.AudioHandling;
using WizHat.DreamingPhoenix.Data;

namespace WizHat.DreamingPhoenix.Persistence
{
    public class PersistentData
    {
        public List<Audio> AudioList = new List<Audio>();
        public List<Scene> SceneList = new List<Scene>();
        public AppOptions AppOptions = new AppOptions();
        public WindowOptions WindowOptions = new WindowOptions();
    }
}

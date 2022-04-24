using System;
using System.Collections.Generic;
using System.Text;
using WizHat.DreamingPhoenix.AudioHandling;
using WizHat.DreamingPhoenix.AudioProperties;
using WizHat.DreamingPhoenix.Data;

namespace WizHat.DreamingPhoenix.Persistence
{
    public class PersistentData
    {
        public List<Category> Categories = new();
        public List<Audio> AudioList = new();
        public List<Scene> SceneList = new();
        public AppOptions AppOptions = new();
        public WindowOptions WindowOptions = new();
    }
}

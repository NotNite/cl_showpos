using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace cl_showpos {
    [Serializable]
    public class Configuration : IPluginConfiguration {
        public int Version { get; set; } = 0;

        public bool DrawShowpos { get; set; } = true;

        [NonSerialized] private DalamudPluginInterface? pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface) {
            this.pluginInterface = pluginInterface;
        }

        public void Save() {
            pluginInterface!.SavePluginConfig(this);
        }
    }
}

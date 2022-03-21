using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Numerics;

namespace cl_showpos {
    [Serializable]
    public class Configuration : IPluginConfiguration {
        public int Version { get; set; } = 0;

        public bool DrawShowpos { get; set; } = true;
        public Vector4 ShowposColor { get; set; } = new(1, 1, 1, 1);
        public float FontSize { get; set; } = 1;

        [NonSerialized] private DalamudPluginInterface? pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface) {
            this.pluginInterface = pluginInterface;
        }

        public void Save() {
            pluginInterface!.SavePluginConfig(this);
        }
    }
}

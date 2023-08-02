using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Numerics;

namespace cl_showpos;

public enum ShowposPosition {
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

[Serializable]
public class Configuration : IPluginConfiguration {
    public int Version { get; set; } = 0;

    public bool DrawShowpos = true;
    public Vector4 ShowposColor = new(1, 1, 1, 1);
    public float FontSize = 1;
    public ShowposPosition Position = ShowposPosition.TopLeft;

    public int OffsetX = 0;
    public int OffsetY = 0;

    public bool DrawName = true;
    public bool DrawTerritory = false;
    public bool DrawLongTerritory = false;
    public bool DrawTerritoryName = false;
    public bool DrawMapCoords = false;

    public int PositionPrecision = 3;

    [NonSerialized] private DalamudPluginInterface pluginInterface = null!;
    public void Initialize(DalamudPluginInterface pi) {
        this.pluginInterface = pi;
    }

    public void Save() {
        pluginInterface.SavePluginConfig(this);
    }
}

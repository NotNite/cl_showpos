using Dalamud.Configuration;
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

    public bool ShowWhenNoPlayer = true;
    public bool SourceAuthenticity = true;

    public int PositionPrecision = 3;

    public void Save() {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}

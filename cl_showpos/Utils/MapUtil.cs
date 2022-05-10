using System.Numerics;
using Lumina.Excel.GeneratedSheets;

namespace cl_showpos.Utils; 

public static class MapUtil {
    public static float ConvertMapXZPositionToCoordinate(float scale, double offset, float value) {
        var sizeFactor = scale / 100f;
        
        return (float) (10 - ((value - -offset) * sizeFactor + 1024f) * -0.2f / sizeFactor) / 10;
    }
    
    public static float ConvertMapYPositionToCoordinate(Map map, float value) {
        var transientSheet = Plugin.DataManager.Excel.GetSheet<TerritoryTypeTransient>();
        var zOffset = transientSheet?.GetRow(map.TerritoryType.Row)?.OffsetZ ?? 0;

        // Technically breaks game specification, but this brings things a bit more inline with what
        // a user would expect to see.
        if (zOffset == -10000) {
            zOffset = 0;
        }
        
        var adjustedValue = value - zOffset;

        if (adjustedValue < 0)
            adjustedValue -= 10;
        
        return adjustedValue / 100f;
    }

    public static Vector3 ToGameMinimapCoords(this Vector3 position, Map map) {
        return new Vector3(
            ConvertMapXZPositionToCoordinate(map.SizeFactor, map.OffsetX, position.X),
            ConvertMapXZPositionToCoordinate(map.SizeFactor, map.OffsetY, position.Z),
            ConvertMapYPositionToCoordinate(map, position.Y));
    }
}
using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;

namespace cl_showpos.Windows;

public class ShowposWindow : Window {
    private string nextStr = string.Empty;

    public ShowposWindow() : base("cl_showpos") {
        this.Flags =
            ImGuiWindowFlags.NoDecoration |
            ImGuiWindowFlags.NoInputs |
            ImGuiWindowFlags.NoBackground |
            ImGuiWindowFlags.NoBringToFrontOnFocus |
            ImGuiWindowFlags.NoFocusOnAppearing;

        this.DisableWindowSounds = true;
        this.RespectCloseHotkey = false;

        this.PositionCondition = ImGuiCond.Always;
        this.SizeCondition = ImGuiCond.Always;

        this.IsOpen = true;
    }

    public override void PreDraw() {
        this.nextStr = this.CreateString();

        // Calc it in the using block so the font & size is accurate
        Vector2 windowSize;
        using (ImRaii.PushFont(UiBuilder.MonoFont)) {
            var padding = ImGui.GetStyle().WindowPadding * 2;
            windowSize = (ImGui.CalcTextSize(this.nextStr) * Plugin.Configuration.FontSize) + padding;
        }
        this.Size = windowSize / ImGuiHelpers.GlobalScale;

        var viewport = ImGui.GetMainViewport();
        var windowPos = viewport.Pos;
        var screenSize = viewport.Size;

        windowPos += Plugin.Configuration.Position switch {
            ShowposPosition.TopLeft => new Vector2(0, 0),
            ShowposPosition.TopRight => new Vector2(screenSize.X - windowSize.X, 0),
            ShowposPosition.BottomLeft => new Vector2(0, screenSize.Y - windowSize.Y),
            ShowposPosition.BottomRight => new Vector2(screenSize.X - windowSize.X, screenSize.Y - windowSize.Y),
            _ => windowPos
        };
        windowPos += new Vector2(Plugin.Configuration.OffsetX, Plugin.Configuration.OffsetY);
        this.Position = windowPos;
    }

    public override void Draw() {
        using (ImRaii.PushFont(UiBuilder.MonoFont)) {
            using (ImRaii.PushColor(ImGuiCol.Text, Plugin.Configuration.ShowposColor)) {
                ImGui.SetWindowFontScale(Plugin.Configuration.FontSize);
                ImGui.TextUnformatted(this.nextStr);
            }
        }
    }

    private string CreateString() {
        var localPlayer = Plugin.ClientState.LocalPlayer;
        var zero = 0.ToString($"F{Plugin.Configuration.PositionPrecision}");
        var str = "";

        if (localPlayer is null) {
            if (!Plugin.Configuration.ShowWhenNoPlayer) return string.Empty;

            if (Plugin.Configuration.DrawName) str += "name: unconnected\n";
            str += $"pos: {zero} {zero} {zero}\n";
            if (Plugin.Configuration.DrawMapCoords) str += $"crd: {zero} {zero} {zero}\n";
            str += Plugin.Configuration.SourceAuthenticity
                       ? $"ang: {zero} {zero} {zero}\n"
                       : $"ang: {zero}\n";
            str += $"vel: {zero}\n";

            if (Plugin.Configuration.DrawTerritory) {
                var teri = Plugin.Configuration.DrawLongTerritory
                               ? "teri: null (0)"
                               : "teri: 0";
                str += $"{teri}\n";

                if (Plugin.Configuration.DrawTerritoryName) str += "tern: ??? > ???\n";
            }
        } else {
            var territoryId = Plugin.ClientState.TerritoryType;
            var territory = Plugin.TerritoryType.GetRow(territoryId)!;
            var map = Plugin.Map.GetRow(Utils.CurrentMapId());

            if (Plugin.Configuration.DrawName) str += $"name: {localPlayer.Name.TextValue}\n";

            var pos = localPlayer.Position.ToString(Plugin.Configuration.PositionPrecision);
            str += $"pos: {pos}\n";

            if (Plugin.Configuration.DrawMapCoords) {
                if (map is not null) {
                    var ttc = Plugin.TerritoryTypeTransient.GetRow(map.TerritoryType.Row);
                    var mapCoords = MapUtil.WorldToMap(
                        localPlayer.Position, map.OffsetX, map.OffsetY,
                        ttc?.OffsetZ ?? 0, map.SizeFactor, true);
                    var crd = mapCoords.ToString(Plugin.Configuration.PositionPrecision, true);
                    str += $"crd: {crd}\n";
                } else {
                    str += $"crd: {zero} {zero} {zero}\n";
                }
            }

            var angVal =
                (localPlayer.Rotation * (180 / Math.PI)).ToString($"F{Plugin.Configuration.PositionPrecision}");
            var ang = Plugin.Configuration.SourceAuthenticity
                          ? $"{zero} {angVal} {zero}"
                          : $"{angVal}";
            str += $"ang: {ang}\n";

            var updateFreq = 1000 / Plugin.Framework.UpdateDelta.Milliseconds;
            var velVec = (Plugin.LastPosition - Plugin.CurrentPosition) * updateFreq;
            var vel = velVec.Length().ToString($"F{Plugin.Configuration.PositionPrecision}");
            str += $"vel: {vel}\n";

            if (Plugin.Configuration.DrawTerritory) {
                var teri = Plugin.Configuration.DrawLongTerritory
                               ? $"{territory.Bg} ({territoryId})"
                               : $"{territoryId}";
                str += $"teri: {teri}\n";

                if (Plugin.Configuration.DrawTerritoryName) {
                    var region = territory.PlaceNameRegion.Value?.Name?.ToDalamudString().TextValue ?? "???";
                    var place = territory.PlaceName.Value?.Name?.ToDalamudString().TextValue ?? "???";
                    var tern = $"{region} > {place}";
                    str += $"tern: {tern}\n";
                }
            }
        }

        return str;
    }
}

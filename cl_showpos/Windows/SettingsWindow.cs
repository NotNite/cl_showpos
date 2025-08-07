using System;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;

namespace cl_showpos.Windows;

public class SettingsWindow : Window {
    public SettingsWindow() : base("cl_showpos settings") {
        this.Flags = ImGuiWindowFlags.AlwaysAutoResize;
    }

    public override void Draw() {
        // Plugin.Configuration.Save(); my beloathed

        if (ImGui.Checkbox("Enabled", ref Plugin.Configuration.DrawShowpos))
            Plugin.Configuration.Save();

        if (ImGui.Checkbox("Draw name", ref Plugin.Configuration.DrawName))
            Plugin.Configuration.Save();

        if (ImGui.Checkbox("Show territory ID", ref Plugin.Configuration.DrawTerritory))
            Plugin.Configuration.Save();

        var usingObj = Plugin.Configuration.DrawTerritory ? null : ImRaii.Disabled();
        using (usingObj) {
            using (ImRaii.PushIndent()) {
                if (ImGui.Checkbox("Show path", ref Plugin.Configuration.DrawLongTerritory))
                    Plugin.Configuration.Save();

                if (ImGui.Checkbox("Show name", ref Plugin.Configuration.DrawTerritoryName))
                    Plugin.Configuration.Save();
            }
        }

        if (ImGui.Checkbox("Show map coordinates", ref Plugin.Configuration.DrawMapCoords))
            Plugin.Configuration.Save();

        if (ImGui.ColorEdit4("Text color", ref Plugin.Configuration.ShowposColor, ImGuiColorEditFlags.NoInputs))
            Plugin.Configuration.Save();

        if (ImGui.SliderFloat("Font scale", ref Plugin.Configuration.FontSize, 0.1f, 10f))
            Plugin.Configuration.Save();

        var posNames = Enum.GetNames<ShowposPosition>();
        var pos = (int) Plugin.Configuration.Position;
        if (ImGui.Combo("Position", ref pos, posNames, posNames.Length)) {
            Plugin.Configuration.Position = (ShowposPosition) pos;
            Plugin.Configuration.Save();
        }

        Span<int> offset = [Plugin.Configuration.OffsetX, Plugin.Configuration.OffsetY];
        if (ImGui.InputInt("Offset", offset)) {
            Plugin.Configuration.OffsetX = offset[0];
            Plugin.Configuration.OffsetY = offset[1];
            Plugin.Configuration.Save();
        }

        if (ImGui.SliderInt("Precision", ref Plugin.Configuration.PositionPrecision, 0, 8))
            Plugin.Configuration.Save();

        if (ImGui.Checkbox("Source Engine authenticity", ref Plugin.Configuration.SourceAuthenticity))
            Plugin.Configuration.Save();

        if (ImGui.Checkbox("Show in loading/title screen", ref Plugin.Configuration.ShowWhenNoPlayer))
            Plugin.Configuration.Save();
    }
}

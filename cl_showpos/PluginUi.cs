using System;
using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;
using cl_showpos;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace cl_showpos;

internal class PluginUi : IDisposable {
    public const ImGuiWindowFlags ShowposFlags =
        ImGuiWindowFlags.NoDecoration |
        ImGuiWindowFlags.NoInputs |
        ImGuiWindowFlags.NoBackground |
        ImGuiWindowFlags.NoBringToFrontOnFocus |
        ImGuiWindowFlags.NoFocusOnAppearing;

    private Configuration configuration = Plugin.Configuration;

    private ExcelSheet<TerritoryTypeTransient> transientSheet =
        Plugin.DataManager.Excel.GetSheet<TerritoryTypeTransient>()!;

    private bool settingsVisible = false;

    public bool SettingsVisible {
        get => settingsVisible;
        set => settingsVisible = value;
    }

    public void Dispose() { }

    public void Draw() {
        DrawShowpos();
        DrawSettingsWindow();
    }

    private void DrawShowpos() {
        if (!this.configuration.DrawShowpos) return;

        // it is midnight can you tell
        var localPlayer = Plugin.ClientState.LocalPlayer;
        var str = "";

        var zero = 0.ToString($"F{this.configuration.PositionPrecision}");

        if (localPlayer == null) {
            if (this.configuration.DrawName) {
                str += "name: unconnected\n";
            }

            str += $"pos: {zero} {zero} {zero}";
            if (this.configuration.DrawMapCoords) str += $"\ncrd: {zero} {zero} {zero}";
            str += $"\nang: {zero} {zero} {zero}";
            str += $"\nvel: {zero}";
        } else {
            var territoryId = Plugin.ClientState.TerritoryType;
            var territory = Plugin.DataManager.GetExcelSheet<TerritoryType>()!.GetRow(territoryId)!;
            var map = Plugin.DataManager.GetExcelSheet<Map>()!.GetRow(cl_showpos.Utils.CurrentMapId());

            // name
            if (this.configuration.DrawName) {
                var nameStr = "name: " + localPlayer.Name + "\n";
                str += nameStr;
            }

            // pos
            var posStr = "pos: " + localPlayer.Position.ToString(this.configuration.PositionPrecision);
            str += posStr;

            // crd
            if (this.configuration.DrawMapCoords && map != null) {
                var ttc = this.transientSheet.GetRow(map.TerritoryType.Row);
                var mapCoords = Dalamud.Utility.MapUtil.WorldToMap(
                    localPlayer.Position, map.OffsetX, map.OffsetY,
                    ttc?.OffsetZ ?? 0, map.SizeFactor, true);

                str += $"\ncrd: {mapCoords.ToString(this.configuration.PositionPrecision, true)}";
            }

            // ang
            var ang = (localPlayer.Rotation * (180 / Math.PI)).ToString($"F{this.configuration.PositionPrecision}");
            var angStr = $"\nang: {zero} {ang} {zero}";
            str += angStr;

            // vel
            var updateFreq = 1000 / Plugin.Framework.UpdateDelta.Milliseconds;
            var vel = (Plugin.LastPosition - localPlayer.Position) * updateFreq;
            var velLen = vel.Length().ToString($"F{this.configuration.PositionPrecision}");
            var velStr = $"\nvel: {velLen}";
            str += velStr;

            // teri
            if (this.configuration.DrawTerritory) {
                if (this.configuration.DrawLongTerritory) {
                    str += $"\nteri: {territory.Bg} ({Plugin.ClientState.TerritoryType})";
                } else {
                    str += $"\nteri: {Plugin.ClientState.TerritoryType}";
                }

                if (this.configuration.DrawTerritoryName) {
                    str += $"\ntern: {territory.PlaceNameRegion.Value?.Name} > " +
                           $"{territory.PlaceName.Value?.Name}";
                }
            }
        }

        ImGui.PushFont(UiBuilder.MonoFont);
        ImGui.PushStyleColor(ImGuiCol.Text, this.configuration.ShowposColor);

        // calc font size
        var windowSize = ImGui.CalcTextSize(str) * this.configuration.FontSize + new Vector2(25, 25);
        ImGui.SetNextWindowSize(windowSize);

        // change window position
        var viewport = ImGui.GetMainViewport();
        var windowPos = viewport.Pos;
        var screenSize = viewport.Size;

        windowPos += this.configuration.Position switch {
            ShowposPosition.TopLeft => new Vector2(0, 0),
            ShowposPosition.TopRight => new Vector2(screenSize.X - windowSize.X, 0),
            ShowposPosition.BottomLeft => new Vector2(0, screenSize.Y - windowSize.Y),
            ShowposPosition.BottomRight => new Vector2(screenSize.X - windowSize.X, screenSize.Y - windowSize.Y),
            _ => windowPos
        };

        windowPos += new Vector2(this.configuration.OffsetX, this.configuration.OffsetY);

        ImGui.SetNextWindowPos(windowPos);

        if (ImGui.Begin("##cl_showpos", ShowposFlags)) {
            ImGui.SetWindowFontScale(this.configuration.FontSize);
            ImGui.TextUnformatted(str);
        }

        ImGui.PopStyleColor();
        ImGui.PopFont();
        ImGui.End();
    }

    private void DrawSettingsWindow() {
        if (!SettingsVisible) return;

        ImGui.SetNextWindowSize(new Vector2(250, 350) * ImGuiHelpers.GlobalScale, ImGuiCond.Appearing);
        if (ImGui.Begin("cl_showpos settings", ref settingsVisible)) {
            var enabled = this.configuration.DrawShowpos;
            if (ImGui.Checkbox("Enabled", ref enabled)) {
                this.configuration.DrawShowpos = enabled;
                this.configuration.Save();
            }

            var drawName = this.configuration.DrawName;
            if (ImGui.Checkbox("Draw name", ref drawName)) {
                this.configuration.DrawName = drawName;
                this.configuration.Save();
            }

            var drawTeri = this.configuration.DrawTerritory;
            if (ImGui.Checkbox("Show territory ID", ref drawTeri)) {
                this.configuration.DrawTerritory = drawTeri;
                this.configuration.Save();
            }

            if (this.configuration.DrawTerritory) {
                ImGui.Indent();

                var drawLongTeri = this.configuration.DrawLongTerritory;
                if (ImGui.Checkbox("Show path", ref drawLongTeri)) {
                    this.configuration.DrawLongTerritory = drawLongTeri;
                    this.configuration.Save();
                }

                var drawTeriName = this.configuration.DrawTerritoryName;
                if (ImGui.Checkbox("Show name", ref drawTeriName)) {
                    this.configuration.DrawTerritoryName = drawTeriName;
                    this.configuration.Save();
                }

                ImGui.Unindent();
            }

            var drawCrd = this.configuration.DrawMapCoords;
            if (ImGui.Checkbox("Show map coordinates", ref drawCrd)) {
                this.configuration.DrawMapCoords = drawCrd;
                this.configuration.Save();
            }

            var color = this.configuration.ShowposColor;
            if (ImGui.ColorEdit4("Text color", ref color, ImGuiColorEditFlags.NoInputs)) {
                this.configuration.ShowposColor = color;
                this.configuration.Save();
            }

            var size = this.configuration.FontSize;
            if (ImGui.SliderFloat("Font size", ref size, 0, 10)) {
                this.configuration.FontSize = size;
                this.configuration.Save();
            }

            var pos = (int) this.configuration.Position;
            var posNames = Enum.GetNames<ShowposPosition>();
            if (ImGui.Combo("Position", ref pos, posNames, posNames.Length)) {
                this.configuration.Position = (ShowposPosition) pos;
                this.configuration.Save();
            }

            // https://github.com/mellinoe/ImGui.NET/issues/181 ???
            var wtfImguiNet = new[] {this.configuration.OffsetX, this.configuration.OffsetY};
            if (ImGui.InputInt2("Offset", ref wtfImguiNet[0])) {
                this.configuration.OffsetX = wtfImguiNet[0];
                this.configuration.OffsetY = wtfImguiNet[1];
                this.configuration.Save();
            }

            var precision = this.configuration.PositionPrecision;
            if (ImGui.SliderInt("Precision", ref precision, 0, 8)) {
                this.configuration.PositionPrecision = precision;
                this.configuration.Save();
            }
        }

        ImGui.End();
    }
}

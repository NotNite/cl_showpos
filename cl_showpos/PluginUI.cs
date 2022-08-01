using System;
using System.Numerics;
using cl_showpos.Game;
using cl_showpos.Utils;
using Dalamud.Interface;
using Dalamud.Logging;
using ImGuiNET;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace cl_showpos {
    internal class PluginUI : IDisposable {
        private readonly Configuration configuration;

        public const ImGuiWindowFlags ShowposFlags =
            ImGuiWindowFlags.NoDecoration |
            ImGuiWindowFlags.NoInputs |
            ImGuiWindowFlags.NoBackground |
            ImGuiWindowFlags.NoBringToFrontOnFocus |
            ImGuiWindowFlags.NoFocusOnAppearing;

        private bool settingsVisible = false;

        private ExcelSheet<TerritoryTypeTransient> _transientSheet;

        public bool SettingsVisible {
            get => settingsVisible;
            set => settingsVisible = value;
        }

        public PluginUI() {
            this.configuration = Plugin.Configuration;
            this._transientSheet = Plugin.DataManager.Excel.GetSheet<TerritoryTypeTransient>()!;
        }

        public void Dispose() {
        }

        public void Draw() {
            DrawShowpos();
            DrawSettingsWindow();
        }

        private void DrawShowpos() {
            if (!configuration.DrawShowpos) return;

            // it is midnight can you tell
            var localPlayer = Plugin.ClientState.LocalPlayer;
            var str = "";

            var zero = 0.ToString($"F{configuration.PositionPrecision}");

            if (localPlayer == null) {
                if (configuration.DrawName) {
                    str += "name: unconnected\n";
                }
                
                str += $"pos: {zero} {zero} {zero}";
                if (configuration.DrawMapCoords) str += $"\ncrd: {zero} {zero} {zero}";
                str += $"\nang: {zero} {zero} {zero}";
                str += $"\nvel: {zero}";
            } else {
                var territoryId = Plugin.ClientState.TerritoryType;
                var territory = Plugin.DataManager.GetExcelSheet<TerritoryType>()!.GetRow(territoryId)!;
                var map = Plugin.DataManager.GetExcelSheet<Map>()!.GetRow(MapHelper.CurrentMapId());

                // name
                if (configuration.DrawName) {
                    var nameStr = "name: " + localPlayer.Name + "\n";
                    str += nameStr;
                }

                // pos
                var posStr = "pos: " + localPlayer.Position.ToString(configuration.PositionPrecision);
                str += posStr;

                // crd
                if (configuration.DrawMapCoords && map != null) {
                    var ttc = _transientSheet.GetRow(map.TerritoryType.Row);

                    var mapCoords = Dalamud.Utility.MapUtil.WorldToMap(localPlayer.Position, map.OffsetX, map.OffsetY,
                        ttc?.OffsetZ ?? 0, map.SizeFactor, true);

                    str += $"\ncrd: {mapCoords.ToString(configuration.PositionPrecision, true)}";
                }

                // ang
                var ang = (localPlayer.Rotation * (180 / Math.PI)).ToString($"F{configuration.PositionPrecision}");
                var angStr = $"\nang: {zero} {ang} {zero}";
                str += angStr;

                // vel
                var updateFreq = 1000 / Plugin.Framework.UpdateDelta.Milliseconds;
                var vel = (Plugin.LastPosition - localPlayer.Position) * updateFreq;
                var velLen = vel.Length().ToString($"F{configuration.PositionPrecision}");
                var velStr = $"\nvel: {velLen}";
                str += velStr;

                // teri
                if (configuration.DrawTerritory) {
                    if (configuration.DrawLongTerritory) {
                        str += $"\nteri: {territory.Bg} ({Plugin.ClientState.TerritoryType})";
                    } else {
                        str += $"\nteri: {Plugin.ClientState.TerritoryType}";
                    }

                    if (configuration.DrawTerritoryName) {
                        str += $"\ntern: {territory.PlaceNameRegion.Value?.Name} > " +
                               $"{territory.PlaceName.Value?.Name}";
                    }
                }
            }

            ImGui.PushFont(UiBuilder.MonoFont);
            ImGui.PushStyleColor(ImGuiCol.Text, configuration.ShowposColor);

            // calc font size
            var windowSize = ImGui.CalcTextSize(str) * configuration.FontSize + new Vector2(25, 25);
            ImGui.SetNextWindowSize(windowSize);

            // change window position
            var windowPos = new Vector2(0, 0);
            var screenSize = ImGui.GetIO().DisplaySize;

            switch (configuration.Position) {
                case ShowposPosition.TopLeft:
                    windowPos = new Vector2(0, 0);
                    break;
                case ShowposPosition.TopRight:
                    windowPos = new Vector2(screenSize.X - windowSize.X, 0);
                    break;
                case ShowposPosition.BottomLeft:
                    windowPos = new Vector2(0, screenSize.Y - windowSize.Y);
                    break;
                case ShowposPosition.BottomRight:
                    windowPos = new Vector2(screenSize.X - windowSize.X, screenSize.Y - windowSize.Y);
                    break;
            }

            windowPos += new Vector2(configuration.OffsetX, configuration.OffsetY);

            ImGui.SetNextWindowPos(windowPos);

            if (ImGui.Begin("##cl_showpos", ShowposFlags)) {
                ImGui.SetWindowFontScale(configuration.FontSize);
                ImGui.TextUnformatted(str);
            }

            ImGui.PopStyleColor();
            ImGui.PopFont();
            ImGui.End();
        }

        private void DrawSettingsWindow() {
            if (!SettingsVisible) return;

            ImGui.SetNextWindowSize(new Vector2(250, 350) * ImGuiHelpers.GlobalScale, ImGuiCond.Always);
            if (ImGui.Begin("cl_showpos settings", ref settingsVisible,
                    ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                    ImGuiWindowFlags.NoScrollWithMouse)) {
                var enabled = configuration.DrawShowpos;
                if (ImGui.Checkbox("Enabled", ref enabled)) {
                    configuration.DrawShowpos = enabled;
                    configuration.Save();
                }

                var drawName = configuration.DrawName;
                if (ImGui.Checkbox("Draw name", ref drawName)) {
                    configuration.DrawName = drawName;
                    configuration.Save();
                }

                var drawTeri = configuration.DrawTerritory;
                if (ImGui.Checkbox("Show territory ID", ref drawTeri)) {
                    configuration.DrawTerritory = drawTeri;
                    configuration.Save();
                }

                if (configuration.DrawTerritory) {
                    ImGui.Indent();

                    var drawLongTeri = configuration.DrawLongTerritory;
                    if (ImGui.Checkbox("Show path", ref drawLongTeri)) {
                        configuration.DrawLongTerritory = drawLongTeri;
                        configuration.Save();
                    }

                    var drawTeriName = configuration.DrawTerritoryName;
                    if (ImGui.Checkbox("Show name", ref drawTeriName)) {
                        configuration.DrawTerritoryName = drawTeriName;
                        configuration.Save();
                    }

                    ImGui.Unindent();
                }

                var drawCrd = configuration.DrawMapCoords;
                if (ImGui.Checkbox("Show map coordinates", ref drawCrd)) {
                    configuration.DrawMapCoords = drawCrd;
                    configuration.Save();
                }

                var color = configuration.ShowposColor;
                if (ImGui.ColorEdit4("Text color", ref color, ImGuiColorEditFlags.NoInputs)) {
                    configuration.ShowposColor = color;
                    configuration.Save();
                }

                var size = configuration.FontSize;
                if (ImGui.SliderFloat("Font size", ref size, 0, 10)) {
                    configuration.FontSize = size;
                    configuration.Save();
                }

                var pos = (int)configuration.Position;
                var posNames = Enum.GetNames<ShowposPosition>();
                if (ImGui.Combo("Position", ref pos, posNames, posNames.Length)) {
                    configuration.Position = (ShowposPosition)pos;
                    configuration.Save();
                }

                // https://github.com/mellinoe/ImGui.NET/issues/181 ???
                var wtfImguiNet = new[] { configuration.OffsetX, configuration.OffsetY };
                if (ImGui.InputInt2("Offset", ref wtfImguiNet[0])) {
                    configuration.OffsetX = wtfImguiNet[0];
                    configuration.OffsetY = wtfImguiNet[1];
                    configuration.Save();
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
}

using System;
using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;

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

        public bool SettingsVisible {
            get => settingsVisible;
            set => settingsVisible = value;
        }

        public PluginUI(Configuration configuration) {
            this.configuration = configuration;
        }

        public void Dispose() { }

        public void Draw() {
            DrawShowpos();
            DrawSettingsWindow();
        }

        private string Vec3ToString(Vector3 vec) {
            return $"{vec.X:0.00} {vec.Y:0.00} {vec.Z:0.00}";
        }

        private void DrawShowpos() {
            if (!configuration.DrawShowpos) return;

            // it is midnight can you tell
            var localPlayer = Plugin.ClientState.LocalPlayer;
            var str = "";

            if (localPlayer == null) {
                str += "name: unconnected";
                str += "\npos: 0.00 0.00 0.00";
                str += "\nang: 0.00 0.00 0.00";
                str += "\nvel: 0.00";
            } else {
                var nameStr = "name: " + localPlayer.Name;
                str += nameStr;

                var posStr = "\npos: " + Vec3ToString(localPlayer.Position);
                str += posStr;

                var ang = localPlayer.Rotation * (180 / Math.PI);
                var angStr = $"\nang: 0.00 {ang:0.00} 0.00";
                str += angStr;

                var updateFreq = 1000 / Plugin.Framework.UpdateDelta.Milliseconds;
                var vel = (Plugin.LastPosition - localPlayer.Position) * updateFreq;
                var velStr = $"\nvel: {vel.Length():0.00}";
                str += velStr;
            }

            if (configuration.DrawTerritory) str += $"\nteri: {Plugin.ClientState.TerritoryType}";

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

            ImGui.SetNextWindowSize(new Vector2(250, 200) * ImGuiHelpers.GlobalScale, ImGuiCond.Always);
            if (ImGui.Begin("cl_showpos settings", ref settingsVisible,
                    ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                    ImGuiWindowFlags.NoScrollWithMouse)) {
                var enabled = configuration.DrawShowpos;
                if (ImGui.Checkbox("Enabled", ref enabled)) {
                    configuration.DrawShowpos = enabled;
                    configuration.Save();
                }

                var drawTeri = configuration.DrawTerritory;
                if (ImGui.Checkbox("Show territory ID", ref drawTeri)) {
                    configuration.DrawTerritory = drawTeri;
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

                var pos = (int) configuration.Position;
                var posNames = Enum.GetNames<ShowposPosition>();
                if (ImGui.Combo("Position", ref pos, posNames, posNames.Length)) {
                    configuration.Position = (ShowposPosition) pos;
                    configuration.Save();
                }

                // https://github.com/mellinoe/ImGui.NET/issues/181 ???
                var wtfImguiNet = new[] {configuration.OffsetX, configuration.OffsetY};
                if (ImGui.InputInt2("Offset", ref wtfImguiNet[0])) {
                    configuration.OffsetX = wtfImguiNet[0];
                    configuration.OffsetY = wtfImguiNet[1];
                    configuration.Save();
                }
            }

            ImGui.End();
        }
    }
}

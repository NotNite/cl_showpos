using ImGuiNET;
using System;
using System.Numerics;
using Dalamud.Interface;

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
            string str;

            if (localPlayer == null) {
                str = "name: unconnected\npos: 0.00 0.00 0.00\nang: 0.00 0.00 0.00\nvel: 0.00";
            } else {
                var nameStr = "name: " + localPlayer.Name;
                var posStr = "pos: " + Vec3ToString(localPlayer.Position);

                var ang = localPlayer.Rotation * (180 / Math.PI);
                var angStr = $"ang: 0.00 {ang:0.00} 0.00";

                var updateFreq = 1000 / Plugin.Framework.UpdateDelta.Milliseconds;
                var vel = (Plugin.LastPosition - localPlayer.Position) * updateFreq;
                var velStr = $"vel: {vel.Length():0.00}";

                str = $"{nameStr}\n{posStr}\n{angStr}\n{velStr}";
            }
            
            ImGui.PushFont(UiBuilder.MonoFont);
            ImGui.PushStyleColor(ImGuiCol.Text, configuration.ShowposColor);

            // calc font size
            var windowSize = ImGui.CalcTextSize(str) * configuration.FontSize + new Vector2(25, 25);

            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.SetNextWindowSize(windowSize);

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

            ImGui.SetNextWindowSize(new Vector2(250, 125), ImGuiCond.Always);
            if (ImGui.Begin("cl_showpos settings", ref settingsVisible,
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse)) {
                var enabled = configuration.DrawShowpos;
                if (ImGui.Checkbox("Enabled", ref enabled)) {
                    configuration.DrawShowpos = enabled;
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
            }

            ImGui.End();
        }
    }
}

using ImGuiNET;
using System;
using System.Numerics;
using Dalamud.Game.ClientState;
using Dalamud.Interface;
using Dalamud.IoC;
using Dalamud.Logging;

namespace cl_showpos {
    internal class PluginUI : IDisposable {
        private readonly Configuration configuration;

        // thanks NoTankYou lmao
        // https://github.com/MidoriKami/NoTankYou/blob/master/NoTankYou/DisplaySystem/WarningBanner.cs#L29
        public const ImGuiWindowFlags ShowposFlags =
            ImGuiWindowFlags.NoScrollbar |
            ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoResize |
            ImGuiWindowFlags.NoBackground |
            ImGuiWindowFlags.NoBringToFrontOnFocus |
            ImGuiWindowFlags.NoFocusOnAppearing |
            ImGuiWindowFlags.NoNavFocus |
            ImGuiWindowFlags.NoInputs;

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

                str = $"{nameStr}\n{posStr}\n{angStr}\nvel: {vel.Length():0.00}";
            }

            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.SetNextWindowSize(new Vector2(500, 500));
            if (ImGui.Begin("##cl_showpos", ShowposFlags)) {
                ImGui.PushFont(UiBuilder.MonoFont);
                ImGui.TextUnformatted(str);
                ImGui.PopFont();
            }

            ImGui.End();
        }

        private void DrawSettingsWindow() {
            if (!SettingsVisible) return;

            ImGui.SetNextWindowSize(new Vector2(250, 75), ImGuiCond.Always);
            if (ImGui.Begin("cl_showpos settings", ref settingsVisible,
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse)) {
                var configValue = configuration.DrawShowpos;
                if (ImGui.Checkbox("Enabled", ref configValue)) {
                    configuration.DrawShowpos = configValue;
                    configuration.Save();
                }
            }

            ImGui.End();
        }
    }
}

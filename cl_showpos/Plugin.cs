using System.Numerics;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;

namespace cl_showpos {
    public sealed class Plugin : IDalamudPlugin {
        public string Name => "cl_showpos";
        private const string CommandName = "/pshowpos";

        [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static CommandManager CommandManager { get; private set; } = null!;
        [PluginService] public static ClientState ClientState { get; private set; } = null!;
        [PluginService] public static Framework Framework { get; private set; } = null!;
        [PluginService] public static DataManager DataManager { get; private set; } = null!;

        public static Configuration Configuration { get; private set; } = null!;
        public static Vector3 LastPosition = Vector3.Zero;

        private PluginUi pluginUi = null!;

        public Plugin() {
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);

            // cl_showpos was made before Dalamud windowing, so it's still using this method
            // ...but given that it's a widget on the top of the screen, who cares?
            this.pluginUi = new PluginUi();

            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand) {
                HelpMessage = "Open the settings menu"
            });

            PluginInterface.UiBuilder.Draw += this.DrawUi;
            PluginInterface.UiBuilder.OpenConfigUi += this.DrawConfigUi;

            Framework.Update += OnFrameworkUpdate;
        }

        public void Dispose() {
            this.pluginUi.Dispose();
            CommandManager.RemoveHandler(CommandName);
        }

        private void OnFrameworkUpdate(Framework framework) {
            if (ClientState.LocalPlayer != null) {
                LastPosition = ClientState.LocalPlayer.Position;
            }
        }

        private void OnCommand(string command, string args) {
            this.pluginUi.SettingsVisible = true;
        }

        private void DrawUi() {
            this.pluginUi.Draw();
        }

        private void DrawConfigUi() {
            this.pluginUi.SettingsVisible = true;
        }
    }
}

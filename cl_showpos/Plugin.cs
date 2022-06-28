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

        private const string commandName = "/pshowpos";

        // you can tell i don't write C#
        [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; }
        [PluginService] public static CommandManager CommandManager { get; private set; }
        [PluginService] public static ClientState ClientState { get; private set; }
        [PluginService] public static Framework Framework { get; private set; }
        [PluginService] public static DataManager DataManager { get; private set; }

        public static Configuration Configuration { get; private set; }
        private PluginUI PluginUI { get; }

        public static Vector3 LastPosition = Vector3.Zero;

        public Plugin() {
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);

            PluginUI = new PluginUI();

            CommandManager.AddHandler(commandName, new CommandInfo(OnCommand) {
                HelpMessage = "Open the settings menu"
            });

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            Framework.Update += OnFrameworkUpdate;
        }

        public void Dispose() {
            PluginUI.Dispose();
            CommandManager.RemoveHandler(commandName);
        }

        private void OnFrameworkUpdate(Framework framework) {
            if (ClientState.LocalPlayer != null) {
                LastPosition = ClientState.LocalPlayer.Position;
            }
        }

        private void OnCommand(string command, string args) {
            PluginUI.SettingsVisible = true;
        }

        private void DrawUI() {
            PluginUI.Draw();
        }

        private void DrawConfigUI() {
            PluginUI.SettingsVisible = true;
        }
    }
}

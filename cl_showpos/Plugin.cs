using System.Numerics;
using cl_showpos.Windows;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace cl_showpos;

public sealed class Plugin : IDalamudPlugin {
    public string Name => "cl_showpos";
    private const string CommandName = "/pshowpos";

    [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static CommandManager CommandManager { get; private set; } = null!;
    [PluginService] public static ClientState ClientState { get; private set; } = null!;
    [PluginService] public static Framework Framework { get; private set; } = null!;
    [PluginService] public static DataManager DataManager { get; private set; } = null!;

    public static Configuration Configuration = null!;

    public static ExcelSheet<TerritoryType> TerritoryType = null!;
    public static ExcelSheet<TerritoryTypeTransient> TerritoryTypeTransient = null!;
    public static ExcelSheet<Map> Map = null!;

    public static Vector3 LastPosition = Vector3.Zero;
    public static Vector3 CurrentPosition = Vector3.Zero;

    private WindowSystem windowSystem;
    private ShowposWindow showposWindow;
    private SettingsWindow settingsWindow;

    public Plugin() {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        TerritoryType = DataManager.GetExcelSheet<TerritoryType>()!;
        TerritoryTypeTransient = DataManager.GetExcelSheet<TerritoryTypeTransient>()!;
        Map = DataManager.GetExcelSheet<Map>()!;

        this.windowSystem = new("cl_showpos");

        this.showposWindow = new();
        this.settingsWindow = new();

        this.windowSystem.AddWindow(this.showposWindow);
        this.windowSystem.AddWindow(this.settingsWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand) {
            HelpMessage = "Open the settings menu"
        });

        PluginInterface.UiBuilder.Draw += this.DrawUi;
        PluginInterface.UiBuilder.OpenConfigUi += this.DrawConfigUi;
        Framework.Update += OnFrameworkUpdate;
    }

    public void Dispose() {
        this.windowSystem.RemoveAllWindows();
        CommandManager.RemoveHandler(CommandName);
        PluginInterface.UiBuilder.Draw -= this.DrawUi;
        PluginInterface.UiBuilder.OpenConfigUi -= this.DrawConfigUi;
        Framework.Update -= OnFrameworkUpdate;
    }

    private void OnFrameworkUpdate(Framework framework) {
        if (ClientState.LocalPlayer != null) {
            LastPosition = CurrentPosition;
            CurrentPosition = ClientState.LocalPlayer.Position;
        }
    }

    private void OnCommand(string command, string args) {
        this.DrawConfigUi();
    }

    private void DrawUi() {
        this.windowSystem.Draw();
    }

    private void DrawConfigUi() {
        this.settingsWindow.IsOpen ^= true;
    }
}

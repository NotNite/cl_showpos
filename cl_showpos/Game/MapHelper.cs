using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace cl_showpos.Game;

public static unsafe class MapHelper {
    public static uint CurrentMapId() {
        return AgentMap.Instance()->CurrentMapId;
    }
}

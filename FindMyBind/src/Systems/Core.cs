using HarmonyLib;
using Vintagestory.API.Common;

namespace FindMyBind;

public class Core : ModSystem
{
    private Harmony HarmonyInstance => new Harmony(Mod.Info.ModID);

    public override void StartPre(ICoreAPI api)
    {
        HarmonyInstance.PatchAll();
        api.Logger.Event("started '{0}' mod", Mod.Info.Name);
    }

    public override void Dispose()
    {
        HarmonyInstance.UnpatchAll(HarmonyInstance.Id);
    }
}

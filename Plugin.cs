using BepInEx;

namespace DivineRelicWithModdedWeapons
{
    [BepInPlugin("DivineRelicModdedWeapons", "Divine Relic with Modded Weapons", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            new DivineRelicModdedWeapons().Init();
            Logger.LogInfo($"Divine Relice with Modded Weapons is loaded!");
        }
    }
}

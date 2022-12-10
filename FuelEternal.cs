using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace FuelEternal
{
    class PluginInfo
    {
        public const string Name = "FuelEternal";
        public const string Guid = "Marfinator." + Name;
        public const string Version = "1.2.0";
    }

    [BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
    [BepInProcess("valheim.exe")]

    public class FuelEternal : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony(PluginInfo.Guid);
        private static ConfigEntry<bool> fe_fire_pit;
        private static ConfigEntry<bool> fe_bonfire;
        private static ConfigEntry<bool> fe_hearth;
        private static ConfigEntry<bool> fe_piece_walltorch;
        private static ConfigEntry<bool> fe_piece_groundtorch;
        private static ConfigEntry<bool> fe_piece_groundtorch_wood;
        private static ConfigEntry<bool> fe_piece_groundtorch_green;
        private static ConfigEntry<bool> fe_piece_groundtorch_blue;
        private static ConfigEntry<bool> fe_piece_brazierfloor01;
        private static ConfigEntry<bool> fe_piece_brazierceiling01;
        private static ConfigEntry<bool> fe_piece_jackoturnip;
        private static ConfigEntry<bool> fe_piece_oven;
        private static ConfigEntry<bool> fe_smelter;
        private static ConfigEntry<bool> fe_blastfurnace;
        private static ConfigEntry<bool> fe_eitrrefinery;
        private static ConfigEntry<bool> fe_piece_bathtub;
        private static ConfigEntry<string> fe_custom_instance;

        void Awake()
        {
            fe_fire_pit = Config.Bind<bool>("Fireplaces", "AllowCampFire", true, "Allow eternal fuel for Campfire");
            fe_bonfire = Config.Bind<bool>("Fireplaces", "AllowBonfire", true, "Allow eternal fuel for Bonfire");
            fe_hearth = Config.Bind<bool>("Fireplaces", "AllowHearth", true, "Allow eternal fuel for Hearth");
            fe_piece_walltorch = Config.Bind<bool>("Fireplaces", "AllowSconce", true, "Allow eternal fuel for Sconce");
            fe_piece_groundtorch = Config.Bind<bool>("Fireplaces", "AllowStandingIronTorch", true, "Allow eternal fuel for Standing iron torch");
            fe_piece_groundtorch_wood = Config.Bind<bool>("Fireplaces", "AllowStandingWoodTorch", true, "Allow eternal fuel for Standing wood torch");
            fe_piece_groundtorch_green = Config.Bind<bool>("Fireplaces", "AllowStandingGreenBurningIronTorch", true, "Allow eternal fuel for Standing green-burning iron torch");
            fe_piece_groundtorch_blue = Config.Bind<bool>("Fireplaces", "AllowStandingBlueBurningIronTorch", true, "Allow eternal fuel for Standing blue-burning iron torch");
            fe_piece_brazierfloor01 = Config.Bind<bool>("Fireplaces", "AllowStandingBrazier", true, "Allow eternal fuel for Standing brazier");
            fe_piece_brazierceiling01 = Config.Bind<bool>("Fireplaces", "AllowHangingBrazier", true, "Allow eternal fuel for Hanging brazier");
            fe_piece_jackoturnip = Config.Bind<bool>("Fireplaces", "AllowJackOTurnip", true, "Allow eternal fuel for Jack-o-turnip");
            fe_piece_oven = Config.Bind<bool>("CookingStations", "AllowStoneOven", true, "Allow eternal fuel for Stone oven");
            fe_piece_bathtub = Config.Bind<bool>("Smelters", "AllowHotTub", true, "Allow eternal fuel for Hot tub");
            fe_smelter = Config.Bind<bool>("Smelters", "AllowSmelter", false, "Allow eternal fuel for Smelter");
            fe_blastfurnace = Config.Bind<bool>("Smelters", "AllowBlastFurnace", false, "Allow eternal fuel for Blast furnace");
            fe_eitrrefinery = Config.Bind<bool>("Smelters", "AllowEitrRefinery", false, "Allow eternal fuel for Eitr refinery");
            fe_custom_instance = Config.Bind<string>("Custom", "CustomItems", "", "Enable Fuel Eternal to manage fuel for custom items added by other mods, comma-separated no spaces (e.g. \"rk_campfire,rk_hearth,rk_brazier\" )");

            harmony.PatchAll();
        }

        void OnDestroy()
        {
            harmony.UnpatchSelf();
        }

        static bool ConfigCheck(string instanceName)
        {
            bool EternalFuel = false;
            switch (instanceName)
            {
                case "fire_pit(Clone)":
                    EternalFuel = fe_fire_pit.Value;
                    break;
                case "bonfire(Clone)":
                    EternalFuel = fe_bonfire.Value;
                    break;
                case "hearth(Clone)":
                    EternalFuel = fe_hearth.Value;
                    break;
                case "piece_walltorch(Clone)":
                    EternalFuel = fe_piece_walltorch.Value;
                    break;
                case "piece_groundtorch(Clone)":
                    EternalFuel = fe_piece_groundtorch.Value;
                    break;
                case "piece_groundtorch_wood(Clone)":
                    EternalFuel = fe_piece_groundtorch_wood.Value;
                    break;
                case "piece_groundtorch_green(Clone)":
                    EternalFuel = fe_piece_groundtorch_green.Value;
                    break;
                case "piece_groundtorch_blue(Clone)":
                    EternalFuel = fe_piece_groundtorch_blue.Value;
                    break;
                case "piece_brazierfloor01(Clone)":
                    EternalFuel = fe_piece_brazierfloor01.Value;
                    break;
                case "piece_brazierceiling01(Clone)":
                    EternalFuel = fe_piece_brazierceiling01.Value;
                    break;
                case "piece_jackoturnip(Clone)":
                    EternalFuel = fe_piece_jackoturnip.Value;
                    break;
                case "piece_oven(Clone)":
                    EternalFuel = fe_piece_oven.Value;
                    break;
                case "smelter(Clone)":
                    EternalFuel = fe_smelter.Value;
                    break;
                case "blastfurnace(Clone)":
                    EternalFuel = fe_blastfurnace.Value;
                    break;
                case "eitrrefinery(Clone)":
                    EternalFuel = fe_eitrrefinery.Value;
                    break;
                case "piece_bathtub(Clone)":
                    EternalFuel = fe_piece_bathtub.Value;
                    break;
            }
            if (fe_custom_instance.Value.Split(',').Contains(instanceName.Remove(instanceName.Length - 7)))
                EternalFuel = true;
            return EternalFuel;
        }


        /*********************************************-Harmony Patches-*********************************************/
        [HarmonyPatch]
        class Fireplace_UpdateFireplace_Patch
        {
            [HarmonyPatch(typeof(Fireplace))]
            [HarmonyPatch("UpdateFireplace")]
            [HarmonyPrefix]
            static void Fireplace_UpdateFireplace(Fireplace __instance, ref ZNetView ___m_nview)
            {
                if (ConfigCheck(__instance.name))
                    ___m_nview.GetZDO().Set("fuel", __instance.m_maxFuel);
            }
        }

        [HarmonyPatch]
        class CookingStation_SetFuel_Patch
        {
            [HarmonyPatch(typeof(CookingStation))]
            [HarmonyPatch("SetFuel")]
            [HarmonyPrefix]
            static void CookingStation_SetFuel(CookingStation __instance, ref float fuel)
            {
                if (ConfigCheck(__instance.name))
                    fuel = __instance.m_maxFuel;
            }
        }

        [HarmonyPatch]
        class Smelter_SetFuel_Patch
        {
            [HarmonyPatch(typeof(Smelter))]
            [HarmonyPatch("SetFuel")]
            [HarmonyPrefix]
            static void Smelter_SetFuel(Smelter __instance, ref float fuel)
            {
                if (ConfigCheck(__instance.name))
                    fuel = __instance.m_maxFuel;
            }
        }

        [HarmonyPatch]
        class CookingStation_Awake_Patch
        {
            [HarmonyPatch(typeof(CookingStation))]
            [HarmonyPatch("Awake")]
            [HarmonyPostfix]
            static void CookingStation_Awake(CookingStation __instance, ref ZNetView ___m_nview)
            {
                if (!___m_nview.isActiveAndEnabled || Player.m_localPlayer == null || Player.m_localPlayer.IsTeleporting())
                    return;

                if (ConfigCheck(__instance.name))
                    Refuel(___m_nview);
            }
        }

        [HarmonyPatch]
        class Smelter_Awake_Patch
        {
            [HarmonyPatch(typeof(Smelter))]
            [HarmonyPatch("Awake")]
            [HarmonyPostfix]
            static void Smelter_Awake(Smelter __instance, ref ZNetView ___m_nview)
            {
                if (!___m_nview.isActiveAndEnabled || Player.m_localPlayer == null || Player.m_localPlayer.IsTeleporting())
                    return;

                if (ConfigCheck(__instance.name))
                    Refuel(___m_nview);
            }
        }

        public static async void Refuel(ZNetView znview)
        {
            await Task.Delay(33);
            znview.InvokeRPC("AddFuel");
        }

    }
}

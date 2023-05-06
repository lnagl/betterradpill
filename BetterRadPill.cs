using System;

using HarmonyLib;
using UnityEngine;
using Klei.AI;

using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using Newtonsoft.Json;

namespace RadPillThreshold
{
    public class Patches
    {
        [JsonObject(MemberSerialization.OptIn)]
        [ModInfo("https://github.com/antoine-fresse")]
        public class BetterRadPillsModSettings
        {
            [Option("Rad threshold", "How many rads before a dupe is allowed to consume a radpill (100/300/600 rads = minor/major/extreme sickness, 900 = incapacitated")]
            [Limit(0, 900)]
            [JsonProperty]
            public float Rads { get; set; }

            [Option("Faster animation", "Dupe ingest rad pills faster")]
            [JsonProperty]
            public bool FasterAnim { get; set; }

            public BetterRadPillsModSettings()
            {
                Rads = 33;
                FasterAnim = true;
            }
        }

        public sealed class BetterRadPills : KMod.UserMod2
        {
            public static BetterRadPillsModSettings Settings;

            public override void OnLoad(Harmony harmony)
            {
                base.OnLoad(harmony);
                PUtil.InitLibrary(false);
                new POptions().RegisterOptions(this, typeof(BetterRadPillsModSettings));
                Settings = POptions.ReadSettings<BetterRadPillsModSettings>();
                if (Settings == null)
                {
                    Settings = new BetterRadPillsModSettings();
                }
            }
        }

        [HarmonyPatch(typeof(MedicinalPillWorkable), "CanBeTakenBy")]
        public static class MedicinalPillWorkable_CanBeTakenBy_Patch
        {
            public static void Postfix(ref bool __result, ref MedicinalPill ___pill, GameObject consumer)
            {
                if (___pill.info.id == "BasicRadPill")
                {
                    if(consumer.GetAmounts().Get(Db.Get().Amounts.RadiationBalance.Id).value < BetterRadPills.Settings.Rads)
                    {
                        __result = false;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MedicinalPillWorkable), "OnSpawn")]
        public static class MedicinalPillWorkable_OnSpawn_Patch
        {
            public static void Postfix(ref MedicinalPillWorkable __instance)
            {
                if (__instance.pill.info.id == "BasicRadPill")
                {
                    if (BetterRadPills.Settings.FasterAnim)
                        __instance.SetWorkTime(1f);
                }
            }
        }

    }

    
}

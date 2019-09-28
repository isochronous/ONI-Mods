﻿using System;
using System.Collections.Generic;
using Database;
using Harmony;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;
using Zolibrary.Logging;
using Zolibrary.Config;
using Zolibrary.Utilities;

namespace DuplicantLifecycle
{
    public class DuplicantLifeCyclePatches
    {
        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
                LogManager.SetModInfo("DuplicantLifeCycle", "1.0.0");
                LogManager.LogInit();
            }
        }

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public class GeneratedBuildings_LoadGeneratedBuildings_Patch
        {
            public static void Prefix()
            {
                Trait aging_trait = Db.Get().CreateTrait(
                    id: (string) DuplicantLifecycleStrings.AgingID, 
                    name: (string) DuplicantLifecycleStrings.AgingNAME, 
                    description: (string) DuplicantLifecycleStrings.AgingDESC, 
                    group_name: null, 
                    should_save: true, 
                    disabled_chore_groups: null, 
                    positive_trait: true, 
                    is_valid_starter_trait: false
                    );
                aging_trait.OnAddTrait = (go => go.FindOrAddUnityComponent<Aging>());
                /*
                Trait immortal_trait = Db.Get().CreateTrait(
                    id: (string)DuplicantLifecycleStrings.ImmortalID,
                    name: (string)DuplicantLifecycleStrings.ImmortalNAME,
                    description: (string)DuplicantLifecycleStrings.ImmortalDESC,
                    group_name: null,
                    should_save: true,
                    disabled_chore_groups: null,
                    positive_trait: true,
                    is_valid_starter_trait: false
                    );
                immortal_trait.OnAddTrait = (go => go.FindOrAddUnityComponent<Immortal>());

                Traverse.Create<DUPLICANTSTATS>().Field("GENESHUFFLERTRAITS").SetValue(
                    new List<DUPLICANTSTATS.TraitVal>() {
                        new DUPLICANTSTATS.TraitVal() { id = "Regeneration" },
                        new DUPLICANTSTATS.TraitVal() { id = "DeeperDiversLungs" },
                        new DUPLICANTSTATS.TraitVal() { id = "SunnyDisposition" },
                        new DUPLICANTSTATS.TraitVal() { id = "RockCrusher" },
                        new DUPLICANTSTATS.TraitVal() { id = "Immortal" }
                    });

                #if DEBUG         
                    foreach ( DUPLICANTSTATS.TraitVal t in DUPLICANTSTATS.GENESHUFFLERTRAITS)
                        LogManager.LogDebug(t.id);
                #endif*/
            }
        }

        [HarmonyPatch(typeof(MinionBrain), "OnSpawn")]
        public class MinionBrain_OnSpawn_Patch
        {
            public static void Postfix(MinionBrain __instance)
            {
                Traits traits_component = __instance.GetComponent<Worker>().GetComponent<Traits>();
                Trait agingTrait = Db.Get().traits.TryGet((string)DuplicantLifecycleStrings.AgingID);

                #if DEBUG
                    LogManager.LogDebug("MinionBrain_OnSpawn: " + traits_component.ToString());
                #endif

                if (!traits_component.HasTrait(agingTrait.Id) && !traits_component.HasTrait(DuplicantLifecycleStrings.ImmortalID))
                    traits_component.Add(agingTrait);
            }
        }
    }
}
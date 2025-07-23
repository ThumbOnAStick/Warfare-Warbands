﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace WarfareAndWarbands
{
    public class WAWSettings : Verse.ModSettings
    {
        public static int occupyChance = 20;
        public static int establishFeeMultiplier = 2;
        public static int eventFrequency = 5;
        public static int maxPlayerWarband = 10;
        public static int raidPlayerWarbandChance = 15;
        public static int playerRaidExp = 5;
        public static int playerRaiddedExp = 5;
        public static int leaderUsedAbilityExp = 15;
        public static bool enableFactionDefeat = false;
        public static bool dropPodRaidRequiresUpgrade = false;
        public static bool everReadUpdateLog = false;
        public static bool upgradeRequiresLeader= true;
        public static bool enableEquipmentBudgetLimit = true;
        public static bool enableDroppodPolicyRequirement = true;
        public static bool enableEliteUpgradePolicyRequirement = true;
        public static float warbandRaidCooldown = 1.5f;
        public static float warbandRecruitTimeMultiplier = 1f;
        public static int townConstructionDuration= 10;
        public static int townConstructionSkillRequirement = 3;
        public static int townConstructionCost = 10000;

        private static readonly float UpdateVersion = 1.11f;


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref occupyChance, "occupyChance", 20);
            Scribe_Values.Look(ref establishFeeMultiplier, "establishFeeMultiplier", 2);
            Scribe_Values.Look(ref eventFrequency, "eventFrequency", 10);
            Scribe_Values.Look(ref maxPlayerWarband, "maxPlayerWarband", 10);
            Scribe_Values.Look(ref raidPlayerWarbandChance, "raidPlayerWarbandChance", 15);
            Scribe_Values.Look(ref enableFactionDefeat, "enableFactionDefeat", defaultValue: false);
            Scribe_Values.Look(ref everReadUpdateLog, $"everReadUpdateLog{UpdateVersion}", defaultValue: false);
            Scribe_Values.Look(ref warbandRaidCooldown, "warbandRaidCooldown", 1.5f);
            Scribe_Values.Look(ref warbandRecruitTimeMultiplier, "warbandRecruitTimeMultiplier", 1f);
            Scribe_Values.Look(ref upgradeRequiresLeader, "upgradeRequiresLeader", true);
            Scribe_Values.Look(ref enableEquipmentBudgetLimit, "enableEquipmentBudgetLimit", true);
            Scribe_Values.Look(ref enableDroppodPolicyRequirement, "enableDroppodPolicyRequirement", true);
            Scribe_Values.Look(ref enableEliteUpgradePolicyRequirement, "enableEliteUpgradePolicyRequirement", true);

        }

        public void DoWindowsContent(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            occupyChance = (int)listing_Standard.SliderLabeled("WAW.occupyChance".Translate(occupyChance), occupyChance, 0, 100, .5f, "WAW.occupyChance.Desc".Translate()); ;
            establishFeeMultiplier = (int)listing_Standard.SliderLabeled("WAW.establishFeeMultiplier".Translate(establishFeeMultiplier), establishFeeMultiplier, 1, 5, .5f, "WAW.establishFeeMultiplier.Desc".Translate(establishFeeMultiplier)); ;
            eventFrequency = (int)listing_Standard.SliderLabeled("WAW.eventFrequency".Translate(eventFrequency), eventFrequency, 5, 20, .5f, "WAW.eventFrequency.Desc".Translate(eventFrequency)); ;
            maxPlayerWarband = (int)listing_Standard.SliderLabeled("WAW.maxPlayerWarband".Translate(maxPlayerWarband), maxPlayerWarband, 3, 20, .5f, "WAW.maxPlayerWarband.Desc".Translate(maxPlayerWarband)); ;
            raidPlayerWarbandChance =
                (int)listing_Standard.SliderLabeled(
                    "WAW.raidPlayerWarbandChance".Translate(raidPlayerWarbandChance),
                    raidPlayerWarbandChance,
                    0, 
                    100, 
                    .5f,
                    "WAW.raidPlayerWarbandChance.Desc".Translate(raidPlayerWarbandChance)
                    ); ;
            warbandRaidCooldown = listing_Standard.SliderLabeled("WAW.warbandRaidCooldown".Translate(warbandRaidCooldown.ToString("0.0")), warbandRaidCooldown, 1, 10, .5f, "WAW.warbandRaidCooldown.Desc".Translate());
            warbandRecruitTimeMultiplier = listing_Standard.SliderLabeled("WAW.warbandRecruitTimeMultiplier".Translate(warbandRecruitTimeMultiplier.ToString("0.0")), warbandRecruitTimeMultiplier, .5f, 5f, .5f);
            listing_Standard.CheckboxLabeled("WAW.upgradeRequiresLeader".Translate(), ref upgradeRequiresLeader, "WAW.upgradeRequiresLeader.Desc".Translate()); ;
            listing_Standard.CheckboxLabeled("WAW.enableFactionDefeat".Translate(), ref enableFactionDefeat, "WAW.enableFactionDefeat.Desc".Translate());
            listing_Standard.CheckboxLabeled("WAW.enableEquipmentBudgetLimit".Translate(), ref enableEquipmentBudgetLimit, "WAW.enableEquipmentBudgetLimit.Desc".Translate()); 
            listing_Standard.CheckboxLabeled("WAW.enableDroppodPolicyRequirement".Translate(), ref enableDroppodPolicyRequirement, "WAW.enableDroppodPolicyRequirement.Desc".Translate()); 
            listing_Standard.CheckboxLabeled("WAW.enableEliteUpgradePolicyRequirement".Translate(), ref enableEliteUpgradePolicyRequirement, "WAW.enableEliteUpgradePolicyRequirement.Desc".Translate()); 
            listing_Standard.End();
            this.Write();
        }



    }
}

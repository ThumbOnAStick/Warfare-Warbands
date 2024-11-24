using RimWorld;
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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref occupyChance, "occupyChance",20);
            Scribe_Values.Look(ref establishFeeMultiplier, "establishFeeMultiplier", 2);
            Scribe_Values.Look(ref eventFrequency, "establishFeeMultiplier", 5);
            Scribe_Values.Look(ref enableFactionDefeat, "enableFactionDefeat", true);
            Scribe_Values.Look(ref warbandRaidCooldown, "warbandRaidCooldown", 1.5f);


        }

        public void DoWindowsContent(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            occupyChance = (int)listing_Standard.SliderLabeled("WAW.occupyChance".Translate(occupyChance), occupyChance, 0, 100, .5f, "WAW.occupyChance.Desc".Translate()); ;
            establishFeeMultiplier = (int)listing_Standard.SliderLabeled("WAW.establishFeeMultiplier".Translate(establishFeeMultiplier), establishFeeMultiplier, 1, 5, .5f, "WAW.establishFeeMultiplier.Desc".Translate(establishFeeMultiplier)); ;
            eventFrequency = (int)listing_Standard.SliderLabeled("WAW.eventFrequency".Translate(eventFrequency), eventFrequency, 1, 10, .5f, "WAW.eventFrequency.Desc".Translate(eventFrequency)); ;
            warbandRaidCooldown = (float)(((int)(listing_Standard.SliderLabeled("WAW.warbandRaidCooldown".Translate(warbandRaidCooldown), warbandRaidCooldown, 0, 10, .5f, "WAW.warbandRaidCooldown.Desc".Translate()) * 100)) / 100f);

            listing_Standard.CheckboxLabeled("WAW.enableFactionDefeat".Translate(), ref enableFactionDefeat, "WAW.enableFactionDefeat.Desc".Translate()); ;

            listing_Standard.End();
            this.Write();
        }

        public static int occupyChance = 20;
        public static int establishFeeMultiplier = 2;
        public static int eventFrequency = 5;
        public static bool enableFactionDefeat = false;
        public static float warbandRaidCooldown = 1.5f;
    }
}

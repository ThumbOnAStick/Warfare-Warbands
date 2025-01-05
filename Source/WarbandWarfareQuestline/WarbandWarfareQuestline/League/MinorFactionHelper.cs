using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WarbandWarfareQuestline.League
{
    public static class MinorFactionHelper
    {
        public static MinorFaction GenerateMinorFaction(FactionTraitDef trait, TechLevel level, Color factionColor)
        {
            MinorFaction minorFaction = new MinorFaction(trait, level, factionColor);
            minorFaction.Init();
            return minorFaction;
        }
    }
}

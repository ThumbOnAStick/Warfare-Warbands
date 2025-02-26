using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands.Warband;

namespace WarbandWarfareQuestline.Skirmish
{
    public class SkirmishWorker
    {
        public static Faction GetRandomHostileFaction()
        {
            return Find.FactionManager.AllFactions.Where(x => x.HostileTo(Faction.OfPlayer) && !x.Hidden).RandomElement();
        }

   


    }
}

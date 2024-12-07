using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace WarfareAndWarbands.Warband.PlayerWarbandRaid
{
    public static class PlayerWarbandRaidUtil
    {
        public static void RaidPlayer(Faction f, Warband playerWarband)
        {
            if (f == null)
            {
                f = Faction.OfMechanoids;
            }
            if (f == null)
            {
                return;
            }
            if (!playerWarband.CanPlayerWarbandBeRaidded())
            {
                return;
            }
     
            LongEventHandler.QueueLongEvent(delegate ()
            {
                var label = "WAW.RaidPlayerWarband".Translate();
                var desc = "WAW.RaidPlayerWarband.Desc".Translate(f.def.pawnsPlural, f.NameColored);
                Letter letter = LetterMaker.MakeLetter(label, desc, LetterDefOf.ThreatBig);
                Find.LetterStack.ReceiveLetter(letter);
                Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(playerWarband.Tile, WAWDefof.WAW_Warband);
                orGenerateMap.fogGrid.ClearAllFog();
                var cell = orGenerateMap.Center;
                CameraJumper.TryJump(cell, orGenerateMap);
                IncidentParms incidentParms = new IncidentParms
                {
                    target = orGenerateMap,
                    faction = f,
                    points = Math.Max(StorytellerUtility.DefaultThreatPointsNow(Find.AnyPlayerHomeMap), 500),
                    forced = true,
                    raidStrategy = RaidStrategyDefOf.ImmediateAttack,
                    canKidnap = false
                };
                //Decide Raid Delay
                int duration = 200;

                Find.Storyteller.incidentQueue.Add(IncidentDefOf.RaidEnemy, Find.TickManager.TicksGame + duration, incidentParms, 0);


            }, "GeneratingMapForNewEncounter", false, null, true);
    
        }
    }
}

using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands;
using RimWorld.BaseGen;

namespace WarbandWarfareQuestline.League
{
    public static class MinorFactionBaseUtil
    {
        public static void AttackNow(Caravan caravan, MapParent settlement)
        {
            bool num = !settlement.HasMap;
            Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(settlement.Tile, null);
            TaggedString letterLabel = "LetterLabelCaravanEnteredEnemyBase".Translate();
            TaggedString letterText = settlement.Faction != null?
                "LetterCaravanEnteredEnemyBase".Translate(caravan.Label, settlement.Label.ApplyTag(TagType.Settlement, settlement.Faction.GetUniqueLoadID())).CapitalizeFirst():
                new TaggedString("");
            if (num)
            {
                Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
            }
            Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent, caravan.PawnsListForReading, settlement.Faction);
            CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, CaravanDropInventoryMode.DoNotDrop, draftColonists: true);
        }

        private static bool IsHumanEnemy(Faction f) { return !f.Hidden && f.def.permanentEnemy && f.def.humanlikeFaction; }

        public static MinorFactionSettlement GenerateSettlement(this MinorFaction faction)
        {
            MinorFactionSettlement result = (MinorFactionSettlement)WorldObjectMaker.MakeWorldObject(WAWDefof.WAW_MinorFactionSettlement);
            TileFinder.TryFindNewSiteTile(out int tile, 20, 50);
            result.Tile = tile;
            result.Name = faction.NameForSettlement();
            result.SetMinorFaction(faction);
            Find.WorldObjects.Add(result);
            return result;
        }

        public static MinorFactionSettlement GenerateSettlementOccupied(this MinorFaction faction)
        {
            var result = faction.GenerateSettlement();
            if(!Find.FactionManager.AllFactions.Any(IsHumanEnemy))
            {
                return result;
            }
            var rndEnemy = Find.FactionManager.AllFactions.Where(IsHumanEnemy).RandomElement();
            result.SetFaction(rndEnemy);
            return result;
        }

        public static void SpawnCorpse(PawnKindDef pawnKindDef, IntVec3 spawnPosition, Map map)
        {
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKindDef, null, PawnGenerationContext.NonPlayer, -1, false, true, false, false, false, 0f, false, true, false, false, true, false, false, false, false, 0f, 0f, null, 0f, null, null, null, null, null, null, null, null, null, null, null, null, true, true, true, false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null, false, false, false, -1, 0, false));
            HealthUtility.DamageUntilDowned(pawn, true, null, null, null);
            if (!pawn.Dead)
            {
                pawn.Kill(null, null);
            }
            pawn.Corpse.Age = new IntRange(5000, 10000).RandomInRange;
            pawn.relations.hidePawnRelations = true;
            GenSpawn.Spawn(pawn.Corpse, spawnPosition, map, WipeMode.Vanish);
            pawn.Corpse.GetComp<CompRottable>().RotProgress += (float)pawn.Corpse.Age;
        }


    }

}

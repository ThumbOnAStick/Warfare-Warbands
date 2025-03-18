using RimWorld;
using RimWorld.Planet;
using System.Linq;
using Verse;

namespace WarbandWarfareQuestline.League.MinorFactions
{
    public static class MinorFactionHelper
    {
        public static MinorFaction GenerateRandomMinorFactionAndJoinPlayer()
        {
            return GenerateRandomMinorFactionAndJoinPlayer(UnityEngine.Random.Range(0f, 1f) > .5f ? TechLevel.Industrial : TechLevel.Neolithic, TileFinder.RandomStartingTile(), out _);
        }

        public static MinorFaction GenerateRandomMinorFactionAndJoinPlayer(TechLevel level, int tile, out WorldObject o)
        {
            var minorFaction = CreateMinorFaction(level);
            var settlement = minorFaction.GenerateSettlement(tile);
            settlement.SetFaction(Faction.OfPlayer);
            o = settlement;
            return minorFaction;
        }

        public static MinorFaction GenerateMinorFaction(FactionTraitDef trait, TechLevel level)
        {
            var minorFaction = new MinorFaction(trait, level);
            minorFaction.Init();
            GameComponent_League.Instance.FactionsTemp.Add(minorFaction);
            return minorFaction;
        }

        public static void JoinPlayer(this MinorFaction f)
        {
            GameComponent_League.Instance.JoinPlayer(f);
        }

        private static MinorFaction CreateMinorFaction(TechLevel level)
        {
            var trait = DefDatabase<FactionTraitDef>.GetRandom();
            var minorFaction = new MinorFaction(trait, level);
            minorFaction.Init();
            minorFaction.JoinPlayer();
            GameComponent_League.Instance.FactionsTemp.Add(minorFaction);
            return minorFaction;
        }
    }
}

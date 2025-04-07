using RimWorld;
using RimWorld.Planet;
using System.Linq;
using Verse;

namespace WarbandWarfareQuestline.League.MinorFactions
{
    public static class MinorFactionHelper
    {
        /// <summary>
        /// Generates a random minor faction and joins it to the player's league.
        /// </summary>
        /// <returns>The generated minor faction.</returns>
        public static MinorFaction GenerateRandomMinorFactionAndJoinPlayer()
        {
            return GenerateRandomMinorFactionOnTileAndJoinPlayer(
                UnityEngine.Random.Range(0f, 1f) > 0.5f ? TechLevel.Industrial : TechLevel.Neolithic,
                TileFinder.RandomStartingTile(),
                out _);
        }

        /// <summary>
        /// Generates a random minor faction on a specified tile and joins it to the player's league.
        /// </summary>
        /// <param name="level">The tech level of the minor faction.</param>
        /// <param name="tile">The tile on which to generate the minor faction.</param>
        /// <param name="worldObject">The generated world object.</param>
        /// <returns>The generated minor faction.</returns>
        public static MinorFaction GenerateRandomMinorFactionOnTileAndJoinPlayer(TechLevel level, int tile, out WorldObject worldObject)
        {
            var minorFaction = GenerateRandomMinorFactionAndJoinPlayer(level);
            var settlement = minorFaction.GenerateSettlement(tile);
            settlement.JoinPlayer();
            settlement.SetFaction(Faction.OfPlayer);
            worldObject = settlement;
            return minorFaction;
        }

        /// <summary>
        /// Generates a random minor factionwith specific trait
        /// </summary>
        /// <param name="trait"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static MinorFaction GenerateRandomMinorFaction(TechLevel level)
        {
            var trait = DefDatabase<FactionTraitDef>.GetRandom();
            return GenerateMinorFaction(trait, level);
        }

        /// <summary>
        /// Generates a minor faction with the specified trait and tech level.
        /// </summary>
        /// <param name="trait">The trait of the minor faction.</param>
        /// <param name="level">The tech level of the minor faction.</param>
        /// <returns>The generated minor faction.</returns>
        public static MinorFaction GenerateMinorFaction(FactionTraitDef trait, TechLevel level)
        {
            var minorFaction = new MinorFaction(trait, level);
            minorFaction.Init();
            return minorFaction;
        }

        /// <summary>
        /// Joins the specified minor faction settlement to the player's league.
        /// </summary>
        /// <param name="settlement">The minor faction settlement to join.</param>
        public static void JoinPlayer(this MinorFactionSettlement settlement)
        {
            GameComponent_League.Instance.JoinPlayer(settlement);
        }

        /// <summary>
        /// Generates a minor faction and joins it to the player's league.
        /// </summary>
        /// <param name="level">The tech level of the minor faction.</param>
        /// <returns>The generated minor faction.</returns>
        private static MinorFaction GenerateRandomMinorFactionAndJoinPlayer(TechLevel level)
        {
            var trait = DefDatabase<FactionTraitDef>.GetRandom();
            var minorFaction = new MinorFaction(trait, level);
            minorFaction.Init();
            return minorFaction;
        }
    }
}

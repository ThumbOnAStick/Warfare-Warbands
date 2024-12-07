using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarfareAndWarbands.Warband.PlayerWarbandRaid
{
    public class GameComponent_PlayerWarbandRaidManager : GameComponent
    {
        int lastRaidTicks;
        private static readonly int RaidPlayerDuration = GenDate.TicksPerDay * 5;

        public GameComponent_PlayerWarbandRaidManager(Game game)
        {
            lastRaidTicks = GenTicks.TicksGame;
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if(GenTicks.TicksGame - lastRaidTicks > RaidPlayerDuration)
            {
                lastRaidTicks = GenTicks.TicksGame;
                RaidRandomWarband();
            }
        }

        bool HasRaidTargets()
        {
            return Find.WorldObjects.MapParents.Any(x => x as Warband != null && x.Faction == Faction.OfPlayer);
        }

        IEnumerable<WorldObject> RaidTargets()
        {
            return Find.WorldObjects.MapParents.Where(x => x as Warband != null && x.Faction == Faction.OfPlayer);
        }

        void TryRaidAll(IEnumerable<WorldObject> warbands)
        {
            foreach(var warband in warbands)
            {
                if(TryRaid(warband))
                {
                    break;
                }
            }
        }

        bool TryRaid(WorldObject worldObject)
        {
            var warband = worldObject as Warband;
            if(warband == null)
            {
                return false;
            }
            int rnd = new IntRange(1, 100).RandomInRange;
            if(rnd < WAWSettings.raidPlayerWarbandChance)
            {
                var faction = WarfareUtil.GetValidHostileWarFactions().RandomElement();
                PlayerWarbandRaidUtil.RaidPlayer(faction, warband);
                return true;
            }
            return false;
        }

        void RaidRandomWarband()
        {
            if(HasRaidTargets())
            {
                var targets = RaidTargets();
                TryRaidAll(targets);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastRaidTicks, "lastRaidTicks", GenTicks.TicksGame);
        }

    }
}

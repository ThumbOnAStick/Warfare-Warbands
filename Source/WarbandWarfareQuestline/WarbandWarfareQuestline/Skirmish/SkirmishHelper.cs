using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands.Warband;
using static RimWorld.ColonistBar;

namespace WarbandWarfareQuestline.Skirmish
{
    public static class SkirmishHelper
    {

        public static Faction GetRandomHostileFaction()
        {
            
            return Find.FactionManager.AllFactions.Where(x => x.HostileTo(Faction.OfPlayer) && !x.Hidden).RandomElement();
        }

        public static Skirmish CreateRandomSkirmish()
        {
            int rndTile = TileFinder.RandomStartingTile();
            Skirmish result = CreateSkirmish(rndTile);
            result.NotifyPlayer();
            return result;
        }

        public static Siege CreateRandomSiege()
        {
            Siege result = CreateSiege();
            result.NotifyPlayer();
            return result;
        }



        public static Skirmish CreateSkirmish(GlobalTargetInfo info)
        {
            return CreateSkirmish(info.Tile);
        }

        static bool ValidateFaction(Faction fac) =>
               fac.AllyOrNeutralTo(Faction.OfPlayer)
               && !fac.Hidden
               && !fac.defeated
               && !fac.IsPlayer
               && !fac.temporary;

        public static Skirmish CreateSkirmish(int tile)
        {
            if (!Find.FactionManager.AllFactions.Any(ValidateFaction))
            {
                return null;
            }
            Faction ally = Find.FactionManager.AllFactions.Where(ValidateFaction).RandomElement();
            Faction enemy = GetRandomHostileFaction();
            List<Warband> warbands = new List<Warband>();
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    Warband hostileNpcWarband = RandomWarbandNear(enemy, tile);
                    Warband allyNpcWarband = RandomWarbandNear(ally, hostileNpcWarband.Tile);
                    allyNpcWarband.npcWarbandManager.SetTargetTile(hostileNpcWarband.Tile);
                    warbands.Add(hostileNpcWarband);
                    warbands.Add(allyNpcWarband);
                }
                catch (Exception ex)
                {
                    Log.Error($"WAW: Exception while generating skirmish: {ex}");
                }
            }

            Skirmish result = new Skirmish(warbands, GenTicks.TicksGame);
            return result;
        }
        public static Siege CreateSiege()
        {
           
            if (!Find.FactionManager.AllFactions.Any(ValidateFaction))
            {
                return null;
            }
            Faction ally = Find.FactionManager.AllFactions.Where(ValidateFaction).RandomElement();
            Faction enemy = GetRandomHostileFaction();
            WorldObject settlement = Find.World.worldObjects.AllWorldObjects.Where(x => x is Settlement && x.Faction == enemy).First();
            List<Warband> warbands = new List<Warband>();

            //Spawn Hostile Warband
            try
            {
                Warband hostileNpcWarband = RandomWarbandNear(enemy, settlement.Tile);
                hostileNpcWarband.npcWarbandManager.SetTargetTile(settlement.Tile);
                warbands.Add(hostileNpcWarband);
            }
            catch (Exception ex)
            {
                Log.Error($"WAW: Exception while generating skirmish: {ex}");
            }
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    //Spawn Ally warband
                    Warband allyNpcWarband = RandomWarbandNear(ally, settlement.Tile);
                    allyNpcWarband.npcWarbandManager.SetTargetTile(settlement.Tile);
                    warbands.Add(allyNpcWarband);
                }
                catch (Exception ex)
                {
                    Log.Error($"WAW: Exception while generating skirmish: {ex}");
                }
            }

            Siege result = new Siege(warbands, GenTicks.TicksGame, settlement);
            return result;
        }

        static Warband RandomWarbandNear(Faction f, int tile)
        {
            TileFinder.TryFindNewSiteTile(out int allyTile, 3, 7, false, TileFinderMode.Near, tile);
            Warband warband = WarbandUtil.SpawnWarband(f, allyTile);
            return warband;
        }
    }
}

using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarfareAndWarbands.Warband.WarbandRecruiting
{
    internal static class WarbandRecruitingUtil
    {

        public static bool SpawnRecruitingWarband(GlobalTargetInfo target)
        {
            return SpawnRecruitingWarband(target.Tile) != null;
        }
        public static WorldObject_WarbandRecruiting SpawnRecruitingWarband(int tile)
        {
            if (!WarbandUtil.CanPlaceMoreWarbands())
            {
                return null;
            }
            if (EmptyWarbandArrangement())
            {
                return null;
            }
            if (Find.World.Impassable(tile))
            {
                return null;
            }
            var warband = (WorldObject_WarbandRecruiting)WorldObjectMaker.MakeWorldObject(WAWDefof.WAW_WarbandRecruiting);
            warband.Tile = tile;
            warband.SetFaction(Faction.OfPlayer);
            Find.WorldObjects.Add(warband);
            return warband;
        }

        static bool EmptyWarbandArrangement()
        {
            return !GameComponent_WAW.playerWarband.bandMembers.Any(x => x.Value > 0);
        }

    }
}

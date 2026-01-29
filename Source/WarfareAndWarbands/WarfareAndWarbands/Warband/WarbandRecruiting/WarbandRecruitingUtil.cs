using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace WarfareAndWarbands.Warband.WarbandRecruiting
{
    internal static class WarbandRecruitingUtil
    {

        public static bool SpawnRecruitingWarband(GlobalTargetInfo target, Pawn leader = null)
        {
            var result = SpawnRecruitingWarband(target.Tile);
            if (leader != null)
                result?.AssignLeader(leader, leader.GetCaravan());
            return result != null;
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
            warband.SetColorOverride();
            Find.WorldObjects.Add(warband);
            WarbandUtil.TryToSendLeaderLetter();
            return warband;
        }


        static bool EmptyWarbandArrangement()
        {
            return !GameComponent_WAW.playerWarbandPreset.bandMembers.Any(x => x.Value > 0);
        }

    }
}

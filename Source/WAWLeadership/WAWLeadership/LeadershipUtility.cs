using RimWorld;
using RimWorld.Planet;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands;
using WarfareAndWarbands.Warband;
using WarfareAndWarbands.Warband.WarbandComponents;
using WAWLeadership.LeadershipAttributes;
using WAWLeadership.WorldObjectComps;

namespace WAWLeadership
{
    public static class LeadershipUtility
    {
        static Pawn leaderCache;
        static WorldObjectComp_PlayerWarbandLeader leaderCompCache;
        static Warband warbandCache;

        public static WorldObjectComp_PlayerWarbandLeader LeaderCompCache => leaderCompCache;

        public static void SetWarbandCache(Warband warband)
        {
            warbandCache = warband;
        }

        public static void SetLeaderCache(Pawn leader)
        {
            leaderCache = leader;
        }
        public static void SetLeaderCompCache(WorldObjectComp_PlayerWarbandLeader leader)
        {
            leaderCompCache = leader;
        }

        public static bool SelectInteractionTarget(GlobalTargetInfo info)
        {
            if (info.WorldObject == null)
            {
                return false;
            }
          
            if (leaderCompCache != null && Find.WorldGrid.ApproxDistanceInTiles(info.Tile, leaderCompCache.parent.Tile) > (float)PlayerWarbandManager.playerAttackRange)
            {
                return false;
            }
            InteractWithWorldObject(info.WorldObject, out bool usedSkill, leaderCache, warbandCache);
            if (usedSkill)
            {
                leaderCompCache?.SetLastUsageTick();
            }
            return usedSkill;
        }

        public static void InteractWithWorldObject(WorldObject o, Pawn leader = null)
        {
            bool usedSkill = false;
            InteractionUtility.TryToInteract(o, ref usedSkill, leader);
        }


        public static void InteractWithWorldObject(WorldObject o, out bool usedSkill, Pawn leader = null, Warband warband = null)
        {
            usedSkill = false;
            InteractionUtility.TryToInteract(o, ref usedSkill, leader, warband);

        }

        public static void SyncRecoverySpeed(this Warband warband)
        {
            if (!warband.HasLeader() || !warband.playerWarbandManager.leader.IsLeaderAvailable())
            {
                return;
            }
            
        }




    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandComponents.Leader
{
    public static class PlayerWarbandLeaderUtil
    {
        public static bool IsLeader(this Pawn pawn, out Warband warband)
        {
            foreach (var ele in WarbandUtil.AllPlayerWarbandsCache)
            {
                if(ele as Warband == null)
                {
                    continue;
                }
                var playerWarband = ele as Warband;
                if (playerWarband.playerWarbandManager.leader != null &&
                   playerWarband.playerWarbandManager.leader.Leader == pawn)
                {
                    warband = playerWarband;
                    return true;
                }
            }
            warband = null;
            return false;
        }

    }
}

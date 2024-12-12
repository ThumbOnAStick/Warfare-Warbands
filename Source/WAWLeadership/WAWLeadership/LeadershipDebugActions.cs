using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI.Group;
using Verse;
using WarfareAndWarbands.Warband;
using WarfareAndWarbands;

namespace WAWLeadership
{
    public static class LeadershipDebugActions
    {
        [DebugAction("WAW", null, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void TryAddTenExp(Pawn p)
        {
            var compLeader = p.TryGetComp<CompLeadership>();
            if (compLeader == null)
            {
                return;
            }
            compLeader.AddExpFor(100);
            Message message = new Message("Leader attrbutes exp added!", MessageTypeDefOf.PositiveEvent);
            Messages.Message(message);
        }


    }
}

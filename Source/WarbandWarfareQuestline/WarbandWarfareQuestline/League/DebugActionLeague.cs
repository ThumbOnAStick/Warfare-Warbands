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

namespace WarbandWarfareQuestline.League
{
    public static class DebugActionLeague
    {
        [DebugAction("WAW", "Random join player league", false, false, false, false, 0, false, actionType = DebugActionType.Action)]
        public static void SpawnRandomMercenary()
        {
            MinorFaction m = MinorFactionHelper.GenerateRandomMinorFaction();
            m.JoinPlayer();
        }
    }
}

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
using RimWorld.Planet;
using WarfareAndWarbands.Warband.VassalWarband.UI;
using WarbandWarfareQuestline.Skirmish;

namespace WarbandWarfareQuestline.League
{
    public static class DebugActionLeague
    {
        [DebugAction("WAW", "Random faction join player league", false, false, false, false, 0, false, actionType = DebugActionType.Action)]
        public static void SpawnRandomMercenary()
        {
            MinorFaction m = MinorFactionHelper.GenerateRandomMinorFaction();
            m.JoinPlayer();
        }

        [DebugAction("WAW", "Do Random Skirmish", false, false, false, false, 0, false, actionType = DebugActionType.Action)]
        public static void RandomSkirmish()
        {
            GameComponent_Skrimish.Instance.CreateRandomSkirmsish();
        }

        [DebugAction("WAW", "Spawn Skirmish", false, false, false, false, 0, false, actionType = DebugActionType.Action)]
        public static void SpawnSkirmish()
        {
            Find.WorldTargeter.BeginTargeting(SpawnSkirmish, true);
        }

        [DebugAction("WAW", "Spawn Siege", false, false, false, false, 0, false, actionType = DebugActionType.Action)]
        public static void SpawnSiege()
        {
            SpawnSiegeEvent();
        }



        static bool SpawnSkirmish(GlobalTargetInfo info)
        {
            if (info.WorldObject != null)
            {
                return false;
            }
            Skirmish.Skirmish skirmish = SkirmishHelper.CreateSkirmish(info);
            GameComponent_Skrimish.Instance.Register(skirmish);
            return true;
        }
        static void SpawnSiegeEvent()
        {
            Siege skirmish = SkirmishHelper.CreateSiege();
            GameComponent_Skrimish.Instance.Register(skirmish);
        }
    }
}

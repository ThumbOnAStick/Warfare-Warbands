﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarbandWarfareQuestline.League.UI;

namespace WarbandWarfareQuestline.League.Policies.UI
{
    public static class LeagueActions 
    {

        public static FloatMenu GetLeagueActionsMenu()
        {
            return new FloatMenu(GetActions().ToList());
        }

        public static IEnumerable<FloatMenuOption> GetActions()
        {
            if (CanConstructRoad())
            {
                yield return RoadConstructorOption();
            }
        }


        #region RoadBuilder

        static bool CanConstructRoad()
        {
            return GameComponent_League.Instance.RoadBuilder.Unlocked;
        }

        static void SwitchToRoadBuilder()
        {
            CameraJumper.TryShowWorld();
            LeagueDrawer.SwitchDrawingMode(LeagueDrawer.LeagueDrawingMode.Roads);
        }

        static FloatMenuOption RoadConstructorOption()
        {
            return new FloatMenuOption("WAW.FLTM.ConstructRoad".Translate(), () => { SwitchToRoadBuilder(); });
        }

        #endregion
    }
}

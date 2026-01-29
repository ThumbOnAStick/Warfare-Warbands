using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarbandWarfareQuestline.League.UI
{
    public static class LeagueActions 
    {

        public static void OpenActionsMenu()
        {
            Find.WindowStack.Add(GetLeagueActionsMenu());
        }

        public static FloatMenu GetLeagueActionsMenu()
        {
            if (GetActions().Count() <= 0)
            {
                return NoActionMenu();
            }
            return new FloatMenu(GetActions().ToList());
        }

        public static IEnumerable<FloatMenuOption> GetActions()
        {
            if (CanConstructRoad())
            {
                yield return RoadConstructorOption();
            }

            if (CanExecuteMilitaryDrill())
            {
                yield return MilitaryDrillOption();
            }

            yield break;
        }

        public static FloatMenu NoActionMenu()
        {
            return new FloatMenu(new List<FloatMenuOption>
            {
                new FloatMenuOption("WAW.FLTM.NoActions".Translate(), null)
            })
            ;
        }


        #region RoadBuilder
        static bool CanConstructRoad()
        {
            return GameComponent_League.Instance.RoadBuilder.IsActive;
        }

        static void SwitchToRoadBuilder()
        {
            // When the cooldown is active, notify the player
            if (!GameComponent_League.Instance.RoadBuilder.IsAvailable())
            {
                GameComponent_League.Instance.RoadBuilder.SendAvailabilityNotification();
                return;
            }

            CameraJumper.TryShowWorld();
            LeagueDrawer.SwitchDrawingMode(LeagueDrawer.LeagueDrawingMode.Roads);
        }

        static FloatMenuOption RoadConstructorOption()
        {
            return new FloatMenuOption("WAW.FLTM.ConstructRoad".Translate(), () => { SwitchToRoadBuilder(); });
        }

        #endregion


        #region MilitaryDrill

        static bool CanExecuteMilitaryDrill()
        {
            return GameComponent_League.Instance.MilitaryDrill.IsActive;
        }

        public static FloatMenuOption MilitaryDrillOption()
        {
            return new FloatMenuOption("WAW.FLTM.MilitaryDrill".Translate(), () => { GameComponent_League.Instance.MilitaryDrill.TryToExecute(); });
        }
        #endregion
    }
}

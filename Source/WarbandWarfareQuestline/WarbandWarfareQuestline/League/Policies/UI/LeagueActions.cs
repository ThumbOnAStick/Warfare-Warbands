using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarbandWarfareQuestline.League.UI;
using WarbandWarfareQuestline.Skirmish;

namespace WarbandWarfareQuestline.League.Policies.UI
{
    public static class LeagueActions 
    {

        public static void OpenActionsMenu()
        {
            if(GetActions().Count() == 0)
            {
                return;
            }
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

            if (GameComponent_Skrimish.Instance.CanCreatePlayerSkirmish())
            {
                yield return PlayerSkirmishOption();
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
            return GameComponent_League.Instance.RoadBuilder.Unlocked;
        }

        static void SwitchToRoadBuilder()
        {
            // When the cool down is active, notify the player
            if (!GameComponent_League.Instance.RoadBuilder.IsBuilderReadyToBuild())
            {
                Messages.Message("WAW.RoadConstruct.DaysAhead".Translate(GameComponent_League.Instance.RoadBuilder.UsageRemainingCoolDownDays.ToString("0.0")), MessageTypeDefOf.RejectInput);
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

        #region PlayerSkirmish
        public static FloatMenuOption PlayerSkirmishOption()
        {
            return new FloatMenuOption("WAW.FLTM.PlayerSkirmish".Translate(), () => { GameComponent_Skrimish.Instance.CreatePlayerSkirmish(); });
        }
        #endregion
    }
}

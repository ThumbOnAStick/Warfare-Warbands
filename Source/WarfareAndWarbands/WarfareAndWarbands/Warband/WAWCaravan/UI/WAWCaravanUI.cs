using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace WarfareAndWarbands.Warband.WAWCaravan.UI
{
    internal static class WAWCaravanUI
    {
        public static Command CreateWarbandWith(Caravan caravan)
        {
            GameComponent_WAW.Instance.SetAlreadyAssignedLeader();
            Command_Action command_Action = new Command_Action();
            command_Action.Disabled = CaravanWarbandUtility.CannotCreateWarband(caravan);
            command_Action.defaultLabel = "WAW.CreateWarband".Translate();
            command_Action.defaultDesc = "WAW.CreateWarband.Desc".Translate();
            command_Action.icon = WAWTex.CreateWarbandTex;
            command_Action.action = delegate ()
              {
                  AssignLeaderOptionUponEstablishment(caravan);
              };
            command_Action.Order = 3000f;
            return command_Action;
        }


        static void AssignLeaderOptionUponEstablishment(Caravan caravan)
        {
            var opts = AssignLeaderOptionUponEstablishmentOptions(caravan).ToList();
            Find.WindowStack.Add(new FloatMenu(opts));
        }

        static IEnumerable<FloatMenuOption> AssignLeaderOptionUponEstablishmentOptions(Caravan caravan)
        {
            foreach(var p in caravan.pawns)
            {
                if (p.IsColonist)
                {
                    yield return AssignLeaderOptionUponEstablishmentOption(p, caravan);
                }
            }
        }

        static FloatMenuOption AssignLeaderOptionUponEstablishmentOption(Pawn pawn, Caravan caravan)
        {
            var option = new FloatMenuOption(
                "WAW.AssignLeader".Translate(pawn.NameFullColored),
                delegate { OpenCaravanWarbandCreationWindow(pawn, caravan); },
                iconThing: pawn,
                iconColor: Color.white
                );
            return option;
        }

        static void OpenCaravanWarbandCreationWindow(Pawn pawn, Caravan caravan)
        {
            GameComponent_WAW.Instance.SetAlreadyAssignedLeader();
            Window_ArrangeWarband_Caravan window = new Window_ArrangeWarband_Caravan(pawn, caravan);
            Find.WindowStack.Add(window);
        }

    }
}

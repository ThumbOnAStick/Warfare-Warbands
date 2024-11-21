using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace WarfareAndWarbands.Warband.UI
{
    public static class WarbandUI
    {
        public static Command MoveWarbandCommand()
        {
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "WAW.ResettleWarband".Translate();
            command_Action.defaultDesc = "WAW.ResettleWarband.Desc".Translate();
            command_Action.icon = CommandIcon;
            command_Action.action = delegate ()
            {

            };
            command_Action.Order = 3000f;
            return command_Action;
        }

        public static Command OrderWarbandToAttackCommand(Warband band)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "WAW.OrderAttack".Translate();
            command_Action.defaultDesc = "WAW.OrderAttack.Desc".Translate();
            command_Action.icon = TexCommand.Attack;
            command_Action.action = delegate ()
            {
                band.OrderPlayerWarbandToAttack();
            };
            command_Action.Order = 3000f;
            return command_Action;
        }


        private static readonly Texture2D CommandIcon = ContentFinder<Texture2D>.Get("UI/Commands/Resettle", true);

    }
}

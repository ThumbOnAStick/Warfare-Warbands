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
    [StaticConstructorOnStartup]
    public static class WarbandUI
    {
        public static Command MoveWarbandCommand(Warband band)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "WAW.ResettleWarband".Translate();
            command_Action.defaultDesc = "WAW.ResettleWarband.Desc".Translate();
            command_Action.icon = WAWTex.ResettleIcon;
            command_Action.action = delegate ()
            {
                band.OrderPlayerWarbandToResettle();
            };
            command_Action.Order = 3000f;
            return command_Action;
        }

        public static Command OrderWarbandToAttackCommand(Warband band)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "WAW.OrderAttack".Translate() + $"(${(int)PlayerWarbandArrangement.GetCost(band.bandMembers)})";
            command_Action.defaultDesc = "WAW.OrderAttack.Desc".Translate();
            command_Action.icon = TexCommand.Attack;
            command_Action.action = delegate ()
            {
                band.attackManager.OrderPlayerWarbandToAttack();
            };
            command_Action.Order = 3000f;
            return command_Action;
        }
        public static Command WithdrawWarbandItems(Warband band)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "WAW.WarbandWithdraw".Translate();
            command_Action.defaultDesc = "WAW.WarbandWithdraw.Desc".Translate();
            command_Action.icon = WAWTex.WarbandWithdrawTex;
            command_Action.action = delegate ()
            {
                FloatMenu floatMenuMap = new FloatMenu(DumpLootOptions(band).ToList());
                Find.WindowStack.Add(floatMenuMap);
            };
            command_Action.Order = 3000f;
            return command_Action;
        }

        static IEnumerable<FloatMenuOption> DumpLootOptions(Warband band)
        {
            yield return new FloatMenuOption("WAW.InItems".Translate(), delegate { band.DumpLoot(); });
            yield return new FloatMenuOption("WAW.InSilvers".Translate(), delegate { band.DumpLootInSilver(); });

        }

        public static Command DismissWarband(Warband band)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "WAW.Dismiss".Translate();
            command_Action.defaultDesc = "WAW.Dismiss.Desc".Translate();
            command_Action.icon = WAWTex.DismissTex;
            command_Action.action = delegate ()
            {
                ConfirmDestroy(band);
            };
            command_Action.Order = 3000f;
            return command_Action;
        }

        static void ConfirmDestroy(Warband warband)
        {
            WindowStack windowStack = Find.WindowStack;
            TaggedString text = "WAW.ConfirmDestroyWarBand".Translate();
            void confirmedAct()
            {
                warband.Destroy();
            }

            windowStack.Add(Dialog_MessageBox.CreateConfirmation(text, confirmedAct, true, null, WindowLayer.Dialog));
        }

        public static Command ConfigureWarband(Warband band)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "WAW.Configure".Translate();
            command_Action.defaultDesc = "WAW.Configure.Desc".Translate();
            command_Action.icon = WAWTex.ReArrangeIcon;
            command_Action.action = delegate ()
            {
                WarbandUtil.ReArrangePlayerWarband(band);
            };
            command_Action.Order = 3000f;
            return command_Action;
        }

        // pawn commands
        public static Command RetreatPawn(CompMercenary comp)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "WAW.Retreat".Translate();
            command_Action.defaultDesc = "WAW.Retreat.Desc".Translate();
            command_Action.icon = TexUI.RotLeftTex;
            command_Action.action = delegate ()
            {
                comp.Retreat();
            };
            command_Action.Order = 3000f;
            return command_Action;
        }
        public static Command PlaceLootChest(Warband warband)
        {
            Command_Action command_Action = new Command_Action
            {
                defaultLabel = "WAW.PlaceLootBox".Translate(),
                defaultDesc = "WAW.PlaceLootBox.Desc".Translate(),
                icon = WAWTex.OpenTex,
                action = delegate ()
                {
                    WarbandUtil.TryToSpawnLootChest(warband);
                },
                Order = 3000f
            };
            return command_Action;
        }
        public static Command TransferContent(CompLootChest lootComp)
        {
            Command_Action command_Action = new Command_Action
            {
                defaultLabel = "WAW.TransferContent".Translate(),
                defaultDesc = "WAW.TransferContent.Desc".Translate(),
                icon = TexUI.ArrowTexRight,
                action = delegate ()
                {
                    lootComp.TransferToWarband();
                },
                Order = 3000f
            };
            return command_Action;
        }

        public static Command RetreatAllPawns(Map m)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "WAW.RetreatAll".Translate();
            command_Action.defaultDesc = "WAW.RetreatAll.Desc".Translate();
            command_Action.icon = TexUI.RotLeftTex;
            command_Action.action = delegate ()
            {
                foreach(var p in m.mapPawns.AllPawnsSpawned)
                {
                    var comp = p.TryGetComp<CompMercenary>();   
                    if(comp != null && comp.ServesPlayerFaction)
                    {
                        comp.Retreat();
                    }
                }
            };
            command_Action.Order = 3000f;
            return command_Action;
        }



    }
}

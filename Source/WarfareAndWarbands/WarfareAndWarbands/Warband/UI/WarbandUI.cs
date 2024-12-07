using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using WarfareAndWarbands.CharacterCustomization;
using WarfareAndWarbands.Warband.PlayerWarbandRaid;
using WarfareAndWarbands.Warband.WarbandComponents;

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
            command_Action.defaultLabel = "WAW.OrderAttack".Translate() + $"(${(int)PlayerWarbandArrangement.GetCostOriginal(band.bandMembers)})";
            command_Action.defaultDesc = "WAW.OrderAttack.Desc".Translate();
            command_Action.icon = TexCommand.Attack;
            command_Action.action = delegate ()
            {
                band.playerWarbandManager?.OrderPlayerWarbandToAttack();
            };
            command_Action.Order = 3000f;
            return command_Action;
        }
        public static Command WithdrawWarbandItems(Warband band)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.Disabled = LootDisabled(band);
            command_Action.disabledReason = "WAW.EmptyStorage".Translate();
            command_Action.defaultLabel = "WAW.WarbandWithdraw".Translate();
            command_Action.defaultDesc = "WAW.WarbandWithdraw.Desc".Translate();
            command_Action.icon = WAWTex.WarbandWithdrawTex;
            command_Action.action = delegate ()
            {
                FloatMenu floatMenuMap = new FloatMenu(WithDrawLootOptions(band).ToList());
                Find.WindowStack.Add(floatMenuMap);
            };
            command_Action.Order = 3000f;
            return command_Action;
        }

        static bool LootDisabled(Warband band)
        {
            return band.playerWarbandManager == null || band.playerWarbandManager.lootManager == null || band.playerWarbandManager.lootManager.GetLootCount() < 1;
        }
        public static Command ResetRaidCooldown(Warband band)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "Reset raid cooldown";
            command_Action.action = delegate ()
            {
                band.playerWarbandManager.ResetRaidTick();
            };
            command_Action.Order = 3000f;
            return command_Action;
        }

        static IEnumerable<FloatMenuOption> WithDrawLootOptions(Warband band)
        {
            yield return new FloatMenuOption("WAW.InItems".Translate(), delegate { band.WithdrawLoot(); });
            yield return new FloatMenuOption("WAW.InSilvers".Translate(), delegate { band.WithdrawLootInSilver(); });

        }

        public static IEnumerable<FloatMenuOption> PlayerWarbandAttackOptions(PlayerWarbandManager attackManager)
        {
            yield return new FloatMenuOption("WAW.LandAttack".Translate(), delegate { attackManager.AttackLand(); });
            if (attackManager.targetMapP.HasMap)
                yield return new FloatMenuOption("WAW.PodAttack".Translate(), delegate { attackManager.AttackDropPod(); });

        }

        public static void GetPlayerWarbandAttackOptions(PlayerWarbandManager attackManager)
        {
            FloatMenu floatMenuMap = new FloatMenu(PlayerWarbandAttackOptions(attackManager).ToList());
            Find.WindowStack.Add(floatMenuMap);
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
                warband?.playerWarbandManager?.leader?.ReturnLeader(); 
                warband?.Destroy();
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
            command_Action.defaultDesc = CannotRetreat(comp.Mercenary) ? "WAW.Surrounded.Desc".Translate() : "WAW.Retreat.Desc".Translate();
            command_Action.disabledReason = "WAW.Surrounded".Translate();
            command_Action.Disabled = CannotRetreat(comp.Mercenary);
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
                foreach (var p in m.mapPawns.AllPawnsSpawned)
                {
                    var comp = p.TryGetComp<CompMercenary>();
                    if (comp != null && comp.ServesPlayerFaction)
                    {
                        comp.Retreat();
                    }
                }
            };
            command_Action.Order = 3000f;
            return command_Action;
        }

        public static IEnumerable<FloatMenuOption> PlayerWarbandLeaderChoices(Warband warband, Caravan caravan)
        {
            if (warband.Tile != caravan.Tile)
            {
                yield break;
            }
            if (warband.playerWarbandManager.leader == null)
            {
                yield break;
            }
            if (warband.playerWarbandManager.leader.Leader != null &&
                !warband.playerWarbandManager.leader.Leader.Dead)
            {
                yield break;
            }
            ThingOwner<Pawn> pawns = caravan.pawns;
            for (int i = 0; i < pawns.Count; i++)
            {
                var pawn = pawns[i];
                if (pawn.IsColonist)
                    yield return AssignLeaderOption(pawn, caravan, warband);
            }
        }

        static FloatMenuOption AssignLeaderOption(Pawn pawn, Caravan caravan, Warband warband)
        {
            var option = new FloatMenuOption(
                "WAW.AssignLeader".Translate(pawn.NameFullColored),
                delegate { AssignLeader(pawn, caravan, warband); },
                iconThing: pawn,
                iconColor: Color.white
                );
            return option;
        }

        static void AssignLeader(Pawn pawn, Caravan caravan, Warband warband)
        {
            warband.playerWarbandManager?.leader?.AssignLeader(pawn, caravan);
        }


        public static string PawnKindLabel(PawnKindDef p)
        {
            string label = GameComponent_Customization.Instance.customizationRequests.Any(x => x.defName == p.defName) ?
        p.label.Colorize(FactionDefOf.PlayerColony.DefaultColor) : p.label;
            return label;
        }

        static bool CannotRetreat(Pawn p)
        {
            return
                p.MapHeld != null &&
                p.MapHeld.GetComponent<MapComponent_WarbandRaidTracker>() != null &&
                p.MapHeld.GetComponent<MapComponent_WarbandRaidTracker>().ValidateMap() &&
                !p.MapHeld.GetComponent<MapComponent_WarbandRaidTracker>().LtterSent();
        }
    }
}

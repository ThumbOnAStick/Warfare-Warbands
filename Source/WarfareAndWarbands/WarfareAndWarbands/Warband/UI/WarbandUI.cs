using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;
using WarfareAndWarbands.CharacterCustomization;
using WarfareAndWarbands.Warband.PlayerWarbandRaid;
using WarfareAndWarbands.Warband.WarbandComponents;
using WarfareAndWarbands.Warband.WAWCaravan.UI;

namespace WarfareAndWarbands.Warband.UI
{
    [StaticConstructorOnStartup]
    public static class WarbandUI
    {

        private static List<Color> allApperalColors;
        private static List<Color> AllApprealColors
        {
            get
            {
                if (allApperalColors == null)
                {
                    allApperalColors = new List<Color>();
                    foreach (ColorDef colorDef in DefDatabase<ColorDef>.AllDefs)
                    {
                        Color color = colorDef.color;
                        if (!allApperalColors.Any((Color x) => x.WithinDiffThresholdFrom(color, 0.15f)))
                        {
                            allApperalColors.Add(color);
                        }
                    }
                    allApperalColors.SortByColor((Color x) => x);
                }
                return allApperalColors;
            }
        }


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
                warband?.playerWarbandManager?.leader?.ReturnLeaderHome(); 
                warband?.Destroy();
            }

            windowStack.Add(Dialog_MessageBox.CreateConfirmation(text, confirmedAct, true, null, WindowLayer.Dialog));
        }

        public static Command ConfigureWarband(Warband band)
        {
            Command_Action command_Action = new Command_Action
            {
                defaultLabel = "WAW.Configure".Translate(),
                defaultDesc = "WAW.Configure.Desc".Translate(),
                icon = WAWTex.ReArrangeIcon,
                action = delegate ()
                {
                    WarbandUtil.ReArrangePlayerWarband(band);
                },
                Order = 3000f
            };
            return command_Action;
        }

        public static Command RenameWarband(Warband band)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "WAW.RenameWarband".Translate();
            command_Action.icon = TexUI.RenameTex;
            command_Action.action = delegate ()
            {
                Dialog_SetCustomName window = new Dialog_SetCustomName(band);
                Find.WindowStack.Add(window);       
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

        public static Command LinkWarband(CompLootChest lootComp)
        {
            Command_Action command_Action = new Command_Action
            {
                defaultLabel = "WAW.LinkWarband".Translate(),
                icon = WAWTex.WarbandWorldObjectTex,
                action = delegate ()
                {
                    var warband = lootComp.warband;
                    if(warband == null)
                    {
                        return;
                    }
                    CameraJumper.TryJump(CameraJumper.GetWorldTarget(warband), CameraJumper.MovementMode.Pan);
                    Find.WorldSelector.Select(warband);
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
            if (Find.WorldGrid.ApproxDistanceInTiles(warband.Tile, caravan.Tile) > 2)
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

        public static IEnumerable<FloatMenuOption> PlayerWarbandEstablishmentLeaderChoices(Warband warband, Caravan caravan)
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

        public static void DrawColorPanel(Rect inRect, out float colorsHeight, out Rect colorSelectorRect, Warband warband = null)
        {
            Rect colorLabelRect = new Rect(inRect.x, inRect.y, 100, 50);
            Rect colorBoxRect = new Rect(inRect.x + 130, inRect.y, 22, 22);
            colorSelectorRect = new Rect(inRect.x, inRect.y + 30, inRect.width, 50);
            Widgets.Label(colorLabelRect, "WAW.ColorOverride".Translate());
            Color color = warband == null ? GameComponent_WAW.playerWarband.colorOverride : warband.playerWarbandManager.colorOverride.GetColorOverride();
            Widgets.ColorBox(colorBoxRect, ref color, color);
            bool selectColor = Widgets.ColorSelector(colorSelectorRect, ref color, AllApprealColors, out colorsHeight, null, 22, 2, null);
            if (selectColor)
            {
                TrySetColorOverride(warband, color);
            }
            GameComponent_WAW.playerWarband.colorOverride = color;
        }

        static void TrySetColorOverride(Warband warband, Color color)
        {
            warband?.playerWarbandManager?.colorOverride?.SetColorOverride(color);

        }

        public static void DrawPawnSelection(
            Rect inRect,
            Rect colorSelectorRect,
            ref Vector2 scrollPosition,
            int pawnKindsEachRow,
            float colorsHeight,
            float descriptionHeight,
            float descriptionWidth,
            float entryWidth,
            float entryHeight
            )
        {
        
            var techLeve = "WAW.TechLevel".Translate((int)GameComponent_WAW.playerWarband.techLevel);
            int techWidth = 80;
            Rect techRect = new Rect(380 - techWidth / 2, colorSelectorRect.y + colorsHeight, techWidth, 50);
            Rect techRectMinus = new Rect(techRect.x - entryWidth, colorSelectorRect.y + colorsHeight, entryWidth, entryHeight);
            Rect techRectAddon = new Rect(techRect.xMax + entryWidth, colorSelectorRect.y + colorsHeight, entryWidth, entryHeight);
            bool decreaseTech = Widgets.ButtonImage(techRectMinus, TexUI.ArrowTexLeft);
            Widgets.Label(techRect, techLeve);
            bool addTech = Widgets.ButtonImage(techRectAddon, TexUI.ArrowTexRight);
            if (addTech && GameComponent_WAW.playerWarband.techLevel < TechLevel.Archotech) { GameComponent_WAW.playerWarband.techLevel++; }
            if (decreaseTech && GameComponent_WAW.playerWarband.techLevel > TechLevel.Undefined) { GameComponent_WAW.playerWarband.techLevel -= 1; }
            var allCombatPawns = WarbandUtil.SoldierPawnKindsWithTechLevel(GameComponent_WAW.playerWarband.techLevel);
            Rect outRect = new Rect(inRect.x, colorSelectorRect.yMax + 50f, inRect.width, 200f);
            Rect viewRect = new Rect(inRect.x, outRect.y, inRect.width - 30f, (float)((allCombatPawns.Count() / pawnKindsEachRow + 1) * (descriptionHeight + 10)));
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            float depth = outRect.y;
            int pawnKindsStacked = 0;
            if (allCombatPawns.Count < 1)
            {
                Widgets.Label(new Rect(30, depth, descriptionWidth, descriptionHeight), "WAW.FoundZeroPawns".Translate());
            }
            foreach (PawnKindDef p in allCombatPawns)
            {
                pawnKindsStacked++;
                if (pawnKindsStacked > pawnKindsEachRow)
                {
                    pawnKindsStacked = 1;
                    depth += descriptionHeight + 10;
                }
                float distance = 30 + 110 * (pawnKindsStacked - 1);
                Widgets.Label(new Rect(distance, depth, descriptionWidth, descriptionHeight), WarbandUI.PawnKindLabel(p) + "(" + p.combatPower + ")");
                var amount = GameComponent_WAW.playerWarband.bandMembers[p.defName];
                bool minus = Widgets.ButtonImage(new Rect(distance, depth + 30, entryWidth, entryHeight), TexUI.ArrowTexLeft);
                Widgets.Label(new Rect(distance + entryWidth, depth + 30, entryWidth, entryHeight), amount.ToString());
                bool add = Widgets.ButtonImage(new Rect(distance + entryWidth * 2, depth + 30, entryWidth, entryHeight), TexUI.ArrowTexRight);
                if (minus && GameComponent_WAW.playerWarband.bandMembers[p.defName] > 0) { GameComponent_WAW.playerWarband.bandMembers[p.defName]--; }
                if (add) { GameComponent_WAW.playerWarband.bandMembers[p.defName]++; }
            }

            Widgets.EndScrollView();
        }

        public static void DrawResetButton()
        {
            bool doReset = Widgets.ButtonText(new Rect(330, 350, 100, 20), "WAW.ResetWarband".Translate());
            if (doReset)
            {
                for (int i = 0; i < GameComponent_WAW.playerWarband.bandMembers.Count; i++)
                {
                    var key = GameComponent_WAW.playerWarband.bandMembers.ElementAt(i).Key;
                    GameComponent_WAW.playerWarband.bandMembers[key] = 0;
                }
            }
        }

        public static void DrawExitButton(Window window, Rect inRect)
        {
            Rect exitButtonRect = new Rect(inRect.xMax - 30, 0, 30, 30);
            bool exit = Widgets.ButtonImage(exitButtonRect, TexButton.CloseXSmall);
            if (exit)
            {
                window.Close();
            }
        }

        static void AssignLeader(Pawn pawn, Caravan caravan, Warband warband)
        {
            warband.playerWarbandManager?.leader?.AssignLeader(pawn, caravan);
        }
        static void AssignLeader(Pawn pawn, Caravan caravan, WorldObject_WarbandRecruiting warband)
        {
            warband.AssignLeader(pawn, caravan);
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

using AlienRace;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Noise;
using WarfareAndWarbands.CharacterCustomization;
using WarfareAndWarbands.Warband.Compatibility_Vehicle;
using WarfareAndWarbands.Warband.PlayerWarbandRaid;
using WarfareAndWarbands.Warband.VassalWarband;
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
            string costString = band.playerWarbandManager.upgradeHolder.CostsSilver ? $"(${(int)PlayerWarbandArrangement.GetCostOriginal(band.bandMembers)})" : "";
            command_Action.defaultLabel = "WAW.OrderAttack".Translate() + costString;
            command_Action.defaultDesc = "WAW.OrderAttack.Desc".Translate();
            command_Action.icon = TexCommand.Attack;
            command_Action.action = delegate ()
            {
                band.playerWarbandManager.OrderPlayerWarbandToAttack();
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
            yield return new FloatMenuOption("WAW.DepositeLoots".Translate(), delegate { band.WithdrawLootToBank(); });

        }

        public static IEnumerable<FloatMenuOption> PlayerWarbandAttackOptions(PlayerWarbandManager attackManager)
        {

            yield return new FloatMenuOption("WAW.LandAttack".Translate(), delegate
            {
                attackManager.AttackLand();
                WarbandUtil.TryToSendQuickAttackLetter();
            });
            if (attackManager.targetMapP.HasMap && attackManager.upgradeHolder.CanDroppod)
            {
                if (GameComponent_WAW.Instance.IsDropRaidAvailable || !WAWSettings.enableDroppodPolicyRequirement)
                    yield return new FloatMenuOption("WAW.PodAttack".Translate(), delegate
                    {
                        attackManager.AttackDropPod();
                        WarbandUtil.TryToSendQuickAttackLetter();
                    });
                else
                {
                    yield return new FloatMenuOption("WAW.FLTM.PodAttackUnavailable".Translate(), null);
                }
            }


        }

        public static IEnumerable<FloatMenuOption> VassalWarbandAttackOptions(WorldObject_VassalWarband vassal, MapParent p)
        {

            yield return new FloatMenuOption("WAW.LandAttack".Translate(), delegate
            {
                vassal.AttackLand(p);
            });
        }

        public static void GetVassalWarbandAttackOptions(this WorldObject_VassalWarband vassal, MapParent p)
        {
            FloatMenu floatMenuMap = new FloatMenu(VassalWarbandAttackOptions(vassal, p).ToList());
            Find.WindowStack.Add(floatMenuMap);
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

        public static Command RecruitPawn(CompMercenary comp)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "WAW.PromoteSoldier".Translate(comp.parent.MarketValue);
            command_Action.defaultDesc = "WAW.PromoteSoldier.Desc".Translate(comp.Mercenary.NameShortColored);
            command_Action.icon = WAWTex.PromoteSoldier;
            command_Action.action = delegate ()
            {
                comp.TryToPromote();
            };
            command_Action.Order = 3000f;
            return command_Action;
        }

        public static Command RecycleVehicle(CompMercenary comp)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "WAW.RecycleVehicle".Translate();
            command_Action.defaultDesc = "WAW.RecycleVehicle.Desc".Translate();
            command_Action.icon = WAWTex.PurchaseVehicleTex;
            command_Action.action = delegate ()
            {
                Vehicle.RecycleVehicleTargetor(comp.Mercenary);
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


        //Loot chest
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

        public static Command AutoLoadLoot(CompLootChest lootComp)
        {
            Command_Action command_Action = new Command_Action
            {
                icon = WAWTex.QuickLoadTex,
                defaultLabel = "WAW.AutoLoad".Translate(),
                defaultDesc = "WAW.AutoLoad.Desc".Translate(),
                onHover = delegate 
                {
                    GenDraw.DrawRadiusRing(lootComp.parent.Position, lootComp.QuickLootRadius);
                },
                action = delegate ()
                {
                    lootComp.LoadEverythingNear();
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
                    if (warband == null)
                    {
                        return;
                    }
                    Find.WorldSelector.ClearSelection();
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

        public static void DrawColorPanel(Rect inRect, Warband warband = null)
        {
            Rect colorLabelRect = new Rect(inRect.x, inRect.y + 100, 100, 50);
            Rect colorBoxRect = new Rect(inRect.x + 130, colorLabelRect.y, 22, 22);
            Rect colorSelectorRect = new Rect(inRect.x, colorLabelRect.y + 30, inRect.width, 50);
            Widgets.Label(colorLabelRect, "WAW.ColorOverride".Translate());
            Color color = warband == null ? GameComponent_WAW.playerWarband.colorOverride : warband.playerWarbandManager.colorOverride.GetColorOverride();
            Widgets.ColorBox(colorBoxRect, ref color, color);
            bool selectColor = Widgets.ColorSelector(colorSelectorRect, ref color, AllApprealColors, out float colorsHeight, null, 22, 2, null);
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
            ref Vector2 scrollPosition,
            int pawnKindsEachRow,
            float descriptionHeight,
            float descriptionWidth,
            float entryWidth,
            float entryHeight,
            Warband warband = null
        )
        {
            var techLevelLabel = "WAW.TechLevel".Translate((int)GameComponent_WAW.playerWarband.techLevel);
            Rect techRect = TopCenterRectFor(inRect, new Vector2(80, 50));
            Rect techRectMinus = new Rect(techRect.x - entryWidth, techRect.y, entryWidth, entryHeight);
            Rect techRectAddon = new Rect(techRect.xMax + entryWidth, techRect.y, entryWidth, entryHeight);

            if (Widgets.ButtonImage(techRectMinus, TexUI.ArrowTexLeft) && GameComponent_WAW.playerWarband.techLevel > TechLevel.Undefined)
                GameComponent_WAW.playerWarband.techLevel--;
            Widgets.Label(techRect, techLevelLabel);
            if (Widgets.ButtonImage(techRectAddon, TexUI.ArrowTexRight) && GameComponent_WAW.playerWarband.techLevel < TechLevel.Archotech)
                GameComponent_WAW.playerWarband.techLevel++;

            var allCombatPawns = WarbandUtil.SoldierPawnKindsWithTechLevel(GameComponent_WAW.playerWarband.techLevel);
            Rect outRect = CenterRectFor(inRect, new Vector2(inRect.width, 200f));
            Rect viewRect = new Rect(outRect.x, outRect.y, inRect.width - 30f, (allCombatPawns.Count() / pawnKindsEachRow + 1) * (descriptionHeight + entryHeight + 10));

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            float depth = outRect.y;
            int pawnKindsStacked = 0;

            if (!allCombatPawns.Any())
            {
                Widgets.Label(new Rect(30, depth, descriptionWidth, descriptionHeight), "WAW.FoundZeroPawns".Translate());
            }
            else
            {
                foreach (PawnKindDef p in allCombatPawns)
                {
                    if (++pawnKindsStacked > pawnKindsEachRow)
                    {
                        pawnKindsStacked = 1;
                        depth += descriptionHeight + 30;
                    }

                    float distance = 30 + 110 * (pawnKindsStacked - 1);
                    Rect boxRect = new Rect(distance, depth, descriptionWidth - 30, descriptionHeight + entryHeight);

                    var amount = GameComponent_WAW.playerWarband.bandMembers[p.defName];

                    Rect labelRect = CenterRectFor(boxRect, new Vector2(entryWidth, entryHeight));
                    Widgets.Label(labelRect, amount.ToString());
                    if (IsPawnkindAboveLimit(p))
                    {
                        // When the combat power of the pawnkind is above the limit, you cannot add more.
                        GUI.color = new Color(1f, .5f, 0f);
                        TooltipHandler.TipRegion(boxRect, "WAW.CombatPowerOutOfLimit".Translate(GameComponent_Customization.Instance.EquipmentBudgetLimit));
                    }
                    else
                    {
                        if (Widgets.ButtonImage(new Rect(labelRect.xMax, labelRect.y, entryWidth, entryHeight), TexUI.ArrowTexRight))
                            GameComponent_WAW.playerWarband.bandMembers[p.defName]++;
                    }
                    Widgets.Label(new Rect(boxRect.position, new Vector2(descriptionWidth, descriptionHeight)), $"{WarbandUI.PawnKindLabel(p)}({p.combatPower})");
                    GUI.color = Color.white;
                    if (Widgets.ButtonImage(new Rect(labelRect.x - entryWidth * 2, labelRect.y, entryWidth, entryHeight), TexUI.ArrowTexLeft) && amount > 0)
                        GameComponent_WAW.playerWarband.bandMembers[p.defName]--;

                    if (GameComponent_WAW.playerWarband.bandMembers.TryGetValue(p.defName, out int count) && count > 0)
                        Widgets.DrawBox(boxRect);

                    if (warband != null && warband.bandMembers.TryGetValue(p.defName, out int warbandCount) && warbandCount > 0)
                        Widgets.DrawBox(boxRect, lineTexture: BaseContent.GreyTex);

                    if (p.defaultFactionType != null)
                    {
                        GUI.color = p.defaultFactionType.DefaultColor;
                        Widgets.DrawTextureFitted(CenterRectFor(boxRect, new Vector2(30, 30), Vector2.up * 30), p.defaultFactionType.FactionIcon, 1f);
                        GUI.color = Color.white;
                    }
                }
            }

            Widgets.EndScrollView();
        }

        static bool IsPawnkindAboveLimit(PawnKindDef p)
        {
            return WAWSettings.enableEquipmentBudgetLimit && p.combatPower > GameComponent_Customization.Instance.EquipmentBudgetLimit; 
        }


        public static void DrawResetButton()
        {
            bool doReset = Widgets.ButtonText(new Rect(330, 325, 100, 20), "WAW.ResetWarband".Translate());
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

        public static Rect TopCenterRectFor(Rect inRect, Vector2 selfSize)
        {
            return new Rect(TopCenterPositionFor(inRect, selfSize), selfSize);
        }

        public static Rect CenterRectFor(Rect inRect, Vector2 selfSize, Vector2 offset)
        {
            return new Rect(CenterPositionFor(inRect, selfSize) + offset, selfSize);
        }

        public static Rect CenterRectFor(Rect inRect, Vector2 selfSize)
        {
            return new Rect(CenterPositionFor(inRect, selfSize), selfSize);
        }

        static Vector2 CenterPositionFor(Rect inRect, Vector2 selfSize)
        {
            return new Vector2(inRect.center.x - selfSize.x / 2, inRect.center.y - selfSize.y / 2);
        }
        static Vector2 TopCenterPositionFor(Rect inRect, Vector2 selfSize)
        {
            return new Vector2(inRect.center.x - selfSize.x / 2, 0);
        }
        public static void DrawNextStepButton(Rect inRect, ref int step)
        {
            if(step >= 2)
            {
                return;
            }
            Rect nextStepButtonRect = new Rect(inRect.x + inRect.width/ 2 - 100, 400, 200, 50);
            if(Widgets.ButtonText(nextStepButtonRect, "WAW.NextStep".Translate()))
            {
                step++;
            }
        }
        static void AssignLeader(Pawn pawn, Caravan caravan, Warband warband)
        {
            warband.playerWarbandManager?.leader?.AssignLeader(pawn, caravan);
        }

        public static string PawnKindLabel(PawnKindDef p)
        {
            string label = GameComponent_Customization.Instance.CustomizationRequests.Any(x => x.defName == p.defName) ?
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

        public static void FillSlots(Rect slotRect,IEnumerable<Texture> textures)
        {
            float heightRate = slotRect.height / slotRect.width;
            int count = textures.Count();
            int splits = 0;
            if (count > 1)
                for (int j = 1; j < 5; j++)
                {
                    if (count <= Math.Pow(4, j))
                    {
                        splits = j;
                        break;
                    }
                }

            int denominator = splits + 1;
            float edge = slotRect.width / denominator;
            for (int i = 0; i < denominator; i++)
            {
                for (int j = 0; j < denominator; j++)
                {
                    int index = i + j * denominator;
                    if (count < index + 1)
                    {
                        continue;
                    }
                    Widgets.DrawTextureFitted(new Rect(
                        slotRect.x + i * edge,
                        slotRect.y + j * edge * heightRate,
                        edge,
                        edge * heightRate), textures.ElementAt(index), 1f);
                }
            }
        }


    }
}

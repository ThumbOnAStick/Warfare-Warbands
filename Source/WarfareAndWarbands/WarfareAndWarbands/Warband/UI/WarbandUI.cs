using AlienRace;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaWeaponsExpanded;
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

        // Pawnkind selection window attribtes
        private static readonly int incrementY = 30;
        private static readonly float pawnKindsEachRow = 5;
        private static readonly Vector2 pawnSelectionPanelSize = new Vector2(800, 500);
        private static readonly float xIncrement = pawnSelectionPanelSize.x / pawnKindsEachRow;
        private static readonly float pawnkindMargin = xIncrement * .5f;
        private static readonly float pawnkindElementWidth = xIncrement - pawnkindMargin;
        private static readonly float descriptionHeight = 100f;
        private static readonly float descriptionWidth = 120f;
        private static readonly float entryWidth = 20f;
        private static readonly float entryHeight = 20f;
        private static readonly float noBackgroundButtonHeight = 20f;
        private static readonly float noBackgroundButtonWidth = 100f;
        private static bool showCustomPawnKindOnly = false;

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
        private static List<Color> allApperalColors;

        public static Vector2 PawnSelectionPanelSize => pawnSelectionPanelSize;

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
                band.playerWarbandManager.SetAttackTile(band.Tile);
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
                Find.WindowStack.Add(new Window_LootManagement(band.playerWarbandManager.lootManager));
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



        public static IEnumerable<FloatMenuOption> PlayerWarbandAttackOptions(PlayerWarbandManager attackManager)
        {

            yield return new FloatMenuOption("WAW.LandAttack".Translate(), delegate
            {
                attackManager.AttackLand();
                WarbandUtil.TryToSendQuickAttackLetter();
            });
            if (attackManager.targetMapP.HasMap && attackManager.upgradeHolder.CanDroppod)
            {
                yield return new FloatMenuOption("WAW.PodAttack".Translate(), delegate
                    {
                        attackManager.AttackDropPod();
                        WarbandUtil.TryToSendQuickAttackLetter();
                    });

            }
            if (attackManager.targetMapP.HasMap && attackManager.upgradeHolder.HasUpgrade)
            {
                var opts = attackManager.upgradeHolder.SelectedUpgrade.GetExtraAttackFloatMenuOptions(attackManager);
                foreach (var option in opts)
                {
                    yield return option;
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
                    if (comp != null && comp.IsPlayerControlledMercenary)
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
            Color color = warband == null ? GameComponent_WAW.playerWarbandPreset.colorOverride : warband.playerWarbandManager.colorOverride.GetColorOverride();
            Widgets.ColorBox(colorBoxRect, ref color, color);
            bool selectColor = Widgets.ColorSelector(colorSelectorRect, ref color, AllApprealColors, out float colorsHeight, null, 22, 2, null);
            if (selectColor)
            {
                TrySetColorOverride(warband, color);
            }
            GameComponent_WAW.playerWarbandPreset.colorOverride = color;
        }

        static void TrySetColorOverride(Warband warband, Color color)
        {
            warband?.playerWarbandManager?.colorOverride?.SetColorOverride(color);

        }

        public static void DrawPawnSelection(
            Rect inRect,
            ref Vector2 scrollPosition,
            Warband warband = null
        )
        {
            // Draw tech level
            var techLevelLabel = "WAW.TechLevel".Translate((int)GameComponent_WAW.playerWarbandPreset.techLevel);
            Rect techRect = TopCenterRectFor(inRect, new Vector2(80, 50));
            Rect techRectMinus = new Rect(techRect.x - entryWidth, techRect.y, entryWidth, entryHeight);
            Rect techRectAddon = new Rect(techRect.xMax + entryWidth, techRect.y, entryWidth, entryHeight);
            if (Widgets.ButtonImage(techRectMinus, TexUI.ArrowTexLeft) && GameComponent_WAW.playerWarbandPreset.techLevel > TechLevel.Undefined)
                GameComponent_WAW.playerWarbandPreset.techLevel--;
            Widgets.Label(techRect, techLevelLabel);
            if (Widgets.ButtonImage(techRectAddon, TexUI.ArrowTexRight) && GameComponent_WAW.playerWarbandPreset.techLevel < TechLevel.Archotech)
                GameComponent_WAW.playerWarbandPreset.techLevel++;

            // Initialize combat pawns panel
            var allCombatPawns = WarbandUtil.SoldierPawnKindsWithTechLevel(GameComponent_WAW.playerWarbandPreset.techLevel);
            if (showCustomPawnKindOnly) allCombatPawns = allCombatPawns.Where(x => GameComponent_Customization.Instance.CustomizationRequests.Any(y => y.defName == x.defName)).ToList();
            Rect outRect = CenterRectFor(inRect, new Vector2(inRect.width, 200f));
            Rect viewRect = new Rect(outRect.x, outRect.y, inRect.width - 30f, (allCombatPawns.Count() / pawnKindsEachRow + 1) * (descriptionHeight + entryHeight + 10));
            float depth = outRect.y;
            int pawnKindsStacked = 0;

            // Draw combat pawns
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);


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
                        depth += descriptionHeight + incrementY;
                    }

                    float pawnKindRectX = xIncrement * (pawnKindsStacked - 1); // Set up initial values
                    Rect pawnKindMetaRect = new Rect(pawnKindRectX, depth, pawnkindElementWidth, descriptionHeight + entryHeight);

                    Widgets.Label(new Rect(pawnKindMetaRect.position, new Vector2(descriptionWidth, descriptionHeight)), $"{GetPawnKindLabel(p)}({p.combatPower})"); // Display pawn kind label and combat power

                    int amount = 0; // Get amount
                    if (GameComponent_WAW.playerWarbandPreset.bandMembers.ContainsKey(p.defName))
                        amount = GameComponent_WAW.playerWarbandPreset.bandMembers[p.defName];

                    Rect labelRect = CenterRectFor(pawnKindMetaRect, new Vector2(entryWidth * 2, entryHeight)); // Set band members amount
                    string v = Widgets.TextArea(labelRect, amount.ToString());
                    if (int.TryParse(v, out int parsedAmount))
                    {
                        GameComponent_WAW.playerWarbandPreset.bandMembers[p.defName] = parsedAmount;
                    }

                    if (Widgets.ButtonImage(new Rect(labelRect.xMax, labelRect.y, entryWidth, entryHeight), TexButton.Plus)) // Add member
                        GameComponent_WAW.playerWarbandPreset.bandMembers[p.defName]++;


                    GUI.color = Color.white;
                    if (Widgets.ButtonImage(new Rect(labelRect.x - entryWidth, labelRect.y, entryWidth, entryHeight), TexButton.Minus) && amount > 0) // Decrease member
                        GameComponent_WAW.playerWarbandPreset.bandMembers[p.defName]--;

                    if (GameComponent_WAW.playerWarbandPreset.bandMembers.TryGetValue(p.defName, out int count) && count > 0) // Display a box when there's more than one member.
                        Widgets.DrawBox(pawnKindMetaRect);

                    if (warband != null && warband.bandMembers.TryGetValue(p.defName, out int warbandCount) && warbandCount > 0) // Display a box when editing an existing warband
                        Widgets.DrawBox(pawnKindMetaRect, lineTexture: BaseContent.GreyTex);

                    if (p.defaultFactionDef != null) // Display faction def icon
                    {
                        GUI.color = p.defaultFactionDef.DefaultColor;
                        Widgets.DrawTextureFitted(CenterRectFor(pawnKindMetaRect, new Vector2(30, 30), Vector2.up * 30), p.defaultFactionDef.FactionIcon, 1f);
                        GUI.color = Color.white;
                    }
                }
            }

            Widgets.EndScrollView();
        }


        public static void DrawResetButton(Rect inRect)
        {
            bool doReset = Widgets.ButtonText(new Rect(inRect.RightHalf().x - noBackgroundButtonWidth / 2, inRect.BottomPart(.1f).y,
                noBackgroundButtonWidth,
                noBackgroundButtonHeight),
                "WAW.ResetWarband".Translate(), false);
            if (doReset)
            {
                for (int i = 0; i < GameComponent_WAW.playerWarbandPreset.bandMembers.Count; i++)
                {
                    var key = GameComponent_WAW.playerWarbandPreset.bandMembers.ElementAt(i).Key;
                    GameComponent_WAW.playerWarbandPreset.bandMembers[key] = 0;
                }
            }
        }

        public static void DrawShowCustomOnlyButton(Rect inRect)
        {
            Widgets.CheckboxLabeled(new Rect(inRect.RightHalf().x - noBackgroundButtonWidth, inRect.BottomPart(.9f).y,
                noBackgroundButtonWidth * 2, noBackgroundButtonHeight * 2),
                "WAW.ShowCustomOnly".Translate(),
                ref showCustomPawnKindOnly);
            if (showCustomPawnKindOnly)
            {
                GameComponent_WAW.playerWarbandPreset.techLevel = TechLevel.Industrial;
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
            Rect nextStepButtonRect = new Rect(inRect.x + inRect.width / 2 - 100, 400, 200, 50);
            if (Widgets.ButtonText(nextStepButtonRect, "WAW.NextStep".Translate()))
            {
                step++;
            }
        }
        static void AssignLeader(Pawn pawn, Caravan caravan, Warband warband)
        {
            warband.playerWarbandManager?.leader?.AssignLeader(pawn, caravan);
        }

        public static string GetPawnKindLabel(PawnKindDef p)
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

        public static void FillSlots(Rect slotRect, IEnumerable<Texture> textures)
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

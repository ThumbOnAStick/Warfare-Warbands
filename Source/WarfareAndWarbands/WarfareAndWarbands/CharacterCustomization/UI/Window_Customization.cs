

using CombatExtended;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using WarfareAndWarbands.CharacterCustomization.Compatibility;
using WarfareAndWarbands.CharacterCustomization.UI;
using WarfareAndWarbands.Warband;

namespace WarfareAndWarbands.CharacterCustomization
{
    public class Window_Customization : Window
    {
        public enum SelectionType { head, top, bottom, utils, weapons, none };
        SelectionType selectionType = SelectionType.head;
        private List<ThingDef> selectedList;
        private List<ThingDef> apparelHeadsCache;
        private List<ThingDef> apparelTopsCache;
        private List<ThingDef> apparelBottomsCache;
        private List<ThingDef> apparelUtilsCache;
        private List<ThingDef> weaponsCache;
        private Dictionary<ThingDef, ThingStyleDef> thingsAndStyles;

        private CustomizationRequest selectedRequest;
        private Vector2 scrollPosition;
        private Vector2 scrollPosition1;
        private bool displayEquipped;
        private string textBuffer;
        private string filterBuffer;

        private static readonly int boxSize = 50;
        private static readonly int itemSpacing = ModsConfig.IdeologyActive ? 50 : 5;
        private static readonly int pawnkindSpacing = 5;
        private static readonly int pawnkindHeight = 50;
        private static readonly int pawnkindWidth = 150;


        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(800f, 600f);
            }
        }
        public Window_Customization()
        {
            selectedList = new List<ThingDef>();
            var requests = GameComponent_Customization.Instance.customizationRequests;
            if (requests.Count > 0)
            {
                SelectNextRequest(requests.First());
            }
            apparelHeadsCache = DefDatabase<ThingDef>.AllDefs.Where(x => IsHeadGear(x)).ToList();
            apparelHeadsCache.SortBy(x => x.BaseMarketValue);
            apparelTopsCache = DefDatabase<ThingDef>.AllDefs.Where(x => IsTopGear(x)).ToList();
            apparelTopsCache.SortBy(x => x.BaseMarketValue);
            apparelBottomsCache = DefDatabase<ThingDef>.AllDefs.Where(x => IsBottomGear(x)).ToList();
            apparelBottomsCache.SortBy(x => x.BaseMarketValue);
            apparelUtilsCache = DefDatabase<ThingDef>.AllDefs.Where(x => IsUtility(x)).ToList();
            apparelUtilsCache.SortBy(x => x.BaseMarketValue);
            weaponsCache = DefDatabase<ThingDef>.AllDefs.Where(x => x.IsWeapon && x.BaseMarketValue > 1).ToList();
            weaponsCache.SortBy(x => x.BaseMarketValue);
            selectedList = apparelHeadsCache;

        }
        protected override void SetInitialSizeAndPosition()
        {
            base.SetInitialSizeAndPosition();

        }
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;
            Rect exitButtonRect = new Rect(inRect.xMax - 30, 0, 30, 30);
            bool exit = Widgets.ButtonImage(exitButtonRect, TexButton.CloseXSmall);
            if (exit)
            {
                this.Close();
            }
            DrawCutomizationRequests(inRect);
            DrawPawn(inRect);
            var requests = GameComponent_Customization.Instance.customizationRequests;
            if (requests.Count >= 1 && selectedRequest != null)
            {
                DrawTextBox(inRect);
                DrawSelectionPanel(inRect);
                DrawXenoType(inRect);
                DrawAlien(inRect);
            }
        }

        void DrawCutomizationRequests(Rect inRect)
        {
            var requests = GameComponent_Customization.Instance.customizationRequests;
            if (requests.Count < 1)
            {
                Rect noPawnKindsFoundRect = new Rect(30, 20, 200, 100);
                Widgets.Label(noPawnKindsFoundRect, "WAW.NoCustomPawnKinds".Translate());
            }

            Rect outRect = new Rect(50, 100, 150, 400);
            Rect viewRect = new Rect(outRect.x, outRect.y, outRect.width, (pawnkindHeight + pawnkindSpacing) * requests.Count + 5);
            Rect boxRect = new Rect(outRect.x - 10, outRect.y - 10, 170, 420);
            Rect newRequestRect = new Rect(boxRect.xMax, boxRect.y, 30, 30);
            Rect deleteRequestRect = new Rect(boxRect.xMax, newRequestRect.yMax + 10, 30, 30);

            Widgets.DrawBox(boxRect, 1);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            int requestIndex = 0;
            foreach (var request in requests)
            {
                Rect buttonRect = new Rect(viewRect.x, viewRect.y - 5 + requestIndex * (pawnkindHeight + pawnkindSpacing), pawnkindWidth, pawnkindHeight);
                Rect requestRect = new Rect(viewRect.x + 10, viewRect.y + 5 + requestIndex * (pawnkindHeight + pawnkindSpacing), pawnkindWidth, pawnkindHeight);
                bool selectTargetRequest = Widgets.ButtonImage(buttonRect, TexUI.FastFillTex, new Color() { a = 0 });
                string requestAndItsPrice = request.label + $"\n({request.GetCombatPower()})";
                Widgets.DrawBox(buttonRect);
                Widgets.Label(requestRect, requestAndItsPrice);
                requestIndex++;
                if (selectTargetRequest)
                {
                    SelectNextRequest(request);
                }
                if (selectedRequest == request)
                {
                    Widgets.DrawHighlight(buttonRect);
                }
            }
            Widgets.EndScrollView();
            bool createNewRequest = Widgets.ButtonImage(newRequestRect, TexButton.NewFile);
            if (createNewRequest)
            {
                string pawnKindName = Guid.NewGuid().ToString();
                string pawnKindLabel = "NewCustom";
                var customizationRequest = new CustomizationRequest(pawnKindName, pawnKindLabel);
                GameComponent_Customization.Instance.AddPawnKindDefFromRequest(pawnKindName, pawnKindLabel, customizationRequest);
                SelectNextRequest(customizationRequest);
            }

            bool deleteRequest = Widgets.ButtonImage(deleteRequestRect, TexButton.Delete);
            if (deleteRequest && selectedRequest != null)
            {
                GameComponent_Customization.Instance.DeleteRequest(selectedRequest);
            }
        }

        void SelectNextRequest(CustomizationRequest customizationRequest)
        {
            filterBuffer = "";
            selectedRequest = customizationRequest;
            textBuffer = customizationRequest.label;
            // try to load stuff
            if (ModsConfig.IdeologyActive)
            {
                this.thingsAndStyles = new Dictionary<ThingDef, ThingStyleDef>();
                foreach (var item in selectedRequest.thingDefsAndStyles)
                {
                    ThingDef thingdef = null;
                    if(DefDatabase<ThingDef>.AllDefs.Any(x => x.defName == item.Key))
                    {
                        thingdef = DefDatabase<ThingDef>.AllDefs.First(x => x.defName == item.Key);
                    }
                    if(thingdef == null)
                    {
                        continue;
                    }

                    ThingStyleDef styleDef = null;
                    if (DefDatabase<ThingStyleDef>.AllDefs.Any(x => x.defName == item.Value))
                    {
                        styleDef = DefDatabase<ThingStyleDef>.AllDefs.First(x => x.defName == item.Value);
                    }

                    this.thingsAndStyles.Add(thingdef, styleDef);
                }
            }
        }

        void DrawTextBox(Rect inRect)
        {
            int boxWidth = 300;
            Rect boxRect = new Rect(inRect.x + 200, 400, boxWidth, 50);
            string label = Widgets.TextEntryLabeled(boxRect, "WAW.PawnKindName".Translate(), textBuffer);
            textBuffer = label;
            selectedRequest.label = label;
        }

        void DrawXenoType(Rect inRect)
        {
            if (!ModsConfig.BiotechActive)
            {
                return;
            }
            Rect xenoRect = new Rect(inRect.x + 500, 400, 50, 50);
            bool click = Widgets.ButtonImage(xenoRect, selectedRequest.TryGetXeno().Icon);
            if (click)
            {
                GetXenoOptionsMenu();
            }
        }

        void DrawAlien(Rect inRect)
        {
            if (ModsConfig.IsActive("erdelf.HumanoidAlienRaces"))
            {
                Rect buttonRect = new Rect(inRect.x + 450, 500, 100, 50);
                HAR.DrwaAlienButton(buttonRect, selectedRequest);
            }
            UpdatePawnKindDef();
        }


        public void GetXenoOptionsMenu()
        {
            IEnumerable<FloatMenuOption> opts = XenoOptions();
            Find.WindowStack.Add(new FloatMenu(opts.ToList()));
        }

        public IEnumerable<FloatMenuOption> XenoOptions()
        {
            var xenoTypes = DefDatabase<XenotypeDef>.AllDefs;
            foreach (XenotypeDef xeno in xenoTypes)
            {
                var option = new FloatMenuOption(xeno.label, delegate { SetXeno(xeno); }, itemIcon: xeno.Icon, iconColor: Color.white);
                yield return option;
            }
        }
        public IEnumerable<FloatMenuOption> StuffOptions(ThingDef equipment)
        {
            if (equipment == null || !equipment.MadeFromStuff)
            {
                yield break;
            }
            var allStuffs = DefDatabase<ThingDef>.AllDefs.Where(x => x.IsStuff).ToList();
            var validStuffs = AllStuffFromFor(allStuffs, equipment);
            foreach (var stuff in validStuffs)
            {
                yield return new FloatMenuOption(
                    stuff.label,
                    delegate
                {
                    selectedRequest.SetStuff(equipment, stuff);
                    MakeSelection(equipment);
                },
                 itemIcon: Widgets.GetIconFor(stuff),
                 iconColor: Color.white);
            }
        }
        private static List<ThingDef> AllStuffFromFor(List<ThingDef> from, ThingDef thingDef)
        {
            return from.FindAll((ThingDef t) => thingDef.stuffCategories.Find((StuffCategoryDef c) => t.stuffProps.categories.Contains(c)) != null);
        }
        public void SetXeno(XenotypeDef xenoTypeDef)
        {
            selectedRequest.SetXeno(xenoTypeDef);
        }

        void DrawPawn(Rect inRect)
        {
            Rect headRect = new Rect((inRect.xMax + inRect.x) / 2, 100, 50, 50);
            Rect bodyRect = new Rect(headRect.x, headRect.yMax + 10, 50, 100);
            Rect headGearRect = new Rect(headRect.x, headRect.y, boxSize, boxSize);
            Rect topGearRect = new Rect(headRect.x, headRect.y + 100, boxSize, boxSize);
            Rect bottomGearRect = new Rect(headRect.x, headRect.y + 150, boxSize, boxSize);
            Rect weaponRect = new Rect(topGearRect.x + 100, topGearRect.y + 25, boxSize, boxSize);
            Rect utilRect = new Rect(bottomGearRect.x - 100, weaponRect.y, boxSize, boxSize);

            Widgets.DrawTextureFitted(bodyRect, WAWTex.BodyTex, 8);
            Widgets.DrawTextureFitted(headRect, WAWTex.HeadTex, 8);
            Widgets.DrawBoxSolidWithOutline(headGearRect, Color.black, Color.white);
            Widgets.DrawBoxSolidWithOutline(topGearRect, Color.black, Color.white);
            Widgets.DrawBoxSolidWithOutline(bottomGearRect, Color.black, Color.white);
            Widgets.DrawBoxSolidWithOutline(weaponRect, Color.black, Color.white);
            Widgets.DrawBoxSolidWithOutline(utilRect, Color.black, Color.white);

            if (selectedRequest != null)
            {
                if (HasHeadGear(selectedRequest))
                    DrawEquipment(headGearRect, selectedRequest.apparelRequests.First(x => IsHeadGear(x)), SelectionType.head);
                else
                    DrawEquipment(headGearRect, null, SelectionType.head);

                if (HasTopGear(selectedRequest))
                    DrawEquipment(topGearRect, selectedRequest.apparelRequests.First(x => IsTopGear(x)), SelectionType.top);
                else
                    DrawEquipment(topGearRect, null, SelectionType.top);

                if (HasBottomGear(selectedRequest))
                    DrawEquipment(bottomGearRect, selectedRequest.apparelRequests.First(x => IsBottomGear(x)), SelectionType.bottom);
                else
                    DrawEquipment(bottomGearRect, null, SelectionType.bottom);

                if (HasUtility(selectedRequest))
                    DrawEquipment(utilRect, selectedRequest.apparelRequests.First(x => IsUtility(x)), SelectionType.utils);
                else
                    DrawEquipment(utilRect, null, SelectionType.utils);

                DrawEquipment(weaponRect, selectedRequest.weaponRequest, SelectionType.weapons);



            }

        }


        void DrawSearchBar(Rect selectionPanelBar)
        {
            int boxHeight = 30;
            int boxWidth = (int)selectionPanelBar.width - boxHeight;
            Rect searchBoxRect = new Rect(selectionPanelBar.x, selectionPanelBar.yMin - boxHeight, boxHeight, boxHeight);
            Rect boxRect = new Rect(searchBoxRect.xMax + 5, selectionPanelBar.yMin - boxHeight, boxWidth, boxHeight);
            filterBuffer = Widgets.TextField(boxRect, filterBuffer);
            Widgets.ButtonImage(searchBoxRect, TexButton.Search);
        }


        void DrawSelectionPanel(Rect inRect)
        {
            int itemHeight = 50;
            int optionHeight = 20;
            Rect outRect = new Rect(inRect.xMax - 200, 100, 150, 400);
            Rect viewRect = new Rect(outRect.x, outRect.y, outRect.width, (itemHeight + itemSpacing) * selectedList.Count);
            Rect boxRect = new Rect(outRect.x - 10, outRect.y - 10, 170, 420);
            Rect optionRect = new Rect(boxRect.x, boxRect.yMax + optionHeight, 150, optionHeight);
            DrawSearchBar(boxRect);
            Widgets.CheckboxLabeled(optionRect, "WAW.DisplayEquipped".Translate(), ref displayEquipped);
            Widgets.DrawBox(boxRect, 1);
            Widgets.BeginScrollView(outRect, ref scrollPosition1, viewRect);
            int selectedIndex = 0;
            var listToDisplay = selectedList;
            if (displayEquipped)
            {
                scrollPosition1 = new Vector2();
                listToDisplay = AllEquippedItems(selectedList);
            }
            if (filterBuffer != "")
            {
                listToDisplay = listToDisplay.Where(x => x.label.Contains(filterBuffer)).ToList();
            }

            foreach (var item in listToDisplay)
            {
                Rect itemRect = new Rect(viewRect.x + 10, viewRect.y + selectedIndex * (itemHeight + itemSpacing), itemHeight, itemHeight);
                DrawSelectorItem(itemRect, item);
                selectedIndex++;
            }

            Widgets.EndScrollView();
        }

        List<ThingDef> AllEquippedItems(List<ThingDef> selectedList)
        {
            return selectedList.Where(x =>
selectedRequest.apparelRequests.Any(a => a.defName == x.defName)
|| (selectedRequest.weaponRequest != null && selectedRequest.weaponRequest.defName == x.defName)).ToList();
        }

        void DrawSelectorItem(Rect rect, ThingDef item)
        {
            string graphicPath = item.graphicData.texPath;
            var stuff = selectedRequest.GetStuff(item);
            Texture2D itemTex = Widgets.GetIconFor(item, stuff);
            TryToDrawStyleIcon(ref itemTex, item); 
            if (Equipped(item))
            {
                Widgets.DrawHighlight(rect);
            }
            Rect labelRect = new Rect(rect.xMax + 5, rect.y, 80, rect.height);
            Widgets.Label(labelRect, item.label);
            DrawApparelTooltip(rect);
            bool selectEquipment = Widgets.ButtonImage(
                rect, 
                itemTex, 
                baseColor: stuff == null? Color.white : stuff.stuffProps.color);
            if (selectEquipment)
            {
                TryMakeStuffOptions(item);
            }
            TryToDrawStyleButton(rect, item);
        }

        void TryToDrawStyleIcon(ref Texture2D itemTex, ThingDef item)
        {
            if (ModsConfig.IdeologyActive)
            {
                if (this.thingsAndStyles.ContainsKey(item))
                {
                    ThingStyleDef style = thingsAndStyles[item];
                    if (style != null)
                        itemTex = style.UIIcon;
                }
            }
        }

        void TryToDrawStyleButton(Rect rect, ThingDef item)
        {
            if (!ModsConfig.IdeologyActive)
            {
                return;
            }
            Rect styleButtonRect = new Rect(rect.x, rect.yMax + 5, rect.width, itemSpacing);
            if (!Mouse.IsOver(styleButtonRect) && !Mouse.IsOver(rect) && !Equipped(item))
            {
                return;
            }
            bool changeStyle = Widgets.CustomButtonText(
                ref styleButtonRect,
                CustomizationUtil.StyleStringFor(item, thingsAndStyles),
               bgColor: new Color(0, 0, 0, 0),
               textColor: Color.white,
               borderColor: new Color(0, 0, 0, 0),
               unfilledBgColor: new Color(0, 0, 0, 0),
               fillPercent: 0);
            if (changeStyle)
            {
                CustomizationUI.GetStyleOptions(item, ref thingsAndStyles, item.StylesFor(), this);
            }
        }

        bool Equipped(ThingDef item)
        {
            return selectedRequest.apparelRequests.Any(x => x.defName == item.defName)
               || (selectedRequest.weaponRequest != null && selectedRequest.weaponRequest.defName == item.defName);
        }

        void DrawApparelTooltip(Rect rect)
        {
            string tooltip;
            if (selectionType != SelectionType.weapons)
            {
                tooltip = "WAW.tooltip".Translate();
            }
            else
            {
                tooltip = "WAW.tooltip1".Translate();

            }
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
            }
            TooltipHandler.TipRegion(rect, tooltip);
        }

        public override void Close(bool doCloseSound = true)
        {
            base.Close(doCloseSound);
            UpdatePawnKindDef();
        }

        void UpdatePawnKindDef()
        {
            selectedRequest?.UpdatePawnKindDef();
        }

        void DrawEquipment(Rect rect, ThingDef equipment = null, SelectionType selectionType = SelectionType.none)
        {
            bool isInvalid = equipment == null || equipment.graphicData.graphicClass == typeof(Graphic_StackCount);
            Texture2D equipmentTex;

            equipmentTex = isInvalid ? Texture2D.blackTexture : ContentFinder<Texture2D>.Get(equipment.graphicData.texPath);

            bool selectBodyGroup = Widgets.ButtonImage(rect, equipmentTex);
            if (selectBodyGroup)
            {
                SelectList(selectionType);
            }

            if (selectionType != SelectionType.weapons)
            {
                Rect labelRect = new Rect(rect.xMax - 20, rect.yMax - 20, 25, 25);
                Widgets.Label(labelRect, $"+{CurrentSelectionWorn(selectionType).Count()}".Colorize(Color.cyan));
            }
        }

        void TryMakeStuffOptions(ThingDef equipment)
        {
            if (CanAddFloatMenu(equipment))
            {
                AddEquipmentSuffOptions(equipment);
            }
            else
            {
                MakeSelection(equipment);
            }
        }

        void AddEquipmentSuffOptions(ThingDef equipment)
        {
            var opts = StuffOptions(equipment).ToList();
            Find.WindowStack.Add(new FloatMenu(opts));
        }

        bool CanAddFloatMenu(ThingDef equipment)
        {
            return equipment != null &&
                equipment.MadeFromStuff &&
            !selectedRequest.apparelRequests.Any(x => x.defName == equipment.defName);
        }

        public void MakeSelection(ThingDef equipment)
        {
            if (selectionType != SelectionType.weapons)
            {
                if (selectedRequest.apparelRequests.Any(x => x.defName == equipment.defName))
                {
                    selectedRequest.apparelRequests.RemoveAll(x => x.defName == equipment.defName);
                }
                else
                {
                    selectedRequest.apparelRequests.Add(equipment);
                    TryToChangeEquipmentStyle(equipment);
                }
            }
            else
            {
                selectedRequest.weaponRequest = equipment;
                TryToChangeEquipmentStyle(equipment);
            }
            UpdatePawnKindDef();
        }

        public void TryToChangeEquipmentStyle(ThingDef equipment)
        {
            if (!ModsConfig.IdeologyActive)
            {
                return;
            }
            if (thingsAndStyles.ContainsKey(equipment))
            {
                var newName = thingsAndStyles.ContainsKey(equipment) &&
                   thingsAndStyles[equipment] != null ? thingsAndStyles[equipment].defName : "None";
                if (selectedRequest.thingDefsAndStyles.ContainsKey(equipment.defName))
                {
                    selectedRequest.thingDefsAndStyles[equipment.defName] = newName;
                }
                else
                {
                    selectedRequest.thingDefsAndStyles.Add(equipment.defName, newName);
                }
            }
        }

        public List<ThingDef> ReturnSelectedList(SelectionType selectionType = SelectionType.none)
        {
            if (selectionType == SelectionType.none)
            {
                return null;
            }
            if (selectionType == SelectionType.head)
            {
                return apparelHeadsCache;
            }
            else if (selectionType == SelectionType.top)
            {
                return apparelTopsCache;
            }
            else if (selectionType == SelectionType.bottom)
            {
                return apparelBottomsCache;
            }
            else if (selectionType == SelectionType.utils)
            {
                return apparelUtilsCache;
            }
            else
            {
                return weaponsCache;
            }
        }

        IEnumerable<ThingDef> CurrentSelectionWorn(SelectionType type)
        {
            return ReturnSelectedList(type).Where(x => selectedRequest.apparelRequests.Any(a => a.defName == x.defName));
        }


        void SelectList(SelectionType selectionType = SelectionType.none)
        {
            scrollPosition1 = new Vector2();
            this.selectionType = selectionType;
            selectedList = ReturnSelectedList(selectionType);
        }


        bool HasHeadGear(CustomizationRequest request)
        {
            return request.apparelRequests.Any(x => IsHeadGear(x));
        }
        bool HasTopGear(CustomizationRequest request)
        {
            return request.apparelRequests.Any(x => IsTopGear(x));
        }
        bool HasBottomGear(CustomizationRequest request)
        {
            return request.apparelRequests.Any(x => IsBottomGear(x));
        }
        bool HasUtility(CustomizationRequest request)
        {
            return request.apparelRequests.Any(x => IsUtility(x));
        }

        bool IsHeadGear(ThingDef apparel)
        {
            if (apparel.apparel == null)
                return false;
            return apparel.apparel.bodyPartGroups.Any(b => b.defName == "UpperHead" || b.defName == "FullHead");
        }

        bool IsTopGear(ThingDef apparel)
        {
            if (apparel.apparel == null)
                return false;
            return apparel.apparel.bodyPartGroups.Any(b => b.defName == "Torso" || b.defName == "Hands" ||
            b.defName == "Shoulders");
        }

        bool IsBottomGear(ThingDef apparel)
        {
            if (apparel.apparel == null)
                return false;
            return (apparel.apparel.bodyPartGroups.Any(b => b.defName == "Legs") ||
                apparel.apparel.bodyPartGroups.Any(b => b.defName == "Feet")) &&
                !apparel.apparel.bodyPartGroups.Any(b => b.defName == "Torso");
        }

        bool IsUtility(ThingDef apparel)
        {
            if (apparel.apparel == null || apparel.thingCategories == null)
                return false;
            return apparel.thingCategories.Any(c => c.defName == "ApparelUtility");
        }




    }
}


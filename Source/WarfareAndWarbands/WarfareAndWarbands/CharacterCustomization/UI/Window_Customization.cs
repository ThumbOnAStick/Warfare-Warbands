

using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;
using Verse.Noise;
using WarfareAndWarbands.CharacterCustomization.Compatibility;
using WarfareAndWarbands.CharacterCustomization.UI;
using WarfareAndWarbands.Warband;
using WarfareAndWarbands.Warband.UI;
using WarfareAndWarbands.Warfare.UI.Test;

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
        private Dictionary<DraggableButton, CustomizationRequest> draggables;
        private Dictionary<ThingDef, ThingStyleDef> thingsAndStyles;
        private CustomizationRequest selectedRequest;
        private Vector2 scrollPosition;
        private Vector2 scrollPosition1;
        private bool displayEquipped;
        private string textBuffer;
        private string filterStream;
        private List<string> filterBuffer;
        private static readonly int boxSize = 50;
        private static readonly int itemSpacing = ModsConfig.IdeologyActive ? 50 : 5;
        private static readonly int pawnkindSpacing = 5;
        private static readonly int pawnkindHeight = 50;
        private static readonly int pawnkindWidth = 150;
        private Rect requestViewRect;
        private Rect requestOutRect;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(800f, 600f);
            }
        }
        public Window_Customization()
        {
            InitializeRequests();
            InitializeEquipments();
            RefreshDraggables();
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
                DrawPawnCost(inRect);
                DrawTextBox(inRect);
                DrawSelectionPanel(inRect);
                DrawXenoType(inRect);
                DrawAlien(inRect);
            }
        }

        void InitializeRequests()
        {
            selectedList = new List<ThingDef>();
            var requests = GameComponent_Customization.Instance.customizationRequests;
            if (requests.Count > 0)
            {
                SelectNextRequest(requests.First());
            }
        }

        void InitializeEquipments()
        {
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

        void RefreshDraggables()
        {
            this.draggables = new Dictionary<DraggableButton, CustomizationRequest>();
            var requests = GameComponent_Customization.Instance.customizationRequests;
            requestOutRect = new Rect(50, 100, pawnkindWidth, 400);
            requestViewRect = new Rect(requestOutRect.x, requestOutRect.y, requestOutRect.width - 10, (pawnkindHeight + pawnkindSpacing) * requests.Count + 5);
            int requestIndex = 0;
            Rect buttonRect = new Rect(requestViewRect.x, requestViewRect.y, requestViewRect.width - 10, pawnkindHeight);
            if (GameComponent_Customization.Instance.customizationRequests.Count < 1)
            {
                return;
            }
            foreach (var request in requests)
            {
                void SelectRequest() => SelectNextRequest(request);
                void OnSwapped(DraggableButton other) => this.OnSwapped(request, other);
                bool IsSelected() => this.selectedRequest == request;
                draggables.Add(new DraggableButton(request.label, buttonRect, SelectRequest, OnSwapped, IsSelected), request);
                requestIndex++;
                buttonRect.y += pawnkindHeight + pawnkindSpacing;
            }

        }

        void OnSwapped(CustomizationRequest request, DraggableButton other)
        {
            if (draggables.ContainsKey(other) && draggables[other] != null)
            {
                CustomizationRequest otherRequest = draggables[other];
                var requests = GameComponent_Customization.Instance.customizationRequests;
                var idx1 = requests.IndexOf(request);
                var idx2 = requests.IndexOf(otherRequest);
                var order = new List<int>();
                for (int i = 0; i < requests.Count; i++)
                {
                    if (i == idx1)
                    {
                        order.Add(idx2);
                    }
                    else if (i == idx2)
                    {
                        order.Add(idx1);
                    }
                    else
                    {
                        order.Add(i);
                    }
                }
                GameComponent_Customization.Instance.customizationRequests = order.Select(i => requests[i]).ToList();


            }
        }


        protected override void SetInitialSizeAndPosition()
        {
            base.SetInitialSizeAndPosition();

        }
    

        void DrawPawnCost(Rect inRect)
        {
            Rect boxRect = new Rect(inRect.x + 250, 475, 200, 50);
            Widgets.Label(boxRect, "WAW.Cost".Translate(selectedRequest.CombatPowerCache));
        }

        void DrawCutomizationRequests(Rect inRect)
        {

            Rect boxRect = new Rect(requestOutRect.x - 10, requestOutRect.y - 10, 170, 420);
            Rect newRequestRect = new Rect(boxRect.xMax, boxRect.y, 30, 30);
            Rect cloneRequestRect = selectedRequest != null ? new Rect(boxRect.xMax, newRequestRect.yMax + 10, 30, 30) : newRequestRect;
            Rect deleteRequestRect = new Rect(boxRect.xMax, cloneRequestRect.yMax + 10, 30, 30);

            var requests = GameComponent_Customization.Instance.customizationRequests;
            if (requests.Count < 1)
            {
                Rect noPawnKindsFoundRect = new Rect(30, 20, 200, 100);
                Widgets.Label(noPawnKindsFoundRect, "WAW.NoCustomPawnKinds".Translate());
            }
            else
            {
                Widgets.DrawBox(boxRect, 1);
            }

            Widgets.BeginScrollView(requestOutRect, ref scrollPosition, requestViewRect);
            bool dragged = false;
            for (int i = 0; i < draggables.Count; i++)
            {
                var button = draggables.ElementAt(i).Key;
                dragged = button.Update(dragged, draggables.Keys.ToList());
            }
            Widgets.EndScrollView();
            bool createNewRequest = Widgets.ButtonImage(newRequestRect, TexButton.NewFile);
            if (createNewRequest)
            {
                string pawnKindName = Guid.NewGuid().ToString();
                string pawnKindLabel = "NewCustom";
                var customizationRequest = new CustomizationRequest(pawnKindName, pawnKindLabel);
                GameComponent_Customization.Instance.AddRequest(pawnKindName, pawnKindLabel, customizationRequest);
                RefreshDraggables();
                SelectNextRequest(customizationRequest);
            }

            if (selectedRequest != null)
            {
                bool cloneRequest = Widgets.ButtonImage(cloneRequestRect, TexButton.Copy);
                if (cloneRequest)
                {
                    var customizationRequest = new CustomizationRequest(selectedRequest);
                    GameComponent_Customization.Instance.AddRequest(customizationRequest);
                    RefreshDraggables();
                    SelectNextRequest(customizationRequest);
                }

            }

            bool deleteRequest = Widgets.ButtonImage(deleteRequestRect, TexButton.Delete);
            if (deleteRequest && selectedRequest != null)
            {
                GameComponent_Customization.Instance.DeleteRequest(selectedRequest);
                RefreshDraggables();
            }
        }



        void SelectNextRequest(CustomizationRequest customizationRequest)
        {
            if (customizationRequest == null)
            {
                return;
            }
            filterBuffer = new List<string>();
            filterStream = "";
            selectedRequest = customizationRequest;
            textBuffer = customizationRequest.label;

            // try to load styles
            if (ModsConfig.IdeologyActive)
            {
                LoadStyles();
            }

            UpdatePawnKindDef();
        }

        void LoadStyles()
        {
            this.thingsAndStyles = new Dictionary<ThingDef, ThingStyleDef>();
            foreach (var item in selectedRequest.thingDefsAndStyles)
            {
                ThingDef thingdef = null;
                if (DefDatabase<ThingDef>.AllDefs.Any(x => x.defName == item.Key))
                {
                    thingdef = DefDatabase<ThingDef>.AllDefs.First(x => x.defName == item.Key);
                }
                if (thingdef == null)
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

        void DrawTextBox(Rect inRect)
        {
            int boxWidth = 300;
            Rect boxRect = new Rect(inRect.x + 200, 400, boxWidth, 50);
            string label = Widgets.TextEntryLabeled(boxRect, "WAW.PawnKindName".Translate(), textBuffer);
            textBuffer = label;
            if (label != selectedRequest.label)
            {
                selectedRequest.label = label;
                RefreshDraggables();
            }
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
                    DrawEquipments(headGearRect, selectedRequest.apparelRequests.Where(x => IsHeadGear(x)), SelectionType.head);
                else
                    DrawEquipments(headGearRect, null, SelectionType.head);

                if (HasTopGear(selectedRequest))
                    DrawEquipments(topGearRect, selectedRequest.apparelRequests.Where(x => IsTopGear(x)), SelectionType.top);
                else
                    DrawEquipments(topGearRect, null, SelectionType.top);

                if (HasBottomGear(selectedRequest))
                    DrawEquipments(bottomGearRect, selectedRequest.apparelRequests.Where(x => IsBottomGear(x)), SelectionType.bottom);
                else
                    DrawEquipments(bottomGearRect, null, SelectionType.bottom);

                if (HasUtility(selectedRequest))
                    DrawEquipments(utilRect, selectedRequest.apparelRequests.Where(x => IsUtility(x)), SelectionType.utils);
                else
                    DrawEquipments(utilRect, null, SelectionType.utils);

                DrawEquipments(weaponRect, new List<ThingDef>() { selectedRequest.weaponRequest }, SelectionType.weapons);



            }

        }


        #region SelectionPanel

        List<ThingDef> AllEquippedItems(List<ThingDef> selectedList)
        {
            return selectedList.Where(x =>
selectedRequest.apparelRequests.Any(a => a.defName == x.defName)
|| (selectedRequest.weaponRequest != null && selectedRequest.weaponRequest.defName == x.defName)).ToList();
        }

        void DrawSearchBar(Rect selectionPanelBar)
        {
            int boxHeight = 30;
            int boxWidth = (int)selectionPanelBar.width - boxHeight;
            Rect searchBoxRect = new Rect(selectionPanelBar.x, selectionPanelBar.yMin - boxHeight, boxHeight, boxHeight);
            Rect boxRect = new Rect(searchBoxRect.xMax + 5, selectionPanelBar.yMin - boxHeight, boxWidth, boxHeight);
            var newStream = Widgets.TextField(boxRect, filterStream);
            if(newStream != filterStream)
            {
                scrollPosition1 = new Vector2();
            }
            filterStream = newStream;
            char space = ' ';
            filterBuffer = newStream == "" ? new List<string>() : newStream.Split(space).ToList();
            Widgets.ButtonImage(searchBoxRect, TexButton.Search);
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
        void DrawSelectorItem(Rect rect, ThingDef item)
        {
            string graphicPath = item.graphicData.texPath;
            var stuff = selectedRequest.GetStuff(item);
            Texture2D itemTex = Widgets.GetIconFor(item, stuff);
            TryToDrawStyleIcon(ref itemTex, item);
            if (Equipped(item))
            {
                Widgets.DrawHighlight(rect);
                if (Mouse.IsOver(rect) && selectionType != SelectionType.weapons)
                {
                    itemTex = TexUI.DismissTex;
                }
            }
            Rect labelRect = new Rect(rect.xMax + 5, rect.y, 80, rect.height);
            Widgets.Label(labelRect, item.label);
            DrawApparelTooltip(rect);
            bool selectEquipment = Widgets.ButtonImage(
                rect,
                itemTex,
                baseColor: stuff == null ? Color.white : stuff.stuffProps.color);
            if (selectEquipment)
            {
                TryMakeStuffOptions(item);
            }
            TryToDrawStyleButton(rect, item);
        }
        bool ContainsAll(string target)
        {
            foreach (var buffer in filterBuffer)
            {
                if (!target.ToString().ToUpper().Contains(buffer.ToUpper()))
                {
                    return false;
                }
            }
            return true;
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
            if (filterBuffer.Count > 0)
            {
                listToDisplay = listToDisplay.Where(x => ContainsAll(x.label)).ToList();
            }

            foreach (var item in listToDisplay)
            {
                Rect itemRect = new Rect(viewRect.x + 10, viewRect.y + selectedIndex * (itemHeight + itemSpacing), itemHeight, itemHeight);
                DrawSelectorItem(itemRect, item);
                selectedIndex++;
            }

            Widgets.EndScrollView();
        }

        #endregion


        bool Equipped(ThingDef item)
        {
            return selectedRequest.apparelRequests.Any(x => x.defName == item.defName)
               || (selectedRequest.weaponRequest != null && selectedRequest.weaponRequest.defName == item.defName);
        }

       

        public override void Close(bool doCloseSound = true)
        {
            base.Close(doCloseSound);
            UpdatePawnKindDef();
            WarbandUtil.RefreshSoldierPawnKinds();
        }

        void UpdatePawnKindDef()
        {
            selectedRequest?.UpdatePawnKindDef();
        }

        void DrawEquipments(Rect rect, IEnumerable<ThingDef> equipments = null, SelectionType selectionType = SelectionType.none)
        {
            if (equipments != null && equipments.Count() > 0 && equipments.First() != null)
                WarbandUI.FillSlots(rect, equipments.Select(x => Widgets.GetIconFor(x)));
            bool selectBodyGroup = Widgets.ButtonInvisible(rect);
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
            filterStream = "";
            filterBuffer = new List<string>();
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


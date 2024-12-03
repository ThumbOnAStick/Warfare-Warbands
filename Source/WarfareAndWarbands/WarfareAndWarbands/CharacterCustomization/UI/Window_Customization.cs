

using CombatExtended;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
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


        private CustomizationRequest selectedRequest;
        private Vector2 scrollPosition;
        private Vector2 scrollPosition1;
        private bool displayEquipped;
        private string textBuffer;

        private static readonly int boxSize = 50;
        private static readonly int pawnkindSpacing = 5;
        private static readonly int pawnkindHeight = 50;
        private static readonly int pawnkindWidth = 150;


        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(800f, 500f);
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
            weaponsCache = DefDatabase<ThingDef>.AllDefs.Where(x => x.IsWeapon).ToList();
            weaponsCache.SortBy(x => x.BaseMarketValue);
            selectedList = apparelHeadsCache;
        }
        protected override void SetInitialSizeAndPosition()
        {
            base.SetInitialSizeAndPosition();

        }
        public override void DoWindowContents(Rect inRect)
        {
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

            Rect outRect = new Rect(50, 50, 150, 400);
            Rect viewRect = new Rect(outRect.x, outRect.y, outRect.width, (pawnkindHeight + pawnkindSpacing) * requests.Count + 5);
            Rect boxRect = new Rect(40, 40, 170, 420);
            Rect newRequestRect = new Rect(boxRect.xMax, boxRect.y, 30, 30);

            Widgets.DrawBox(boxRect, 1);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            int requestIndex = 0;
            foreach (var request in requests)
            {
                Rect buttonRect = new Rect(viewRect.x, viewRect.y - 5 + requestIndex * (pawnkindHeight + pawnkindSpacing), pawnkindWidth, pawnkindHeight);
                Rect requestRect = new Rect(viewRect.x + 10, viewRect.y + 5 + requestIndex * (pawnkindHeight + pawnkindSpacing), pawnkindWidth, pawnkindHeight);
                bool selectTargetRequest = Widgets.ButtonImage(buttonRect, TexUI.FastFillTex, new Color() { a = 0 });
                Widgets.DrawBox(buttonRect);
                Widgets.Label(requestRect, request.label);
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
                string pawnKindName = $"playermadekinddef{Find.TickManager.TicksGame}";
                string pawnKindLabel = "NewCustom";
                var customizationRequest = new CustomizationRequest(pawnKindName, pawnKindLabel);
                GameComponent_Customization.Instance.AddPawnKindDefFromRequest(pawnKindName, pawnKindLabel, customizationRequest);
                SelectNextRequest(customizationRequest);
            }
        }

        void SelectNextRequest(CustomizationRequest customizationRequest)
        {
            selectedRequest = customizationRequest;
            textBuffer = customizationRequest.label;
        }

        void DrawTextBox(Rect inRect)
        {
            int boxWidth = 200;
            Rect boxRect = new Rect(inRect.x + 200, 400, boxWidth, 50);
            string label = Widgets.TextEntryLabeled(boxRect, "WAW.PawnKindName".Translate(), textBuffer);
            textBuffer = label;
            selectedRequest.label = label;
            UpdatePawnKindDef();

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

        void DrawSelectionPanel(Rect inRect)
        {
            int itemHeight = 50;
            int optionHeight = 20;
            Rect outRect = new Rect(inRect.xMax - 200, 50, 150, 400);
            Rect viewRect = new Rect(outRect.x, outRect.y, outRect.width, (itemHeight + pawnkindSpacing) * selectedList.Count);
            Rect boxRect = new Rect(outRect.x - 10, outRect.y - 10, 170, 420);
            Rect optionRect = new Rect(boxRect.x - 150, boxRect.y - optionHeight, 150, optionHeight);
            Widgets.CheckboxLabeled(optionRect, "WAW.DisplayEquipped".Translate(), ref displayEquipped);
            Widgets.DrawBox(boxRect, 1);
            Widgets.BeginScrollView(outRect, ref scrollPosition1, viewRect);
            int selectedIndex = 0;
            var listToDisplay = selectedList;
            if (displayEquipped)
            {
                listToDisplay = AllEquippedItems(selectedList);
            }

            foreach (var item in listToDisplay)
            {
                Rect itemRect = new Rect(viewRect.x + 10, viewRect.y + selectedIndex * (itemHeight + pawnkindSpacing), itemHeight, itemHeight);
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
            Texture2D itemTex = item.graphicData.graphicClass == typeof(Graphic_Multi) || item.graphicData.graphicClass == typeof(Graphic_StackCount)
                ? null : ContentFinder<Texture2D>.Get(graphicPath);
            Texture2D weaponTex = itemTex != null ? itemTex : Texture2D.redTexture;

            if (selectedRequest.apparelRequests.Any(x => x.defName == item.defName)
               || (selectedRequest.weaponRequest!=null && selectedRequest.weaponRequest.defName == item.defName))
            {
                Widgets.DrawHighlight(rect);
            }

            Rect labelRect = new Rect(rect.xMax + 5, rect.y, 80, rect.height);
            Widgets.Label(labelRect, item.label);

            bool selectBodyGroup = Widgets.ButtonImage(rect, itemTex);
            if (selectBodyGroup)
            {
                if (selectionType != SelectionType.weapons)
                {
                    if (selectedRequest.apparelRequests.Any(x => x.defName == item.defName))
                    {
                        selectedRequest.apparelRequests.RemoveAll(x => x.defName == item.defName);
                    }
                    else
                    {
                        selectedRequest.apparelRequests.Add(item);
                    }
                }
                else
                {
                    selectedRequest.weaponRequest = item;
                }
                UpdatePawnKindDef();
            }

        }

        void UpdatePawnKindDef()
        {
            string name = selectedRequest.defName;
            if (DefDatabase<PawnKindDef>.AllDefs.Any(x => x.defName == name))
            {
                PawnKindDef targetDef = DefDatabase<PawnKindDef>.AllDefs.First(x => x.defName == name);
                targetDef.combatPower = selectedRequest.GetCombatPower();
                targetDef.label = selectedRequest.label;
            }
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

        }


        void SelectList(SelectionType selectionType = SelectionType.none)
        {
            this.selectionType = selectionType;
            if (selectionType == SelectionType.none)
            {
                return;
            }
            if (selectionType == SelectionType.head)
            {
                selectedList = apparelHeadsCache;
            }
            else if (selectionType == SelectionType.top)
            {
                selectedList = apparelTopsCache;
            }
            else if (selectionType == SelectionType.bottom)
            {
                selectedList = apparelBottomsCache;
            }
            else if (selectionType == SelectionType.utils)
            {
                selectedList = apparelUtilsCache;
            }
            else
            {
                selectedList = weaponsCache;
            }

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
            return apparel.apparel.bodyPartGroups.Any(b => b.defName == "Torso");
        }

        bool IsBottomGear(ThingDef apparel)
        {
            if (apparel.apparel == null)
                return false;
            return apparel.apparel.bodyPartGroups.Any(b => b.defName == "Legs") &&
                !apparel.apparel.bodyPartGroups.Any(b => b.defName == "Torso");
        }

        bool IsUtility(ThingDef apparel)
        {
            if (apparel.apparel == null)
                return false;
            return apparel.thingCategories.Any(c => c.defName == "ApparelUtility");
        }




    }
}


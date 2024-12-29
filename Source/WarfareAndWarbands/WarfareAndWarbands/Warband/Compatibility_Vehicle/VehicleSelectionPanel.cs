using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Vehicles;
using Verse;
using Verse.Sound;
using WarfareAndWarbands.Warband.UI;
using WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandUpgrades;
using WarfareAndWarbands.Warband.WarbandRecruiting;

namespace WarfareAndWarbands.Warband.Compatibility_Vehicle
{
    internal class VehicleSelectionPanel : Window
    {
        private Upgrade_Vehicle _upgrade;
        private string _filterBuffer;
        private readonly int elementWidth = 50;
        private readonly int elementHight = 50;
        private readonly int elementMargin = 20;
        private readonly int searchBarHeight = 50;
        private readonly int descriptionWidth = 200;

        public VehicleSelectionPanel()
        {
            this._upgrade = new Upgrade_Vehicle();
            _filterBuffer = "";
        }

        public VehicleSelectionPanel(Upgrade_Vehicle upgrade)
        {
            this._upgrade = upgrade;
            _filterBuffer = "";
        }

        public override Vector2 InitialSize => new Vector2(700, 500);

        public override void DoWindowContents(Rect inRect)
        {
            WarbandUI.DrawExitButton(this, inRect);
            var allVehicles =
                GenDefDatabase.GetAllDefsInDatabaseForDef(typeof(VehicleDef)).Cast<ThingDef>().OrderBy(v => v.BaseMarketValue);
            List<ThingDef> allGroundVehicles = new List<ThingDef>();
            for (int i = 0; i < allVehicles.Count(); i++)
            {
                var vehicle = (VehicleDef)allVehicles.ElementAt(i);
                if (vehicle.vehicleType == VehicleType.Land)
                {
                    allGroundVehicles.Add(vehicle);
                }
            }
            Rect outRect = new Rect(
            inRect.x,
            inRect.y + searchBarHeight,
            elementWidth * 2 + descriptionWidth + 30,
            inRect.height);
            Rect viewRect = new Rect(
                outRect.x,
                outRect.y,
                elementWidth * 2 + descriptionWidth + 10,
                (elementHight) * 2 * allGroundVehicles.Count() 
                );
            DrawSearchBar(outRect);
            Widgets.BeginScrollView(outRect, ref this._upgrade.scrollPosition, viewRect);
            var allFilteredVehicles = _filterBuffer != ""?allGroundVehicles.Where(x => x.label.Contains(_filterBuffer)): allGroundVehicles;
            for (int i = 0; i < allFilteredVehicles.Count(); i++)
            {
                var vehicle = allGroundVehicles.ElementAt(i);
                Rect textureRect = new Rect(
                    viewRect.x,
                    viewRect.y + (elementHight + elementMargin) * i,
                    elementWidth,
                    elementHight);
                Rect buttonRect = new Rect(textureRect.x + elementWidth, textureRect.y, descriptionWidth, elementHight);
                Rect labelRect1 = new Rect(buttonRect.xMax + 10, textureRect.y, elementWidth, elementHight);
                Rect labelRect = new Rect(labelRect1.x, textureRect.y + elementHight / 2, elementWidth, elementHight);
                Widgets.DrawTextureFitted(textureRect, vehicle.uiIcon, 1);
                if (Widgets.ButtonText(buttonRect, "WAW.BuyVehicle".Translate(vehicle.label)))
                {
                    if (WarbandUtil.TryToSpendSilverFromColony(Find.AnyPlayerHomeMap, (int)vehicle.BaseMarketValue))
                    {
                        SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
                        this._upgrade.AddVehicle(vehicle.defName);

                    }
                }
                if (_upgrade.Vehicles.ContainsKey(vehicle.defName))
                {
                    Widgets.Label(labelRect, _upgrade.Vehicles[vehicle.defName].ToString());
                }
                else
                {
                    Widgets.Label(labelRect, "0");
                }
                Widgets.Label(labelRect1, $"${vehicle.BaseMarketValue}".Colorize(new Color(.8f, .8f, .3f)));

            }
            Widgets.EndScrollView();

            Rect rightRect = new Rect(outRect.xMax, outRect.y, outRect.width, outRect.height);
            for (int i = 0; i < _upgrade.Vehicles.Count(); i++)
            {
                var ele = _upgrade.Vehicles.ElementAt(i);
                if(ele.Value < 1)
                {
                    continue;
                }
                if(!allGroundVehicles.Any(x => x.defName == ele.Key))
                {
                    continue; 
                }
                var vehicle = allGroundVehicles.Find(x => x.defName == ele.Key);
                Rect textureRect = new Rect(
                 rightRect.x,
                 rightRect.y + (elementHight + elementMargin) * i,
                 elementWidth,
                 elementHight);
                Rect labelRect = new Rect(textureRect.x, textureRect.y + elementHight / 2, elementWidth, elementHight);
                Widgets.DrawTextureFitted(textureRect, vehicle.uiIcon, 1);
                Widgets.Label(labelRect, ele.Value.ToString());
            }


        }


        void DrawSearchBar(Rect selectionPanelBar)
        {
            int boxHeight = 30;
            int boxWidth = (int)selectionPanelBar.width - boxHeight;
            Rect searchBoxRect = new Rect(selectionPanelBar.x, selectionPanelBar.yMin - boxHeight, boxHeight, boxHeight);
            Rect boxRect = new Rect(searchBoxRect.xMax + 5, selectionPanelBar.yMin - boxHeight, boxWidth, boxHeight);
            var newBuffer = Widgets.TextField(boxRect, _filterBuffer);
            if (newBuffer != _filterBuffer)
            {
                _upgrade.scrollPosition = new Vector2();
            }
            _filterBuffer = newBuffer;
            Widgets.ButtonImage(searchBoxRect, TexButton.Search);
        }

    }
}

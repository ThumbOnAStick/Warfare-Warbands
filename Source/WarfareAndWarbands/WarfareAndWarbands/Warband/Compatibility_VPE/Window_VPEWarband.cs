using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaPsycastsExpanded;
using Vehicles;
using Verse;
using WarfareAndWarbands.CharacterCustomization;
using WarfareAndWarbands.Warband.UI;
using WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandUpgrades;
using WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandUpgrades.Psycaster;

namespace WarfareAndWarbands.Warband.Compatibility_VPE
{
    public class Window_VPEWarband : Window
    {
        private List<CustomizationRequest> _requests;
        private Upgrade_Psycaster _psycasterUpgrade;
        private List<Def> _selectedDefs;
        private CustomizationRequest _selectedRequest;
        private Rect _pathsOutRect;
        private Rect _requestsOutRect;
        private Vector2 scrollPosition;
        private Vector2 scrollPosition1;
        private readonly float _margin = 10;
        private readonly float _offset = 50;
        private readonly float _elementWidth = 200;
        private readonly float _elementHight = 100;
        private readonly float _pathImageWidth = 50;
        private readonly float _labelWidth = 100;


        public Window_VPEWarband()
        {
            _requests = new List<CustomizationRequest>();
            _psycasterUpgrade = new Upgrade_Psycaster();
            _pathsOutRect = new Rect(_margin, _offset, _elementWidth, this.InitialSize.y);
            _requestsOutRect = new Rect(_pathsOutRect.xMax, _offset, _elementWidth, this.InitialSize.y);
            _selectedDefs = new List<Def>();    
        }
        public Window_VPEWarband(List<CustomizationRequest> requests, Upgrade_Psycaster psyUpgrade) : this()
        {
            _requests = requests;
            _psycasterUpgrade = psyUpgrade;
        }
        public override Vector2 InitialSize => new Vector2(700, 500);

        protected override void SetInitialSizeAndPosition()
        {
            base.SetInitialSizeAndPosition();
        }

        public override void DoWindowContents(Rect inRect)
        {
            WarbandUI.DrawExitButton(this, inRect); 

            var allPsyPaths = GenDefDatabase.GetAllDefsInDatabaseForDef(typeof(PsycasterPathDef));
            Rect pathViewRect = new Rect(_pathsOutRect.x, _pathsOutRect.y, _elementWidth - _margin, allPsyPaths.Count() * (_elementHight + Margin) + _elementHight);
            Rect titleRect = new Rect(_pathsOutRect.x, 0, _elementWidth, _offset);
            if (this._selectedDefs == null)
                Widgets.Label(titleRect, "WAW.SelectOnePath".Translate());
            else
                Widgets.Label(titleRect, "WAW.AlreadySelectedPaths".Translate(this._selectedDefs.Count));
            Widgets.BeginScrollView(_pathsOutRect, ref scrollPosition, pathViewRect);
            for (int i = 0; i < allPsyPaths.Count(); i++)
            {
                Rect invisibleButtonRect = new Rect(
                    pathViewRect.x,
                    pathViewRect.y + i * (_elementHight + Margin),
                    _elementWidth,
                    _elementHight);
                Rect pathRect = new Rect(
                    pathViewRect.x,
                    pathViewRect.y + i * (_elementHight + Margin),
                    _pathImageWidth,
                    _elementHight);
                Rect labelRect = new Rect(
                    pathRect.xMax + Margin,
                    pathRect.y + pathRect.height / 2 + -_pathImageWidth / 2,
                    _labelWidth,
                    _pathImageWidth
                    );
                if (!(allPsyPaths.ElementAt(i) is PsycasterPathDef pathDef))
                {
                    continue;
                }
                var pathImage = pathDef.altBackgroundImage ?? TexUI.FastFillTex;
                Widgets.DrawTextureFitted(pathRect, pathImage, 1f);
                Widgets.Label(labelRect, pathDef.label);
                TryToSelect(invisibleButtonRect, pathRect, pathDef);
            }
            Widgets.EndScrollView();

            Rect requestsViewRect = new Rect(_requestsOutRect.x, _requestsOutRect.y, _elementWidth - _margin, this._requests.Count() * (_elementHight + Margin));
            Rect titleRect1 = new Rect(_pathsOutRect.xMax, 0, _elementWidth, _offset);
            if (this._selectedRequest == null)
                Widgets.Label(titleRect1, "WAW.SelectOneRequest".Translate());
         
            Widgets.BeginScrollView(_requestsOutRect, ref scrollPosition1, requestsViewRect);
            for (int i = 0; i < this._requests.Count(); i++)
            {
                Rect invisibleButtonRect = new Rect(
                 requestsViewRect.x,
                 requestsViewRect.y + i * (_elementHight + Margin),
                 _elementWidth,
                 _elementHight);
                Rect labelRect = new Rect(
                 requestsViewRect.x + Margin,
                 requestsViewRect.y + i * (_elementHight + Margin) + Margin,
                 _pathImageWidth,
                 _elementHight);
                Widgets.Label(labelRect, _requests[i].label);
                if (Widgets.ButtonInvisible(invisibleButtonRect))
                {
                    _selectedRequest = _requests[i];
                }
                if (_selectedRequest == _requests[i])
                {
                    Widgets.DrawHighlight(invisibleButtonRect);
                }

            }
            Widgets.EndScrollView();

            Rect recruitButtonRect = new Rect(_requestsOutRect.xMax + Margin, _requestsOutRect.y + this.InitialSize.y / 2, _elementWidth, _offset);
            Rect requestCountRect = new Rect(recruitButtonRect.x + recruitButtonRect.width / 2 - Margin, recruitButtonRect.y - Margin, _elementWidth, _offset);

            DrawSlots(recruitButtonRect, requestCountRect);

            Widgets.Label(requestCountRect, $"{_psycasterUpgrade.Infos.Count}/{_psycasterUpgrade.CasterCap}");
            if (Widgets.ButtonText(recruitButtonRect, "WAW.RecruitCaster".Translate()))
            {
                if (_selectedRequest == null)
                {
                    Messages.Message("WAW.SelectOneRequest".Translate(), MessageTypeDefOf.RejectInput);
                    return;
                }
                if (_selectedDefs == null)
                {
                    Messages.Message("WAW.SelectOnePath".Translate(), MessageTypeDefOf.RejectInput);
                    return;
                }
                PsycasterInfo info = new PsycasterInfo(_selectedRequest, _selectedDefs.Select(x => x.defName).ToList());
                this._psycasterUpgrade.TryToAddInfo(info);
            }

           
            float eltexWidth = 20;
            float labelWidth = 100;
            Rect eltexLableRect = new Rect(recruitButtonRect.x, recruitButtonRect.y - _elementHight * 2, labelWidth, eltexWidth);
            Rect eltexRect = new Rect(eltexLableRect.xMax, eltexLableRect.y, eltexWidth, eltexWidth);
            Widgets.DrawTextureFitted(eltexRect, VPE_DefOf.VPE_Eltex.uiIcon, 1f);
            Widgets.Label(eltexLableRect, "WAW.EltexCost".Translate(this._psycasterUpgrade.GetEltexCost()));

            Rect tipRect = new Rect(eltexLableRect.x, eltexLableRect.yMax + Margin, _elementWidth, _elementWidth);
            Widgets.Label(tipRect, "WAW.EltexTip".Translate(this._psycasterUpgrade.Infos.Count));

        }

        void DrawSlots(Rect recruitButtonRect, Rect requestCountRect)
        {

            for (int i = 0; i < this._psycasterUpgrade.CasterCap; i++)
            {
                Rect boxRect = new Rect(recruitButtonRect.x + i * (_pathImageWidth + Margin), requestCountRect.y - _elementHight, _pathImageWidth, _elementHight);
                if (this._psycasterUpgrade.Infos.Count > i)
                {
                    if (Mouse.IsOver(boxRect))
                    {
                        Widgets.DrawTextureFitted(boxRect, TexUI.DismissTex, 1f);
                    }
                    else
                    {
                        DrawPsyPath(boxRect, this._psycasterUpgrade.Infos[i]);
                    }
                    if (Widgets.ButtonInvisible(boxRect))
                    {
                        this._psycasterUpgrade.Infos.Remove(_psycasterUpgrade.Infos[i]);
                    }
                }
                Widgets.DrawBox(boxRect);
            }
        }



        void DrawPsyPath(Rect rect, PsycasterInfo info)
        {
            WarbandUI.FillSlots(rect, info.AllPsycasterPathTex());
        }



        void TryToSelect(Rect outerRect, Rect rect, Def pathDef)
        {
            Predicate<Def> p = x => x.defName == pathDef.defName;
            bool selected = _selectedDefs.Any(p);
            bool select = Widgets.ButtonInvisible(outerRect);
            if (select)
            {
                if (selected)
                {
                    _selectedDefs.RemoveAll(p);
                }
                else
                {
                    _selectedDefs.Add(pathDef);
                }
            }
            if (selected)
            {
                Widgets.DrawHighlight(outerRect);
                if (Mouse.IsOver(outerRect))
                {
                    Widgets.DrawTextureFitted(rect, TexUI.DismissTex, 1f);
                }
            }
        }


    }
}

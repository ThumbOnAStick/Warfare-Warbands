using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using WarfareAndWarbands.Warband;

namespace WarfareAndWarbands.Warfare.UI
{
    public class Window_WAW_Comm : Window
    {
        private int currentPanel = 0;
        private List<TabRecord> _tabs = new List<TabRecord>();
        private readonly Map map;
        private static readonly int warbandPanel = 0;
        private static readonly int leaguePanel = 1;
        private static readonly int warfarePanel = 2;
        public Window_WAW_Comm()
        {
            this.map = Find.AnyPlayerHomeMap;
        }
        public Window_WAW_Comm(Map map)
        {
            this.map = map;
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(500f, 500f);
            }
        }

        protected override void SetInitialSizeAndPosition()
        {
            base.SetInitialSizeAndPosition();
            WAWUI.InitWindow();
        }

        void DrawSelectionMenu(Rect inRect)
        {
            Rect menuRect = new Rect(0, 50, inRect.width, inRect.height);
            Widgets.DrawMenuSection(menuRect);
            TabDrawer.DrawTabs(menuRect, this._tabs, 200f);
        }
        public void DoWarband()
        {
            currentPanel = warbandPanel;
        }

        public void DoLeague()
        {
            currentPanel = leaguePanel;
        }

        public void DoWarfare()
        {
            currentPanel = warfarePanel;
        }
        public override void PostOpen()
        {
            base.PostOpen();
            //Init Tabs 
            _tabs = new List<TabRecord>();
            this._tabs.Add(new TabRecord("WAW.MainPanelWarband".Translate(), () => { this.DoWarband(); }, () => this.currentPanel == warbandPanel));
            this._tabs.Add(new TabRecord("WAW.MainPanelLeague".Translate(), () => { this.DoLeague(); }, () => this.currentPanel == leaguePanel));
            this._tabs.Add(new TabRecord("WAW.MainPanelWarfare".Translate(), () => { this.DoWarfare(); }, () => this.currentPanel == warfarePanel));

        }
        public override void DoWindowContents(Rect inRect)
        {
            DrawSelectionMenu(inRect);
            WAWUI.DoWindowContents(inRect, this, map, this.currentPanel);
        }


    }
}

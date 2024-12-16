using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using WarfareAndWarbands.Warband;

namespace WarfareAndWarbands.QuickRaid.UI
{
    public class Window_QuickRaid : Window
    {

        private Vector2 scrollPosition;
        static readonly float descriptionHeight = 50f;
        static readonly float buttonWidth = 200f;
        private string filterBuffer;

        public Window_QuickRaid()
        {
            filterBuffer = "";
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(500f, 500f);
            }
        }

        public override void PostOpen()
        {
            base.PostOpen();
            GameComponent_WAW.Instance.SetAlreadyUseQuickRaid();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect exitButtonRect = new Rect(430, 0, 30, 30);
            bool exit = Widgets.ButtonImage(exitButtonRect, TexButton.CloseXSmall);
            if (exit)
            {
                this.Close();
            }

            Rect renameLabelRect = new Rect(30, 30f, 200, 50);
            Widgets.Label(renameLabelRect,"WAW.YouCanRename".Translate());

            var allAvialablePlayerWarbands = WarbandUtil.AllActivePlayerWarbands();
            if(allAvialablePlayerWarbands.Count() < 1)
            {
                Rect zeroFoundRect = new Rect(inRect.x, exitButtonRect.yMax + 50f, inRect.width, 200f);
                Widgets.Label(zeroFoundRect, "WAW.ZeroFound".Translate());
                return;
            }
            Rect outRect = new Rect(inRect.x, exitButtonRect.yMax + 50f, inRect.width, 200f);
            Rect viewRect = new Rect(inRect.x, outRect.y, inRect.width - 30f, (float)(allAvialablePlayerWarbands.Count() * (descriptionHeight + 10)));
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            if (filterBuffer != "")
            {
                allAvialablePlayerWarbands = allAvialablePlayerWarbands.Where(x => x.GetCustomName().Contains(filterBuffer)).ToList();
            }
            float currentHeight = outRect.y;
            foreach (var warband in allAvialablePlayerWarbands)
            {
                bool selectWarband = Widgets.ButtonText(
                    new Rect(
                    (outRect.x + outRect.xMax - buttonWidth) / 2,
                    currentHeight,
                    buttonWidth,
                    descriptionHeight),
                    warband.GetCustomName());
                currentHeight += descriptionHeight + 10;

                if (selectWarband)
                {
                    CameraJumper.TryJump(CameraJumper.GetWorldTarget(warband), CameraJumper.MovementMode.Pan);
                    Find.WorldSelector.Select(warband);
                    this.Close();
                }
            }
            Widgets.EndScrollView();
            DrawSearchBar(new Rect(outRect.x, outRect.yMax + 50, 200, 50));

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
    }
}

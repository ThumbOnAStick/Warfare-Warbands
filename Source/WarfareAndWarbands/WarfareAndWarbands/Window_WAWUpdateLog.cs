using System;
using UnityEngine;
using Verse;

namespace WarfareAndWarbands
{
    internal class Window_WAWUpdateLog : Window
    {
        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(500f, 800f);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect exitButtonRect = new Rect(430, 0, 30, 30);
            bool exit = Widgets.ButtonImage(exitButtonRect, TexButton.CloseXSmall);
            if (exit)
            {
                this.Close();
            }

            int iconSize = 100;
            Rect IconRect = new Rect((inRect.xMax + inRect.x) / 2 - iconSize / 2, 0, iconSize, iconSize);
            Widgets.DrawTextureFitted(IconRect, WAWTex.ModIcon, 1);

            int textWidth = 150;
            Rect showAgainRect = new Rect(30, 0, textWidth, 50);
            Widgets.CheckboxLabeled(showAgainRect, "WAW.DontShowAgain".Translate(), ref WAWSettings.everReadUpdateLog);

            Text.Font = GameFont.Medium;
            var title = "WAW.Title".Translate();
            int titleWidth = (int)title.GetWidthCached();
            Rect titleRect = new Rect((inRect.xMax + inRect.x) / 2 - titleWidth / 2, 125, titleWidth, 50);
            Widgets.Label(titleRect, title);

            Text.Font = GameFont.Small;
            var content = "WAW.Desc".Translate();
            int contentWidth = (int)title.GetWidthCached();
            Rect contentRect = new Rect((inRect.xMax + inRect.x) / 2 - contentWidth / 2, titleRect.yMax + 50,
                contentWidth, 100);
            Widgets.Label(contentRect, content);


            int content1Width = 400;
            Rect content1Rect = new Rect((inRect.xMax + inRect.x) / 2 - content1Width / 2, contentRect.yMax + 50,
               content1Width, 300);
            Widgets.DrawTextureFitted(content1Rect, WAWTex.ScreenShot,1);
        }
    }
}

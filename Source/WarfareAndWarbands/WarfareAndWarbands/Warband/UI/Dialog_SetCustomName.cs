using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace WarfareAndWarbands.Warband.UI
{
    public class Dialog_SetCustomName : Window
    {

        private string textBuffer;

        private Warband warband;

        public override Vector2 InitialSize => new Vector2(500f, 200f);


        public Dialog_SetCustomName()
        {

        }

        public Dialog_SetCustomName(Warband warband)
        {
            this.warband = warband;
            textBuffer = warband.GetCustomName();
        }

        public override void DoWindowContents(Rect inRect)
        {
            //Draw close button
            Rect exitButtonRect = new Rect(430, 0, 30, 30);
            bool exit = Widgets.ButtonImage(exitButtonRect, TexButton.CloseXSmall);
            if (exit)
            {
                this.Close();
            }

            int boxWidth = 300;
            int boxHight = 50;
            Rect boxRect = new Rect(
                (inRect.x + inRect.xMax) / 2 - boxWidth / 2,
                 (inRect.y + inRect.yMax) / 2 - boxHight / 2,
                boxWidth,
                50);
            textBuffer = Widgets.TextField(boxRect, textBuffer);
            warband?.SetCustomName(textBuffer);

        }

        public override void PostClose()
        {
            base.PostClose();
        }
    }
}

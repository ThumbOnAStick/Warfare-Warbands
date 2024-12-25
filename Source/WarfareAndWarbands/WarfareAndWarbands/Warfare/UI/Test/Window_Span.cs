using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using Verse;
using WarfareAndWarbands.Warfare.UI.Test;

namespace WarfareAndWarbands.Warfare.UI
{
    public class Window_Span : Window
    {
        private readonly List<DraggableButton> buttons;

        public Window_Span()
        {
            buttons = new List<DraggableButton>()
            {
                new DraggableButton(TexButton.Play,new Rect(0, 0, 50, 50)),
                new DraggableButton(TexButton.Add,new Rect(0, 60, 50, 50)),
                new DraggableButton(TexButton.Banish,new Rect(0, 120, 50, 50)),
                new DraggableButton(TexButton.CloseXSmall,new Rect(0, 180, 50, 50)),
            };
        }

        public override void DoWindowContents(Rect inRect)
        {
            bool dragged = false;
            for (int i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i];    
                dragged = button.Update(dragged, buttons);
            }
        }

    }
}

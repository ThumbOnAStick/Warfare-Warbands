using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;

namespace WarfareAndWarbands.Warfare.UI.Test
{
    public class DraggableButton
    {
        public Rect IdleRect => this.idleRect;
        private bool dragging;
        private int mouseOverFrameCounter;
        private Vector2 mouseOffset;
        private Rect idleRect;
        private Texture2D buttonTexture;
        private string text;
        private bool drawBox = true;
        private Action action;
        private Action<DraggableButton> onSwapped;
        private Func<bool> shouldHighlight;

        public DraggableButton()
        {
            dragging = false;
            mouseOverFrameCounter = 0;
            idleRect = new Rect();
        }

        public DraggableButton(Texture2D buttonTexture, Rect idleRect, Action act = null, Func<bool> shouldHighlight = null)
        {
            dragging = false;
            mouseOverFrameCounter = 0;
            this.buttonTexture = buttonTexture;
            this.idleRect = idleRect;
            this.action = act;
            this.shouldHighlight = shouldHighlight;
        }
        public DraggableButton(string text, Rect idleRect, Action action = null,Action<DraggableButton> onSwapped = null, Func<bool> shouldHighlight = null)
        {
            dragging = false;
            mouseOverFrameCounter = 0;
            this.idleRect = idleRect;
            this.text = text;
            this.action = action;
            this.shouldHighlight = shouldHighlight;
            this.onSwapped = onSwapped;
        }

        public DraggableButton(string text, bool drawBox, Rect idleRect, Action act = null, Func<bool> shouldHighlight = null)
        {
            dragging = false;
            mouseOverFrameCounter = 0;
            this.idleRect = idleRect;
            this.text = text;
            this.drawBox = drawBox;
            this.action = act;
            this.shouldHighlight = shouldHighlight;

        }


        public bool Update(bool dragged, List<DraggableButton> buttons)
        {
            DrawIdle();
            if (dragged)
            {
                return false;
            }
            ValidateDrag();
            DrawWhileDragging();
            if (this.dragging)
            {
                TryToReplaceOthers(buttons);
            }
            return dragging;
        }

        void DrawIdle()
        {
            if (mouseOverFrameCounter < 10)
                Draw(idleRect);
            DrawHighlight();

        }
        void DrawHighlight()
        {
            if (this.shouldHighlight != null && this.shouldHighlight.Invoke())
            {
                Widgets.DrawHighlight(idleRect);
            }
        }
        void ValidateDrag()
        {
            if (Input.GetMouseButtonDown(0) && Mouse.IsOver(idleRect))
            {
                dragging = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                TryToDoAction();
                dragging = false;
                mouseOverFrameCounter = 0;
            }
        }
        void TryToDoAction()
        {
            if (Mouse.IsOver(idleRect))
                this.action?.Invoke();
        }

        void DrawWhileDragging()
        {
            if (dragging)
            {
                IncrementMouseOverFrame();
                TryDrawDragged();
            }
        }

        void IncrementMouseOverFrame()
        {
            if (Mouse.IsOver(idleRect))
            {
                mouseOverFrameCounter++;
            }
        }

        void Draw(Rect inRect)
        {
            if (this.buttonTexture != null)
            {
                Widgets.DrawTextureFitted(inRect, this.buttonTexture, 1f);
            }
            if (this.text != null)
            {
                Widgets.Label(new Rect(inRect.position + Vector2.one * 5, inRect.size), this.text);
            }
            if (drawBox)
            {
                Widgets.DrawBox(inRect);
            }
        }

        void TryDrawDragged()
        {
            if (mouseOverFrameCounter > 10)
            {
                Draw(GetMouseRect());
            }
            else
            {
                mouseOffset = idleRect.position - Event.current.mousePosition;
            }
        }

        void TryToReplaceOthers(List<DraggableButton> buttons)
        {
            foreach (var button in buttons)
            {
                if (TryToReplaceOther(button))
                {
                    return;
                }
            }
        }

        bool TryToReplaceOther(DraggableButton button)
        {
            if (button != null)
            {
                if (TryToReplace(button))
                {
                    return true;
                }
            }
            return false;
        }

        public Rect GetMouseRect()
        {
            return new Rect(Event.current.mousePosition + mouseOffset, idleRect.size);
        }

        public void SetIdleRect(Rect value)
        {
            this.idleRect = value;
        }

        public bool TryToReplace(DraggableButton otherButton)
        {
            var otherRect = otherButton.IdleRect;
            var oldRect = this.IdleRect;
            if (Vector2.Distance(GetMouseRect().position, otherRect.position) < otherRect.height / 2)
            {
                SetIdleRect(otherRect);
                otherButton.SetIdleRect(oldRect);
                this.onSwapped?.Invoke(otherButton);
                return true;
            }
            return false;
        }

    }
}

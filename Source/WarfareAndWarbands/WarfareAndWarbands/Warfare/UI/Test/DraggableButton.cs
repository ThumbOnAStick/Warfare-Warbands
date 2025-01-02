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
        private bool _beingDragged;
        private int _mouseDraggingFrameCounter;
        private int _mouseOverFrameCounter;

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
            _beingDragged = false;
            _mouseDraggingFrameCounter = 0;
            idleRect = new Rect();
        }

        public DraggableButton(Texture2D buttonTexture, Rect idleRect, Action act = null, Func<bool> shouldHighlight = null)
        {
            _beingDragged = false;
            _mouseDraggingFrameCounter = 0;
            this.buttonTexture = buttonTexture;
            this.idleRect = idleRect;
            this.action = act;
            this.shouldHighlight = shouldHighlight;
        }
        public DraggableButton(string text, Rect idleRect, Action action = null, Action<DraggableButton> onSwapped = null, Func<bool> shouldHighlight = null)
        {
            _beingDragged = false;
            _mouseDraggingFrameCounter = 0;
            this.idleRect = idleRect;
            this.text = text;
            this.action = action;
            this.shouldHighlight = shouldHighlight;
            this.onSwapped = onSwapped;
        }

        public DraggableButton(string text, bool drawBox, Rect idleRect, Action act = null, Func<bool> shouldHighlight = null)
        {
            _beingDragged = false;
            _mouseDraggingFrameCounter = 0;
            this.idleRect = idleRect;
            this.text = text;
            this.drawBox = drawBox;
            this.action = act;
            this.shouldHighlight = shouldHighlight;

        }


        public bool Update(bool otherDragged, List<DraggableButton> buttons)
        {
            DrawIdle(otherDragged);
            if (otherDragged)
            {
                return false;
            }
            ValidateDrag();
            DrawWhileDragging();
            if (this._beingDragged)
            {
                TryToReplaceOthers(buttons);
            }
            return _beingDragged;
        }

        void DrawIdle(bool otherDragged)
        {
            if (_mouseDraggingFrameCounter < 10)
                Draw(idleRect, otherDragged);
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
            if (Mouse.IsOver(idleRect))
            {
                if (_mouseOverFrameCounter < 30)
                    _mouseOverFrameCounter++;
                if (Input.GetMouseButtonDown(0))
                {
                    _beingDragged = true;
                }
            }
            else
            {
                _mouseOverFrameCounter = 0;
            }
            if (Input.GetMouseButtonUp(0))
            {
                TryToDoAction();
                _beingDragged = false;
                _mouseDraggingFrameCounter = 0;

            }
        }
        void TryToDoAction()
        {
            if (Mouse.IsOver(idleRect))
                this.action?.Invoke();
        }

        void DrawWhileDragging()
        {
            if (_beingDragged)
            {
                IncrementMouseOverFrame();
                TryDrawDragged();
            }
        }

        void IncrementMouseOverFrame()
        {
            if (Mouse.IsOver(idleRect))
            {
                _mouseDraggingFrameCounter++;
            }
        }

        void Draw(Rect inRect, bool otherDragged)
        {
            if (drawBox)
            {
                Widgets.DrawBox(inRect);
            }
            if (Mouse.IsOver(idleRect) && !_beingDragged && !otherDragged && _mouseOverFrameCounter > 20)
            {
                Widgets.Label(new Rect(inRect.position + Vector2.one * 5, inRect.size), "WAW.DRAG".Translate());
                return;
            }
            if (this.buttonTexture != null)
            {
                Widgets.DrawTextureFitted(inRect, this.buttonTexture, 1f);
            }
            if (this.text != null)
            {
                Widgets.Label(new Rect(inRect.position + Vector2.one * 5, inRect.size), this.text);
            }

        }

        void TryDrawDragged()
        {
            if (_mouseDraggingFrameCounter > 10)
            {
                Draw(GetMouseRect(), otherDragged: false);
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

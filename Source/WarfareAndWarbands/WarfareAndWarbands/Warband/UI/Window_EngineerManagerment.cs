using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandUpgrades;
using WarfareAndWarbands.Warfare.UI;

namespace WarfareAndWarbands.Warband.UI
{
    public class Window_EngineerManagerment : Window
    {
        private readonly Upgrade_Engineer _engineerUpgrade;
        private Vector2 scrollPosition1;
        private readonly List<string> _shellNumbuffers;



        public Window_EngineerManagerment()
        {
            _engineerUpgrade = new Upgrade_Engineer();
            _shellNumbuffers = new List<string>();  
            for(int i = 0; i < GameComponent_WAW.Instance.MortarShells.Count; i++)
            {
                _shellNumbuffers.Add("0");
            }
        }

        public Window_EngineerManagerment(Upgrade_Engineer upgrade) : this()
        {
            _engineerUpgrade = upgrade;
        }

        public override Vector2 InitialSize => new Vector2(600, 700);

        public override void DoWindowContents(Rect inRect)
        {
            // Draw closeX
            this.DrawCloseButton(inRect);

            DrawShellManagement(inRect);


        }

        void DrawShellManagement(Rect inRect)
        {
            // Check shell count
            var shellDefs = GameComponent_WAW.Instance.MortarShells;
            if (shellDefs.Count < 1)
            {
                GUI.color = Color.red;
                Widgets.Label(inRect.BottomHalf(), "WAW.NoShellDefs".Translate());
                GUI.color = Color.white;
                return;
            }

            // List all shell defs
            Rect shellSelectionRect = inRect.BottomPart(.9f);
            Rect viewRect = new Rect(shellSelectionRect.x, shellSelectionRect.y + Margin, shellSelectionRect.width - 16f, shellDefs.Count * 30f);
            Widgets.BeginScrollView(shellSelectionRect, ref scrollPosition1, viewRect);
            for (int i = 0; i < shellDefs.Count; i++)
            {
                var shellDef = shellDefs[i];
                Rect rowRect = new Rect(0, i * 30f + viewRect.y, viewRect.width, 30f);
                Widgets.DrawTextureFitted(rowRect.LeftPart(.2f), shellDef.uiIcon, 1);
                Widgets.Label(rowRect.LeftHalf().RightHalf(), shellDef.LabelCap);
                _shellNumbuffers[i] = _engineerUpgrade.Shells.TryGetValue(shellDef, out int val) ? val.ToString() : "0";
                string shellCount = Widgets.TextField(rowRect.RightHalf().LeftHalf(), _shellNumbuffers[i]);
                if (int.TryParse(shellCount, out int number))
                {
                    _shellNumbuffers[i] = shellCount;
                }
                else
                {
                    _shellNumbuffers[i] = "0";

                }
                _engineerUpgrade.TryToChangeShells(shellDef, number);

            }
            Widgets.EndScrollView();

            // Inform player about how to fire artillery
            Rect infoRect = inRect.BottomPart(.2f);
            Widgets.Label(infoRect, "WAW.HowToFireArtillery".Translate());
        }


    }
}

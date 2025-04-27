using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;
using WarbandWarfareQuestline.League.MinorFactions;
using WarfareAndWarbands;

namespace WarbandWarfareQuestline.League.UI
{
    public class Window_LeagueManagement : Window
    {
        private const float rowHeight = 30f;
        private const float iconWidth = 20f;
        private const float factionNameWidth = 200f;
        private const float traitWidth = 100f;

        private readonly List<MinorFactionSettlement> _factionSettlements;

        public  Window_LeagueManagement()
        {
            _factionSettlements = GameComponent_League.Instance.MinorFactionSettlements;   
        }

        void CreateBanishWindow(MinorFactionSettlement mS)
        {
            var pros = new List<MinorFaction>();
            var dissenters = new List<MinorFaction>();
            foreach (var factionSettlement in _factionSettlements)
            {
                if(factionSettlement == mS)
                {
                    continue;
                }
                if (factionSettlement.MinorFaction.Trait.hatedTrait != mS.MinorFaction.Trait && 
                    factionSettlement.MinorFaction.Trait.dislikedCategory == mS.MinorFaction.Trait.dislikedCategory)
                {
                    dissenters.Add(factionSettlement.MinorFaction);
                }
                else
                {
                    pros.Add(factionSettlement.MinorFaction);
                }
            }
        
            Find.WindowStack.Add(new Window_KickoutCongress(pros, dissenters, mS));
        }


        void DrawOneRow(float y, MinorFactionSettlement mS)
        {
            MinorFaction m = mS.MinorFaction;
            float x = 0;
            //Draw icon
            Rect iconRect = new Rect(x, y, iconWidth, iconWidth);
            Widgets.DrawHighlight(iconRect);
            GUI.color = m.FactionColor;
            Widgets.DrawTextureFitted(iconRect, m.GetFactionIcon(), 1f);
            GUI.color = Color.white;
            //Draw label
            x += Margin + iconWidth;
            Widgets.Label(new Rect(x, y, factionNameWidth, rowHeight), m.FactionName);
            //Draw trait
            x += factionNameWidth + Margin;
            Rect traitRect = new Rect(x, y, traitWidth, rowHeight);
            TooltipHandler.TipRegion(traitRect, m.Trait.description);
            Widgets.Label(traitRect, m.Trait.LabelCap);
            //Draw locate button
            x += traitWidth + Margin;
            Rect locaterRect = new Rect(x, y, iconWidth, iconWidth);
            TooltipHandler.TipRegion(locaterRect, "WAW.LocateFaction".Translate(m.FactionName));
            if(Widgets.ButtonImage(locaterRect, TexButton.Search))
            {
                CameraJumper.TryJumpAndSelect(mS);
                Close();
            }
            //Draw kickout button
            x += iconWidth + Margin;
            Rect kickoutRect = new Rect(x, y, iconWidth, iconWidth);
            TooltipHandler.TipRegion(kickoutRect, "WAW.KickoutFaction".Translate(m.FactionName));
            if (Widgets.ButtonImage(kickoutRect, TexButton.Banish))
            {
                CreateBanishWindow(mS);
                Close();
            }
 
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (_factionSettlements.Count <= 0)
            {
                // Draw empty message
                GUI.color = Color.red;
                Widgets.Label(inRect.BottomHalf(), "WAW.NoFactionsInLeague".Translate());
                GUI.color = Color.white;
                return;
            }
            for (int i = 0; i < _factionSettlements.Count; i++)
            {
                var factionSettlement = _factionSettlements[i];
                float y = i * rowHeight;
                DrawOneRow(y, factionSettlement);
                if (factionSettlement == null)
                {
                    Close();
                    return;
                }
            }

            int closeButtonSize = 25;
            Rect closeButtonRect = new Rect(inRect.xMax - closeButtonSize, 0, closeButtonSize, closeButtonSize);
            if (Widgets.ButtonImage(closeButtonRect, TexButton.CloseXSmall))
            {
                Close();
            }
        }
    }
}



using RimWorld;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using WarfareAndWarbands.Warband.UI;

namespace WarbandWarfareQuestline.League.MinorFactions
{
    public static class MinorFactionDrawer
    {
        public static void DrawFactionIcon(Rect rect, MinorFaction f)
        {
            GUI.color = f.FactionColor;
            Widgets.DrawTextureFitted(rect, f.GetFactionIcon(), 1);
            GUI.color = Color.white;
        }

        public static void DrawFactionIconCircularDisplay(Rect rect, IEnumerable<MinorFaction> factions, float iconDrawSize, float distributionFactor)
        {
            Vector2 center = rect.center;
            Vector2 iconOffset = Vector2.one * iconDrawSize / 2;
            //Draw one faction
            
            if(factions.Count() == 1 && factions.First() != null)
            {
                DrawFactionIcon(new Rect(center - iconOffset, Vector2.one * iconDrawSize), factions.First());
                return;
            }

            //Draw multiple factions
            float degreeStep = 360f / factions.Count();
            float radius = Mathf.Min(rect.width, rect.height) * distributionFactor;
            foreach (var (faction, index) in factions.Select((f, i) => (f, i)))
            {
                if (faction == null) continue;
                float angle = (degreeStep * index + 270) * Mathf.Deg2Rad;
                Vector2 position = new Vector2(center.x + Mathf.Cos(angle) * radius, center.y + Mathf.Sin(angle) * radius) - Vector2.one * iconDrawSize / 2;
                DrawFactionIcon(new Rect(position.x, position.y, iconDrawSize, iconDrawSize), faction);
            }
        }

    }
}

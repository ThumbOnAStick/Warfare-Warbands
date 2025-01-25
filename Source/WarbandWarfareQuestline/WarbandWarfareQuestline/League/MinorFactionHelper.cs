using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WarbandWarfareQuestline.League
{
    public static class MinorFactionHelper
    {
        public static MinorFaction GenerateMinorFaction(FactionTraitDef trait, TechLevel level, Color factionColor)
        {
            MinorFaction minorFaction = new MinorFaction(trait, level, factionColor);
            minorFaction.Init();
            GameComponent_League.Instance.FactionsTemp.Add(minorFaction);
            return minorFaction;
        }

        public static void JoinPlayer(this MinorFaction f)
        {
            bool ContiansFaction(MinorFaction x) { return x.FactionID == f.FactionID; }
            if (GameComponent_League.Instance.FactionsTemp.Any(ContiansFaction))
            {
                GameComponent_League.Instance.FactionsTemp.RemoveAll(ContiansFaction);
                if (!GameComponent_League.Instance.Factions.Contains(f))
                    GameComponent_League.Instance.Factions.Add(f);
            }
        }

    }
}

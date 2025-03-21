﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WarbandWarfareQuestline.League.MinorFactions;

namespace WarbandWarfareQuestline.League
{
    public class PlayerLeague : IExposable
    {
        private List<MinorFaction> factions;

        public PlayerLeague()
        {
            factions = new List<MinorFaction>();
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref factions, "factions", LookMode.Deep);
        }
    }
}

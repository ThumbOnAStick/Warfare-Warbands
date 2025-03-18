﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarbandWarfareQuestline.League.Policies;

namespace WarbandWarfareQuestline
{
    public class FactionTraitDef : Def
    {
        public int commonality;

        //supply bonus per day
        public float supplyBonus;

        public PolicyCategoryDef dislikedCategory;
 
    }
}

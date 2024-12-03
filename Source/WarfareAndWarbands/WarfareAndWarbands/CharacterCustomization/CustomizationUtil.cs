using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using System.Reflection;
using UnityEngine;

namespace WarfareAndWarbands.CharacterCustomization
{
    public static class CustomizationUtil
    {
        public static void DefineSkillRange(this SkillRange skillRange, SkillDef skillDef,
            IntRange skillRangeInt)
        {
            typeof(SkillRange).GetField("range",
              BindingFlags.NonPublic | BindingFlags.Instance).SetValue(skillRange, skillRangeInt);
            typeof(SkillRange).GetField("skill",
                BindingFlags.NonPublic | BindingFlags.Instance).SetValue(skillRange, skillDef);

        }

        public static PawnKindDef GenerateDefaultKindDef(CustomizationRequest customizationRequest)
        {

            var defaultKindDef = new PawnKindDef
            {
                defName = customizationRequest.defName,
                combatPower = customizationRequest.GetCombatPower(),
                label = customizationRequest.label,
                isFighter = true,
                race = ThingDefOf.Human,
                defaultFactionType = FactionDefOf.OutlanderCivil
            };
            if (DefDatabase<PawnKindDef>.GetNamed(defaultKindDef.defName, false) == null)
                DefDatabase<PawnKindDef>.Add(defaultKindDef);
            AddSkills(defaultKindDef);

            return defaultKindDef;
        }

        public static void AddSkills(PawnKindDef defaultKindDef)
        {
            defaultKindDef.skills = new List<SkillRange>();
            AddSkill(SkillDefOf.Shooting, new IntRange(8, 10), defaultKindDef);
            AddSkill(SkillDefOf.Melee, new IntRange(8, 10), defaultKindDef);
        }

        public static void AddSkill(SkillDef skillDef, IntRange skillRangeInt, PawnKindDef defaultKindDef)
        {
            SkillRange skillRange = new SkillRange();
            skillRange.DefineSkillRange(skillDef, new IntRange(8, 10));
            defaultKindDef.skills.Add(skillRange);
        }



    }
}

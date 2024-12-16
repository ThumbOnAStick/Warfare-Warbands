using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using System.Reflection;
using UnityEngine;
using static RimWorld.PsychicRitualRoleDef;

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
                defaultFactionType = FactionDefOf.OutlanderCivil,
                modExtensions = PawnKindDefOf.Pirate.modExtensions,
                allowOldAgeInjuries = false,
                maxGenerationAge = 30,
            };
            AddSkills(defaultKindDef);

            return defaultKindDef;
        }

        public static string StyleStringFor(ThingDef thingDef, Dictionary<ThingDef, ThingStyleDef> dic)
        {
            if (!dic.ContainsKey(thingDef) || dic[thingDef] == null)
            {
                return "WAW.DefaultStyle".Translate();
            }
            return dic[thingDef].Category.label;
        }

        public static IEnumerable<ThingStyleDef> StylesFor(this ThingDef thingDef)
        {
            if (!thingDef.CanBeStyled())
            {
                yield break;
            }
            foreach (var cat in thingDef.RelevantStyleCategories)
            {
                if (cat.thingDefStyles.Any(x => x.ThingDef == thingDef))
                    yield return cat.thingDefStyles.Find(x => x.ThingDef == thingDef).StyleDef;
            }
        }

        public static ThingStyleDef GetStyle(CustomizationRequest request, ThingDef def)
        {
            Dictionary<string, string> dic = request.thingDefsAndStyles;
            if (!dic.ContainsKey(def.defName))
            {
                return null;
            }
            return GetStyle(dic[def.defName]);
        }

        public static ThingStyleDef GetStyle(string weaponStyleDefName)
        {
            if(DefDatabase<ThingStyleDef>.AllDefs.Any(x => x.defName == weaponStyleDefName))
                return DefDatabase<ThingStyleDef>.GetNamed(weaponStyleDefName, false);
            return null;
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

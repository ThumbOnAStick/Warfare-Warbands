using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using AlienRace;
using UnityEngine;
using System.Linq;

namespace WarfareAndWarbands.CharacterCustomization.Compatibility
{
    internal static class HAR
    {
        private static ThingDef_AlienRace GetAlienRace(this CustomizationRequest request)
        {
            if (!DefDatabase<ThingDef_AlienRace>.AllDefs.Any(x => x.defName == request.alienDefName))
            {
                return null; 
            }
            return DefDatabase<ThingDef_AlienRace>.AllDefs.First(x => x.defName == request.alienDefName);
        }

        public static void SetAlienRace(this PawnKindDef kindDef, CustomizationRequest request)
        {
            var alienRace = request.GetAlienRace();
            if (alienRace == null)
            {
                return;
            }
            kindDef.race = alienRace;
        }

        public static float GetAlienRaceValueOffset(this CustomizationRequest request)
        {
            var alienRace = request.GetAlienRace();
            if (alienRace == null)
            {
                return 1;
            }
            return alienRace.BaseMarketValue / ThingDefOf.Human.BaseMarketValue;
        }

        public static string GetAlienRaceString(this CustomizationRequest request)
        {
            var alienRace = request.GetAlienRace();
            if (alienRace == null)
            {
                return ThingDefOf.Human.label;
            }
            return alienRace.label;
        }

        public static void DrwaAlienButton(Rect buttonRect, CustomizationRequest request)
        {
            bool press = Widgets.ButtonText(buttonRect, "WAW.AlienRace".Translate(request.GetAlienRaceString()));
            if (press)
            {
                var options = AlienOptoins(request).ToList();
                FloatMenu menu = new FloatMenu(options);
                Find.WindowStack.Add(menu);
            }
        }

        public static IEnumerable<FloatMenuOption> AlienOptoins(CustomizationRequest request)
        {
            var allAliens = DefDatabase<ThingDef_AlienRace>.AllDefs;
            foreach (var alien in allAliens)
            {
                if(alien.race.ToolUser && alien.race.CanEverEat(ThingDefOf.MealSimple))
                yield return new FloatMenuOption(alien.label, delegate { SetAlienRace(request, alien); });
            }
        }

        public static void SetAlienRace(CustomizationRequest request, ThingDef_AlienRace alien)
        {
            request.SetAlienRace(alien.defName);
        }
    }
}

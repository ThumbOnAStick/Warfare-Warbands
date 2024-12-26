using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using UnityEngine;
using System.Linq;
using AlienRace;

namespace WarfareAndWarbands.CharacterCustomization.Compatibility
{
    internal static class HAR
    {
        public static ThingDef GetAlienRace(this CustomizationRequest request)
        {
            var allAliens = GenDefDatabase.GetAllDefsInDatabaseForDef(typeof(ThingDef_AlienRace));
            foreach (var thing in allAliens)
            {
                var alien = thing as ThingDef;
                if(alien.defName == request.alienDefName)
                return alien;
            }
            return null;
        }

        

        internal static void SetAlienRace(this PawnKindDef kindDef, CustomizationRequest request)
        {
            ThingDef alienRace = request.GetAlienRace();
            if (alienRace == null)
            {
                return;
            }
            kindDef.race = alienRace;
        }

        internal static float GetAlienRaceValueOffset(this CustomizationRequest request)
        {
            ThingDef alienRace = request.GetAlienRace();
            if (alienRace == null)
            {
                return 1;
            }
            return alienRace.BaseMarketValue / ThingDefOf.Human.BaseMarketValue;
        }

        internal static string GetAlienRaceString(this CustomizationRequest request)
        {
            ThingDef alienRace = request.GetAlienRace();
            if (alienRace == null)
            {
                return ThingDefOf.Human.label;
            }
            return alienRace.label;
        }

        internal static void DrwaAlienButton(Rect buttonRect, CustomizationRequest request)
        {
            bool press = Widgets.ButtonText(buttonRect, "WAW.AlienRace".Translate(request.GetAlienRaceString()));
            if (press)
            {
                var options = AlienOptoins(request).ToList();
                FloatMenu menu = new FloatMenu(options);
                Find.WindowStack.Add(menu);
            }
        }

        internal static IEnumerable<FloatMenuOption> AlienOptoins(CustomizationRequest request)
        {
            var allAliens = GenDefDatabase.GetAllDefsInDatabaseForDef(typeof(ThingDef_AlienRace));
            foreach (var thing in allAliens)
            {
                var alien = thing as ThingDef;
                if (alien.race.ToolUser && alien.race.CanEverEat(ThingDefOf.MealSimple))
                    yield return new FloatMenuOption(alien.label, delegate { SetAlienRace(request, alien); });
            }
        }

        internal static void SetAlienRace(CustomizationRequest request, ThingDef alien)
        {
            request.SetAlienRace(alien.defName);
            request.UpdatePawnKindDef();
        }

    }
}

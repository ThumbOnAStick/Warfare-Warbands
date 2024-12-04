using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarfareAndWarbands.CharacterCustomization.Compatibility
{
    internal static class CE
    {

        public static void GenerateAmmoFor(Pawn p)
        {
            if (!p.kindDef.modExtensions.Any(x => x.GetType() == typeof(LoadoutPropertiesExtension)))
                p.kindDef.modExtensions.Add(new LoadoutPropertiesExtension());
            var modExtension = p.kindDef.GetModExtension<LoadoutPropertiesExtension>();
            modExtension?.GenerateLoadoutFor(p, 1);
        }

    }
}

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
            var modExtension = p.kindDef.GetModExtension<LoadoutPropertiesExtension>();
            modExtension?.GenerateLoadoutFor(p, 1);
        }

    }
}

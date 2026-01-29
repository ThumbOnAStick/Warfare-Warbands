using Verse;
using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WarfareAndWarbands.CharacterCustomization.Compatibility
{
    internal static class CE
    {

        public static void GenerateAmmoFor(Pawn p)
        {
            if (p == null) return;
            if (p.equipment == null) return;
            if (p.equipment.Primary == null) return;
            if (p.equipment.Primary.TryGetComp<CompAmmoUser>() == null) return;
            Log.Message($"WAW & CE: Tried to generate ammo for {p.Name}");
            try
            {
                // Reset ammo count
                var ammoUser = p.equipment.Primary.GetComp<CompAmmoUser>();
                if (ammoUser.Props.ammoSet.ammoTypes.Count < 1) return;
                ammoUser.ResetAmmoCount(ammoUser.Props.ammoSet.ammoTypes.RandomElement().ammo);
                // Load inventory with ammo
                var inventory = p.inventory;
                if (inventory == null) return;
                AmmoDef ammoType = ammoUser.Props.ammoSet.ammoTypes.First().ammo;
                Thing ammo = ThingMaker.MakeThing(ammoType);
                int magazineSize = Math.Max(ammoUser.Props.magazineSize, 1);
                ammo.stackCount = magazineSize;
                inventory.TryAddAndUnforbid(ammo);
            }
            catch (Exception ex) {

                Log.Error($"WAW: Failed to generate ammo for pawn {p.Name}!" + ex.StackTrace);

            }



        }

    }
}

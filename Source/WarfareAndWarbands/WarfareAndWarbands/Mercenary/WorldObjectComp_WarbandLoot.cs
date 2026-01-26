using RimWorld.Planet;
using System.Collections.Generic;
using Verse;
using WarfareAndWarbands.Warband.Mercenary;

namespace WarfareAndWarbands.Mercenary
{
    public class WorldObjectComp_WarbandLoot: WorldObjectComp
    {
        public MapParent MapP => (MapParent)this.parent;

        private Command Command_OpenWarbandLootWindow()
        {
            return new Command_Action() {
                icon = TexButton.IconX,
                defaultLabel = "WAW.OpenLootWindow".Translate(),
                defaultDesc = "WAW.OpenLootWindow.Desc".Translate(),
                action = delegate
                {
                    Find.WindowStack.Add(new Dialog_WarbandLoot(this.MapP));
                }
            };

        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach(var gizmos in base.GetGizmos())
            {
                yield return gizmos;
            }

            if (MapP == null) yield break;

            if (!MapP.HasMap) yield break;

            if (MercenaryUtil.AnyMercenaryInMap(MapP))
            {
                yield return Command_OpenWarbandLootWindow();
            }
        }
    }
}

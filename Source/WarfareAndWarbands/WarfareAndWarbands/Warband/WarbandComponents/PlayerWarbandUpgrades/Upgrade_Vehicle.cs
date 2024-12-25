using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using WarfareAndWarbands.Warband.Compatibility_Vehicle;

namespace WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandUpgrades
{
    public class Upgrade_Vehicle : PlayerWarbandUpgrade
    {
        private Dictionary<string, int> _vehicles;
        private List<string> _stringBuffers;
        private List<int> _intBuffers;
        public Vector2 scrollPosition;

        public Upgrade_Vehicle()
        {
            _vehicles = new Dictionary<string, int>();
        }

        public Dictionary<string, int> Vehicles => _vehicles;

        public override bool CanDroppod => false;


        public void OnSquadSent()
        {
            _vehicles.Clear();
        }

        public void AddVehicle(string defName)
        {
            if (_vehicles.ContainsKey(defName))
            {
                _vehicles[defName]++;
            }
            else
            {
                _vehicles.Add(defName, 1);
            }
        }

        public override IEnumerable<Gizmo> GetGizmosExtra()
        {
            yield return new Command_Action()
            {
                Order = 3000,
                icon = WAWTex.PurchaseVehicleTex,
                defaultLabel = "purchase vehicles",
                action = delegate { Find.WindowStack.Add(new VehicleSelectionPanel(this)); }
            };
        }

        public override Texture2D TextureOverride()
        {
            return WAWTex.WarbandVehicleTex;
        }

        public override void OnArrived(List<Pawn> pawns)
        {
            base.OnArrived(pawns);
            OnSquadSent();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<string, int>(ref this._vehicles,
"vehicles", LookMode.Value, LookMode.Value, ref _stringBuffers, ref _intBuffers);
            Scribe_Values.Look(ref this.scrollPosition, "scrollPosition", Vector2.zero);
        }



    }
}

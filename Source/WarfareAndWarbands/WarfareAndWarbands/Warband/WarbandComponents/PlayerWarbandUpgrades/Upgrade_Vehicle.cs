﻿using RimWorld;
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
        public override string Label => "WAW.VehicleLabel".Translate();
        public override string Lore => "WAW.VehicleLore".Translate();
        public Dictionary<string, int> Vehicles => _vehicles;

        public override int UpgradeCost => 10000;

        public override bool CanDroppod => false;

        public override float MoveSpeed => 0.001f;

        public override string ModRequired => "SmashPhil.VehicleFramework";

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
                defaultLabel = "WAW.PurchaseVehicles".Translate(),
                action = delegate { Find.WindowStack.Add(new VehicleSelectionPanel(this)); }
            };
        }

        public override Texture2D TextureOverride()
        {
            return WAWTex.WarbandVehicleTex;
        }

        public override void OnPawnsGenerated(List<Pawn> pawns)
        {
            base.OnPawnsGenerated(pawns);
            OnSquadSent();
        }

        public override TaggedString GetInspectString()
        {
            var result = "WAW.CanPurcahseVehicle".Translate().Colorize(Color.cyan);
            result += "\n" + base.GetInspectString();
            return result;
        }

        public override void OnUpgraded()
        {
            base.OnUpgraded();
            Find.LetterStack.ReceiveLetter("WAW.AboutBrigadeCombatTeam".Translate(), "WAW.AboutBrigadeCombatTeam.Desc".Translate(), LetterDefOf.NeutralEvent);
        }

        public override bool CanAttackCurrent()
        {
            bool anyUsable = Vehicles.Any(x => x.Value > 0);
            if (anyUsable)
                return true;
            else
            {
                Messages.Message("WAW.NoVehicle".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }
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

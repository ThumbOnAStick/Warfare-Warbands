using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandComponents
{
    public class PlayerWarbandColorOverride:IExposable
    {
        public PlayerWarbandColorOverride()
        {
            colorOverride = Color.white;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref colorOverride, "colorOverride", Color.white);
        }

        public Color GetColorOverride()
        {
            return this.colorOverride;
        }

        public void SetColorOverride(Color color)
        {
            this.colorOverride = color; 
        }

        private Color colorOverride;
    }
}

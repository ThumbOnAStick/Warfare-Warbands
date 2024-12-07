using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using WarfareAndWarbands.Warband.UI;
using WarfareAndWarbands.Warband.WarbandRecruiting;

namespace WarfareAndWarbands.Warband
{
    public class WorldObject_WarbandRecruiting : WorldObject
    {
        public override string Label => "WAW.WarbandRecruiting".Translate();
        private Dictionary<string, int> bandMembers;
        private List<string> pawnkindCache;
        private List<int> memberCountCache;

        private float waitDays;


        public WorldObject_WarbandRecruiting()
        {
            this.bandMembers = new Dictionary<string, int>();
        }

        public override void PostAdd()
        {
            base.PostAdd();
            this.bandMembers = new Dictionary<string, int>(GameComponent_WAW.playerWarband.bandMembers);
            waitDays = GetWaitDays();

        }

        public override string GetInspectString()
        {
            var outString = base.GetInspectString();
            outString += "\n" + "WAW.ActiveMembers".Translate();
            foreach (var member in this.bandMembers)
            {
                if (member.Value > 0)
                    outString += "\n" + WarbandUtil.GetSoldierLabel(member.Key) + "(" + member.Value + ")";
            }
            outString += "\n" + "WAW.RemainingDays".Translate(GetRemainingDays().ToString("0.0"));
            return outString;

        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<string, int>(ref this.bandMembers,
  "bandMembers", LookMode.Value, LookMode.Value, ref pawnkindCache, ref memberCountCache);
        }

        public override void Tick()
        {
            base.Tick();
            if (this.creationGameTicks + GetWaitTicks() < GenTicks.TicksGame)
            {
                CreateWarband();
                this.Destroy();

            }
        }

        public void CreateWarband()
        {
            var warband = WarbandUtil.SpawnWarband(Faction.OfPlayer, this.Tile);
            SendCreateWarbandMessage(warband);
        }

        void SendCreateWarbandMessage(Warband warband)
        {
            string label = "WAW.WarbandCreated".Translate();
            string desc = "WAW.WarbandCreated.Desc".Translate();
            Letter letter = LetterMaker.MakeLetter(label, desc, LetterDefOf.PositiveEvent);
            if (warband != null)
                letter.lookTargets = warband;
            Find.LetterStack.ReceiveLetter(letter);
        }

        public override Color ExpandingIconColor => Faction.OfPlayer.Color;

        public SimpleCurve WaitDaysCurve()
        {
            SimpleCurve curve = new SimpleCurve
            {
                { 1, .5f },
                { 10, 5f },
                { 50, 10f }
            };

            return curve;
        }



        public int GetMemberCount()
        {
            int result = 0;
            foreach (var ele in this.bandMembers)
            {
                result += ele.Value;
            }
            return result;
        }

        public float GetWaitDays()
        {
            int memberCount = GetMemberCount();
            return WaitDaysCurve().Evaluate(memberCount); 
        }

        public int GetWaitTicks()
        {
            return (int)this.waitDays * GenDate.TicksPerDay;
        }

        public int GetRemainingTicks()
        {
            return this.creationGameTicks + GetWaitTicks() - GenTicks.TicksGame;
        }

        public float GetRemainingDays()
        {
            return GenDate.TicksToDays(this.GetRemainingTicks());
        }
    }
}

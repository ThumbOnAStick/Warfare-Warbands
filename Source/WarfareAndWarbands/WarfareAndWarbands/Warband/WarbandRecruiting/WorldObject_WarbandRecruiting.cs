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
        public Dictionary<string, int> BandMembers => bandMembers;
        private Dictionary<string, int> bandMembers;
        private List<string> pawnkindCache;
        private List<int> memberCountCache;
        private Color color;
        private float waitDays;
        private Pawn leader;


        public WorldObject_WarbandRecruiting()
        {
            this.bandMembers = new Dictionary<string, int>();
        }

        public override void PostAdd()
        {
            base.PostAdd();
            this.bandMembers = new Dictionary<string, int>(GameComponent_WAW.playerWarband.bandMembers);
            this.color = GameComponent_WAW.playerWarband.colorOverride;
            waitDays = GetWaitDays();
            WarbandUtil.RefreshAllPlayerWarbands();
        }

        public override void PostRemove()
        {
            base.PostRemove();
            WarbandUtil.RefreshAllPlayerWarbands();
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

            if (this.leader != null)
            {
                string leaderString = "WAW.Leader".Translate(this.leader);
                outString += "\n" + leaderString;
            }
            return outString;

        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<string, int>(ref this.bandMembers,
  "bandMembers", LookMode.Value, LookMode.Value, ref pawnkindCache, ref memberCountCache);
            Scribe_Values.Look(ref this.color, "color", Color.white);
            Scribe_Values.Look(ref this.waitDays, "waitDays", this.GetWaitDays());
            Scribe_Deep.Look(ref this.leader, "leader");

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
            var warband = WarbandUtil.SpawnWarband(Faction.OfPlayer, this.Tile, color, leader);
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
            return (int)(this.waitDays * GenDate.TicksPerDay * WAWSettings.warbandRecruitTimeMultiplier);
        }

        public int GetRemainingTicks()
        {
            return this.creationGameTicks + GetWaitTicks() - GenTicks.TicksGame;
        }

        public float GetRemainingDays()
        {
            return GenDate.TicksToDays(this.GetRemainingTicks());
        }

        public void SetColorOverride()
        {
            this.color = GameComponent_WAW.playerWarband.colorOverride;
        }

        public void AssignLeader(Pawn p, Caravan caravan)
        {
            if (leader != null && !leader.Dead)
            {
                return;
            }
            if (p == null)
            {
                return;
            }
            this.leader = p;
            caravan?.RemovePawn(p);
            ResolvePawn(p);
            SendLeaderSetMessage(p);
            ResolveCaravan(caravan);
        }

        void ResolvePawn(Pawn p)
        {
            p.holdingOwner?.Remove(p);
            if (!Find.WorldPawns.Contains(p))
                Find.WorldPawns.PassToWorld(p);
        }

        void ResolveCaravan(Caravan caravan)
        {
            for (int i = 0; i < caravan.pawns.Count; i++)
            {
                if (caravan.pawns[i].IsColonist)
                {
                    return;
                }
            }
            caravan.Destroy();
        }
        void SendLeaderSetMessage(Pawn pawn)
        {
            string label = "WAW.LeaderSet".Translate();
            string desc = "WAW.LeaderSet.Desc".Translate(pawn.NameFullColored, pawn.gender.GetObjective(), pawn.gender.GetPronoun());
            Letter letter = LetterMaker.MakeLetter(label, desc, LetterDefOf.NeutralEvent);
            Find.LetterStack.ReceiveLetter(letter);
        }

    }
}

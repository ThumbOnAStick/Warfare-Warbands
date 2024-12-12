using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using WarfareAndWarbands;
using WarfareAndWarbands.Warband;
using WarfareAndWarbands.Warband.Mercenary;
using WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandComponents.Leader;
using WAWLeadership.UI;

namespace WAWLeadership
{
    public class CompLeadership : ThingComp
    {

        private bool isWarbandLeader;
        private Leadership leadership;
        public Leadership Leadership { get { return leadership; } }

        public CompLeadership()
        {
            GameComponent_WAW.Instance.onRaid.AddListener(AddExpForRaiding);
            GameComponent_WAW.Instance.onRaided.AddListener(AddExpForRaiding);  
        }

        public void AddExpForRaiding()
        {
            if (Pawn == GameComponent_WAW.Instance.GetRaidLeaderCache())
            {
                Log.Message($"EXP added to leader {Pawn.Name} for raiding! ({WAWSettings.playerRaidExp} points)");
                AddExpFor(WAWSettings.playerRaidExp);
            }
        }

        public void AddExpForDefending()
        {
            if (Pawn == GameComponent_WAW.Instance.GetRaidLeaderCache())
            {
                Log.Message($"EXP added to leader {Pawn.Name} for defending! ({WAWSettings.playerRaidExp} points)");
                AddExpFor(WAWSettings.playerRaiddedExp);
            }
        }

        public void AddExpFor(int amount)
        {
            leadership.AddExp(amount, out bool levelUp);
            TryNotifyPlayerOfLeaderLevelUp(levelUp);
        }

        public void TryNotifyPlayerOfLeaderLevelUp(bool levelUp)
        {
            if (levelUp)
            {
                Message m = new Message("WAW.LeaderLevelUp".Translate(), MessageTypeDefOf.PositiveEvent);
                Messages.Message(m);
            }
        }

        public bool GetIsWarbandLeader()
        {
            return isWarbandLeader;
        }

        public void SetIsWarbandLeader(bool isWarbandLeader)
        {
            this.isWarbandLeader = isWarbandLeader;
        }


        public override void CompTickRare()
        {
            base.CompTickRare();
            if (IsColonist())
            {
                SetIsWarbandLeader(PlayerWarbandLeaderUtil.IsLeader(this.parent as Pawn, out Warband warband));
            }

            if (leadership.AttributeSet.AllAttributesEmpty())
            {
                leadership.AssignRandomAttribute(this.parent as Pawn);
            }
        }

        public Pawn Pawn => parent as Pawn;

        public bool IsColonist()
        {
            return parent is Pawn pawn && pawn.IsColonist;
        }


        public override void CompTick()
        {
            base.CompTick();
            if (leadership == null)
            {
                leadership = new Leadership();
            }
            if (!parent.def.inspectorTabsResolved.Any(x => x as ITab_Leadership != null))
            {
                ResolveLeaderTab();
            }
            leadership?.Tick();
        }

        public void InitAttributes()
        {
            if (leadership == null)
            {
                leadership = new Leadership();
            }
            leadership.AttributeSet.InitAttributes();
            leadership.AssignRandomAttribute(this.parent as Pawn);
        }

        public void ResolveLeaderTab()
        {
            var tabs = parent.GetInspectTabs();

            if (tabs != null && !tabs.Any())
            {
                return;
            }
            if (tabs.FirstOrDefault(x => x is ITab_Leadership) != null)
            {
                return;
            }
            try
            {

                parent.def.inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(typeof(ITab_Leadership)));
            }
            catch (Exception ex)
            {
                Log.Error(string.Concat(new object[]
                {
                            "Could not instantiate inspector tab of type ",
                           typeof(ITab_Leadership) ,
                            ": ",
                            ex
                }));
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this.isWarbandLeader, "isWarbandLeader", false);
            Scribe_Deep.Look(ref this.leadership, "leadership");
        }
    }
}

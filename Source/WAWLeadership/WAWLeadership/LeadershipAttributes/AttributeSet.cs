using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandComponents;

namespace WAWLeadership.LeadershipAttributes
{
    public class AttributeSet : IExposable
    {
        public int AttributePoints => attributePoints;
        private int attributePoints;
        private static readonly int addAttributeChance = 3;
        public HashSet<LeadershipAttribute> Attributes => attributes;
        private HashSet<LeadershipAttribute> attributes;

        public AttributeSet()
        {
            attributePoints = 0;
            attributes = new HashSet<LeadershipAttribute>();
        }

        public void Tick()
        {
            if (attributes.Count < 6)
            {
                InitAttributes();
            }
        }

        public void GainPoint()
        {
            attributePoints++;
        }

        public SimpleCurve PointsCurve()
        {
            return new SimpleCurve
            {
                { 1, 1 },
                { 3, 1 },
                { 4, 2 },
                { 8, 2 },
                { 9, 3 }
            };
        }

        public void GainPoints(int levelUpAmount, int oldLevel)
        {
            for (int i = 0; i < levelUpAmount; i++)
            {
                int point = (int)PointsCurve().Evaluate(oldLevel + i);
                attributePoints += point;
            }
        }

        public bool AllAttributesEmpty()
        {
            return !this.attributes.Any(x => x.GetLevel() > 0);
        }

        public void AssignRandomAttribute(Pawn pawn)
        {
            foreach (var attribute in attributes)
            {
                int rnd = new IntRange(1, 10).RandomInRange;
                if (rnd < addAttributeChance)
                {
                    attribute?.TryToIncrementLevel();
                }
                if (CanBenifitFromSkill(attribute, pawn))
                {
                    attribute.TryToIncrementLevel();
                }
            }
        }

        bool CanBenifitFromSkill(LeadershipAttribute attribute, Pawn pawn)
        {
            return attribute != null && attribute.BonusFromSkill() != null && pawn.skills != null && pawn.skills.GetSkill(attribute.BonusFromSkill()).Level >= 10;
        }

        public void InitAttributes()
        {
            attributes = new HashSet<LeadershipAttribute>
            {
                new Attribute_Commanding(),
                new Attribute_Medic(),
                new Attribute_Recruiting(),
                new Attribute_Engineering(),
                new Attribute_Economy(),
                new Attribute_Diplomacy()
            };
        }

        public LeadershipAttribute GetAttribute<T>() where T : LeadershipAttribute
        {
            for (int i = 0; i < this.attributes.Count; i++)
            {
                var attribute = this.attributes.ElementAt(i);
                if (attribute != null && attribute as T != null)
                {
                    return attribute;
                }

            }
            return null;
        }

        public void DistributePoint<T>(out bool distributed) where T : LeadershipAttribute
        {
            distributed = false;    
            if (this.attributePoints < 1)
            {
                return;
            }
            var attribute = GetAttribute<T>();
            if(attribute == null)
            {
                return;
            }
            attribute.TryToIncrementLevel(out distributed);
            if (distributed)
            {
                attributePoints--;
            }
        }


        public bool TryToDistributePoint(ref LeadershipAttribute attribute)
        {
            if (!attributes.Contains(attribute))
            {
                return false;
            }
            DistributePoint(ref attribute, out bool succeed);
            return succeed;
        }

        void DistributePoint(ref LeadershipAttribute attribute, out bool succeed)
        {
            succeed = false;
            if (attribute != null)
            {
                attribute.TryToIncrementLevel(out succeed);
                if (succeed)
                {
                    attributePoints--;
                }
            }
        }

        public void ApplySkillBonuses(PlayerWarbandSkillBonus skillBonus)
        {
            foreach (var attribute in attributes)
            {
                skillBonus.TryToAddNewBonus(attribute.BoostsSkill(), attribute.SkillBonus());
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref attributePoints, "attributePoints", 0);
            Scribe_Collections.Look(ref attributes, "attributes", LookMode.Deep);

        }
    }
}

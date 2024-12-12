using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

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
            // attribute set check
            if (attributes.Count < 6)
            {
                InitAttributes();
            }
        }

        public void GainPoint()
        {
            attributePoints++;
        }

        public void GainPoints(int levelUpAmount)
        {
            attributePoints += levelUpAmount;
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
                new Attribute_Raiding(),
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

        public void DistributePoint<T>() where T : LeadershipAttribute
        {
            if (this.attributePoints > 0)
            {
                attributePoints--;
            }
            else
            {
                return;
            }

            GetAttribute<T>()?.TryToIncrementLevel();
        }
        public void DistributePoint(LeadershipAttribute attribute)
        {
            if (!attributes.Contains(attribute))
            {
                return;
            }
            attributePoints--;
            attribute?.TryToIncrementLevel();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref attributePoints, "attributePoints", 0);
            Scribe_Collections.Look(ref attributes, "attributes", LookMode.Deep);

        }
    }
}

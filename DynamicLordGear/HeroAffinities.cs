using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace DynamicLordGear
{
    enum HeroAffinity
    {
        OneHanded,
        TwoHanded,
        Polearm,
        Throwing,
        Bow,
        Crossbow,
        Foot,
        Mounted,
    }

    internal class HeroAffinities
    {
        public Dictionary<HeroAffinity, float> Values = new Dictionary<HeroAffinity, float>();

        public HeroAffinities(float initValue)
        {
            foreach (HeroAffinity affinity in (HeroAffinity[])Enum.GetValues(typeof(HeroAffinity)))
            {
                Values[affinity] = initValue;
            }
        }

        public void SetFromHero(Hero hero, float weight)
        {
            foreach (HeroAffinity affinity in (HeroAffinity[])Enum.GetValues(typeof(HeroAffinity)))
            {
                Values[affinity] = ((1.0f - weight) * Values[affinity]) + (weight * GetHeroSkillAffinity(hero, GetSkillForAffinity(affinity)));
            }
        }

        SkillObject GetSkillForAffinity(HeroAffinity affinity)
        {
            switch (affinity)
            {
                case HeroAffinity.OneHanded:
                    return DefaultSkills.OneHanded;
                case HeroAffinity.TwoHanded:
                    return DefaultSkills.TwoHanded;
                case HeroAffinity.Polearm:
                    return DefaultSkills.Polearm;
                case HeroAffinity.Throwing:
                    return DefaultSkills.Throwing;
                case HeroAffinity.Bow:
                    return DefaultSkills.Bow;
                case HeroAffinity.Crossbow:
                    return DefaultSkills.Crossbow;
                case HeroAffinity.Foot:
                    return DefaultSkills.Athletics;
                case HeroAffinity.Mounted:
                    return DefaultSkills.Riding;
                default:
                    return DefaultSkills.OneHanded;
            }

        }

        float GetHeroSkillAffinity(Hero hero, SkillObject skill)
        {
            float skillAffinity = ((float)hero.GetSkillValue(skill)) / 300.0f;
            float focusAffinity = ((float)hero.HeroDeveloper.GetFocus(skill)) / 5.0f;
            float attributeAffinity = 0.0f;

            if (skill.Attributes.Length > 0)
            {
                attributeAffinity = ((float)hero.GetAttributeValue(skill.Attributes[0])) / 10.0f;
            }

            float totalWeight = DynamicLordGearSettings.Instance.HeroAffinity_FocusPointWeight
                + DynamicLordGearSettings.Instance.HeroAffinity_SkillpointWeight
                + DynamicLordGearSettings.Instance.HeroAffinity_AttributeWeight;

            return ((DynamicLordGearSettings.Instance.HeroAffinity_FocusPointWeight / totalWeight) * focusAffinity) +
                ((DynamicLordGearSettings.Instance.HeroAffinity_SkillpointWeight / totalWeight) * skillAffinity) +
                ((DynamicLordGearSettings.Instance.HeroAffinity_AttributeWeight / totalWeight) * attributeAffinity);
        }
    }
}

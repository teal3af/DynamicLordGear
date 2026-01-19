using MCM.Abstractions;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using MCM.Abstractions.Base.PerCampaign;
using MCM.Abstractions.Base.PerSave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;

namespace DynamicLordGear
{
    internal sealed class DynamicLordGearSettings : AttributePerCampaignSettings<DynamicLordGearSettings>
    {
        public override string Id => "TeaLeaf.DynamicLordGear.V1";
        public override string DisplayName => $"Dynamic Lord Gear";
        public override string FolderName => "TeaLeaf.DLG";

        [SettingPropertyBool("Dynamic Gear Selection", Order = 0, RequireRestart = false, HintText = "When this is checked, gear is selected dynamically based on the options below. Otherwise gear is always picked from an existing lord template for their culture.")]
        [SettingPropertyGroup("General", GroupOrder = 0)]
        public bool DynamicGearSelection { get; set;} = false;

        [SettingPropertyBool("Use Standard Lord Armor", Order = 1, RequireRestart = false, HintText = "When this is checked, lord armor still uses the standard sets whilst weapons remain dynamic.")]
        [SettingPropertyGroup("General", GroupOrder = 0)]
        public bool UseStandardLordArmor { get; set; } = false;

        [SettingPropertyBool("Update Gear for Well Equipped Lords", Order = 2, RequireRestart = false, HintText = "By default, the mod only gives gear to lords who *don't* have proper armor or weapons yet. Turn this on to have all lords periodically pick new gear. NOTE: Once they have some gear, the undergeared lords will not be recognized as undergeared again by this mod.")]
        [SettingPropertyGroup("General", GroupOrder = 0)]
        public bool UpdateGearForWellEquippedLords { get; set; } = false;

        [SettingPropertyBool("Apply to Wanderers", Order = 2, RequireRestart = false, HintText = "Give wanderers gear. NOTE: This will make them more expensive to hire, as price is calculated based on their gear. You can sell their stuff though.")]
        [SettingPropertyGroup("Wanderers", GroupOrder = 1)]
        public bool ApplyToWanderers { get; set; } = false;

        [SettingPropertyBool("Match Wanderer Gear Tier to Player's", Order = 3, RequireRestart = false, HintText = "Scale wanderer gear with the player's clan tier, making them start out better equipped (and more expensive) as play progresses.")]
        [SettingPropertyGroup("Wanderers", GroupOrder = 1)]
        public bool MatchWandererGearTierToPlayer { get; set; } = false;

        [SettingPropertyInteger("Wanderer Clan Tier", minValue: 1, maxValue: 6, "0", Order = 4, RequireRestart = false, HintText = "When picking equipment, treat wanderers as if this is their clan tier. IGNORED if above option is checked.")]
        [SettingPropertyGroup("Wanderers", GroupOrder = 1)]
        public int WandererClanTier { get; set; } = 2;

        [SettingPropertyBool("Give Wanderers Cheap Worn Out Gear", Order = 5, RequireRestart = false, HintText = "Matches vanilla behaviour. Apply special worn/rusty/old modifiers to all wanderer gear. These modifiers are different to the normal variants because they lower sell price far more than the combat stats. This will make wanderers a lot cheaper to hire (and preventing exploits!)")]
        [SettingPropertyGroup("Wanderers", GroupOrder = 1)]
        public bool GiveWanderersWornOutGear { get; set; } = true;

        [SettingPropertyInteger("Minimum Gear Tier", minValue: 1, maxValue: 6, "0", Order = 6, RequireRestart = false, HintText = "Don't give lords gear of a lower tier than this, if possible.")]
        [SettingPropertyGroup("Gear Tier Calculation", GroupOrder = 1)]
        public int GearTier_Minimum { get; set; } = 6;

        [SettingPropertyFloatingInteger("Combat Skill Weight", 0f, 1f, "0.0", Order = 7, RequireRestart = false, HintText = "When calculating the gear tier for a lord, how much to factor in their combat ability.")]
        [SettingPropertyGroup("Gear Tier Calculation", GroupOrder = 1)]
        public float GearTier_HeroSkillWeight { get; set; } = 0.5f;

        [SettingPropertyFloatingInteger("Clan Tier Weight", 0f, 1f, "0.0", Order = 8, RequireRestart = false, HintText = "When calculating the gear tier for a lord, how much to factor in their clan's tier.")]
        [SettingPropertyGroup("Gear Tier Calculation", GroupOrder = 1)]
        public float GearTier_ClanTierWeight { get; set; } = 1.0f;

        [SettingPropertyFloatingInteger("Skill Weight", 0f, 1f, "0.0", Order = 9, RequireRestart = false, HintText = "When calculating a hero's affinity for gear, how much weight to give to their skills.")]
        [SettingPropertyGroup("Hero Affinity Calculation", GroupOrder = 2)]
        public float HeroAffinity_SkillWeight { get; set; } = 1.0f;

        [SettingPropertyFloatingInteger("Skill Point weight", 0f, 1f, "0.0", Order = 10, RequireRestart = false, HintText = "When calculating skill based affinity, how much weight to give to raw skill points.")]
        [SettingPropertyGroup("Hero Affinity Calculation/Skill Components", GroupOrder = 3)]
        public float HeroAffinity_SkillpointWeight { get; set; } = 1.0f;

        [SettingPropertyFloatingInteger("Focus Point weight", 0f, 1f, "0.0", Order = 11, RequireRestart = false, HintText = "When calculating skill based affinity, how much weight to give to focus points.")]
        [SettingPropertyGroup("Hero Affinity Calculation/Skill Components", GroupOrder = 3)]
        public float HeroAffinity_FocusPointWeight { get; set; } = 0.6f;

        [SettingPropertyFloatingInteger("Attribute Weight", 0f, 1f, "0.0", Order = 12, RequireRestart = false, HintText = "When calculating skill based affinity, how much weight to give to base attributes.")]
        [SettingPropertyGroup("Hero Affinity Calculation/Skill Components", GroupOrder = 3)]
        public float HeroAffinity_AttributeWeight { get; set; } = 0.3f;

        [SettingPropertyFloatingInteger("Culture Weight", 0f, 1f, "0.0", Order = 13, RequireRestart = false, HintText = "When calculating a hero's affinity for gear, how much weight to give to their culture.")]
        [SettingPropertyGroup("Hero Affinity Calculation", GroupOrder = 4)]
        public float HeroAffinity_CultureWeight { get; set; } = 1.0f;

        [SettingPropertyFloatingInteger("Personal Culture Weight", 0f, 1f, "0.0", Order = 14, RequireRestart = false, HintText = "When calculating affinity from the hero's culture, how much weight to give to their personal culture.")]
        [SettingPropertyGroup("Hero Affinity Calculation/Culture Components", GroupOrder = 5)]
        public float HeroAffinity_PersonalCultureWeight { get; set; } = 1.0f;

        [SettingPropertyFloatingInteger("Clan Culture Weight", 0f, 1f, "0.0", Order = 15, RequireRestart = false, HintText = "When calculating affinity from the hero's culture, how much weight to give to their clan's culture.")]
        [SettingPropertyGroup("Hero Affinity Calculation/Culture Components", GroupOrder = 5)]
        public float HeroAffinity_ClanCultureWeight { get; set; } = 0.5f;

        [SettingPropertyFloatingInteger("Father Culture Weight", 0f, 1f, "0.0", Order = 16, RequireRestart = false, HintText = "When calculating affinity from the hero's culture, how much weight to give to their father's culture.")]
        [SettingPropertyGroup("Hero Affinity Calculation/Culture Components", GroupOrder = 5)]
        public float HeroAffinity_FatherCultureWeight { get; set; } = 0.75f;

        [SettingPropertyFloatingInteger("Mother Culture Weight", 0f, 1f, "0.0", Order = 17, RequireRestart = false, HintText = "When calculating affinity from the hero's culture, how much weight to give to their mother's culture")]
        [SettingPropertyGroup("Hero Affinity Calculation/Culture Components", GroupOrder = 5)]
        public float HeroAffinity_MotherCultureWeight { get; set; } = 0.75f;

        [SettingPropertyFloatingInteger("Gear Catalog Weight", 0f, 1f, "0.0", Order = 18, RequireRestart = false, HintText = "When calculating a culture's affinity for gear, how much weight to give to the gear tagged as theirs.")]
        [SettingPropertyGroup("Culture Affinity Calculation", GroupOrder = 6)]
        public float CultureAffinity_GearCatalogWeight { get; set; } = 1.0f;
        [SettingPropertyFloatingInteger("Original Lord Loadout Weight", 0f, 1f, "0.0", Order = 19, RequireRestart = false, HintText = "When calculating a culture's affinity for gear, how much weight to give to the loadouts assigned to the original lords in the data.")]
        [SettingPropertyGroup("Culture Affinity Calculation", GroupOrder = 6)]
        public float CultureAffinity_LordLoadoutWeight { get; set; } = 1.0f;

        [SettingPropertyFloatingInteger("Culture Strictness", 0f, 5f, "0.0", Order = 20, RequireRestart = false, HintText = "How rigidily will lords stick to their culture(s) when picking gear. 0 = no cultural preference at all. 2 = strong bias to own culture gear 5 = will only pick gear outside culture when absolutely necessary.")]
        [SettingPropertyGroup("Gear Selection", GroupOrder = 7)]
        public float GearChoice_CultureStrictness { get; set; } = 2.0f;

        [SettingPropertyBool("Apply Item Modifiers", Order = 21, RequireRestart = false, HintText = "Sometimes the system will pick items that don't perfectly match the target tier. This setting will apply modifiers such as masterwork, balanced, rusty, etc to upgrade or downgrade them to fit better.")]
        [SettingPropertyGroup("Gear Selection", GroupOrder = 7)]
        public bool GearChoice_ApplyItemModifiers { get; set; } = false;

        [SettingPropertyInteger("Random Seed", 0, int.MaxValue, "0", Order = 22, RequireRestart = false, HintText = "There is some randomness to gear selection. You can shift this to get different results.")]
        [SettingPropertyGroup("Gear Selection", GroupOrder = 7)]
        public int RandomSeed {get; set;} = 0;

        [SettingPropertyButton("Reselect all hero gear now", Order = 22, Content = "GO!", RequireRestart = false, HintText = "Click to select gear for all NPC heroes right now.")]
        [SettingPropertyGroup("Commands", GroupOrder = 8)]
        public Action ReselectAllGearAction { get; set; } = (() => { Campaign.Current?.GetCampaignBehavior<DynamicLordGearBehavior>()?.SelectGearForAllNPCHeroes(onSessionStart:false); });

        //Hidden Variables for now - useful to be keep tweaking easy, but might be information overload for players
        //=======================================================================================================================

        //Shield loadout slots use the one-handed affinity multiplied by this much.
        public float Loadout_ShieldAffinityMul = 1.15f;

        //Ammo loadout slots use the ranged affinity multiplied by this much.
        public float Loadout_AmmoAffinityMul = 0.85f;

        //Empty loadout slots use average affinity of non-empty slots, multiplied by this much.
        //EG A character with strong 2H and Throwing skill will have strong affinity with a 2h+Throwing+Throwing+Throwing loadout
        //  but a character with 2H and weak Throwing skill might have stronger affinity with a 2h+Empty+Empty+Empty loadout
        public float Loadout_EmptySlotMul = 0.70f;

        public bool DEBUG_ApplyToPlayer = false;
        public bool DEBUG_ApplyToPlayerClan = false;

        public override IEnumerable<ISettingsPreset> GetBuiltInPresets()
        {
            yield return new MemorySettingsPreset(Id, "fix", "Fix Ungeared Lords", () => new DynamicLordGearSettings());

            yield return new MemorySettingsPreset(Id, "balanced", "Balanced", () => new DynamicLordGearSettings()
            {
                UpdateGearForWellEquippedLords = true,
                DynamicGearSelection = true,
                GearTier_Minimum = 1,
            });

            yield return new MemorySettingsPreset(Id, "skill", "Skill Focus", () => new DynamicLordGearSettings
            {
                UpdateGearForWellEquippedLords = true,
                DynamicGearSelection = true,
                HeroAffinity_SkillWeight = 1.0f,
                HeroAffinity_CultureWeight = 0.5f,
                GearTier_Minimum = 1,
            });

            yield return new MemorySettingsPreset(Id, "culture", "Culture Focus", () => new DynamicLordGearSettings
            {
                UpdateGearForWellEquippedLords = true,
                DynamicGearSelection = true,
                HeroAffinity_SkillWeight = 0.5f,
                HeroAffinity_CultureWeight = 1.0f,
                GearTier_Minimum = 1,
            });

            yield return new MemorySettingsPreset(Id, "optimized", "Optimized Misfits", () => new DynamicLordGearSettings
            {
                UpdateGearForWellEquippedLords = true,
                DynamicGearSelection = true,
                HeroAffinity_SkillWeight = 1.0f,
                HeroAffinity_CultureWeight = 0.0f,
                GearChoice_CultureStrictness = 0.0f,
                GearTier_Minimum = 1,
            });

            
        }
    }
}

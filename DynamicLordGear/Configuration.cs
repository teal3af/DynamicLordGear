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
        public override string DisplayName => new TaleWorlds.Localization.TextObject("{=MOOWFpsg}Dynamic Lord Gear").ToString();
        public override string FolderName => "TeaLeaf.DLG";

        [SettingPropertyBool("{=WZH5IWes}Dynamic Gear Selection", Order = 1, RequireRestart = false,
            HintText = "{=QJGWqibY}When this is checked, gear is selected dynamically based on the options below. Otherwise gear is always picked from an existing lord template for their culture.")]
        [SettingPropertyGroup("{=g13Bnmlu}General", GroupOrder = 0)]
        public bool DynamicGearSelection { get; set;} = false;

        [SettingPropertyBool("{=9AoflS63}Use Standard Lord Armor", Order = 2, RequireRestart = false,
            HintText = "{=QIbG4sfV}When this is checked, lord armor still uses the standard sets whilst weapons remain dynamic.")]
        [SettingPropertyGroup("{=g13Bnmlu}General", GroupOrder = 0)]
        public bool UseStandardLordArmor { get; set; } = false;

        [SettingPropertyBool("{=vZSP7FRp}Update Gear for Well Equipped Lords", Order = 3, RequireRestart = false,
            HintText = "{=vAwBaFSH}By default, the mod only gives gear to lords who *don't* have proper armor or weapons yet. Turn this on to have all lords periodically pick new gear. NOTE: Once they have some gear, the undergeared lords will not be recognized as undergeared again by this mod.")]
        [SettingPropertyGroup("{=g13Bnmlu}General", GroupOrder = 0)]
        public bool UpdateGearForWellEquippedLords { get; set; } = false;

        [SettingPropertyBool("{=myCH5pXc}Apply to Wanderers", Order = 4, RequireRestart = false,
            HintText = "{=hahdDbRK}Give wanderers gear. NOTE: This will make them more expensive to hire, as price is calculated based on their gear. You can sell their stuff though.")]
        [SettingPropertyGroup("{=AdF5XMGG}Wanderers", GroupOrder = 1)]
        public bool ApplyToWanderers { get; set; } = false;

        [SettingPropertyBool("{=Gu8ytEWr}Match Wanderer Gear Tier to Player's", Order = 5, RequireRestart = false,
            HintText = "{=UcTkM7mD}Scale wanderer gear with the player's clan tier, making them start out better equipped (and more expensive) as play progresses.")]
        [SettingPropertyGroup("{=AdF5XMGG}Wanderers", GroupOrder = 1)]
        public bool MatchWandererGearTierToPlayer { get; set; } = false;

        [SettingPropertyInteger("{=ormjhhKU}Wanderer Clan Tier", minValue: 1, maxValue: 6, "0", Order = 6, RequireRestart = false,
            HintText = "{=Fev7BJ6m}When picking equipment, treat wanderers as if this is their clan tier. IGNORED if above option is checked.")]
        [SettingPropertyGroup("{=AdF5XMGG}Wanderers", GroupOrder = 1)]
        public int WandererClanTier { get; set; } = 2;

        [SettingPropertyBool("{=avsnc7TC}Give Wanderers Cheap Worn Out Gear", Order = 7, RequireRestart = false,
            HintText = "{=DBUaRJcm}Matches vanilla behaviour. Apply special worn/rusty/old modifiers to all wanderer gear. These modifiers are different to the normal variants because they lower sell price far more than the combat stats. This will make wanderers a lot cheaper to hire (and preventing exploits!)")]
        [SettingPropertyGroup("{=AdF5XMGG}Wanderers", GroupOrder = 1)]
        public bool GiveWanderersWornOutGear { get; set; } = true;

        [SettingPropertyInteger("{=gnl105ml}Minimum Gear Tier", minValue: 1, maxValue: 6, "0", Order = 8, RequireRestart = false,
            HintText = "{=CpO4gJt3}Don't give lords gear of a lower tier than this, if possible.")]
        [SettingPropertyGroup("{=BeEeKFQc}Gear Tier Calculation", GroupOrder = 2)]
        public int GearTier_Minimum { get; set; } = 6;

        [SettingPropertyFloatingInteger("{=vSEp4s63}Combat Skill Weight", 0f, 1f, "0.0", Order = 9, RequireRestart = false,
            HintText = "{=q7L7LenY}When calculating the gear tier for a lord, how much to factor in their combat ability.")]
        [SettingPropertyGroup("{=BeEeKFQc}Gear Tier Calculation", GroupOrder = 2)]
        public float GearTier_HeroSkillWeight { get; set; } = 0.5f;

        [SettingPropertyFloatingInteger("{=a7UDaCDR}Clan Tier Weight", 0f, 1f, "0.0", Order = 10, RequireRestart = false,
            HintText = "{=wXzyt98b}When calculating the gear tier for a lord, how much to factor in their clan's tier.")]
        [SettingPropertyGroup("{=BeEeKFQc}Gear Tier Calculation", GroupOrder = 2)]
        public float GearTier_ClanTierWeight { get; set; } = 1.0f;

        [SettingPropertyFloatingInteger("{=LLkekt8e}Skill Weight", 0f, 1f, "0.0", Order = 11, RequireRestart = false,
            HintText = "{=GuvfRzTN}When calculating a hero's affinity for gear, how much weight to give to their skills.")]
        [SettingPropertyGroup("{=1w91iyzc}Hero Affinity Calculation", GroupOrder = 3)]
        public float HeroAffinity_SkillWeight { get; set; } = 1.0f;

        [SettingPropertyFloatingInteger("{=DOLKediw}Culture Weight", 0f, 1f, "0.0", Order = 12, RequireRestart = false,
            HintText = "{=Puu9DY2V}When calculating a hero's affinity for gear, how much weight to give to their culture.")]
        [SettingPropertyGroup("{=1w91iyzc}Hero Affinity Calculation", GroupOrder = 3)]
        public float HeroAffinity_CultureWeight { get; set; } = 1.0f;

        [SettingPropertyFloatingInteger("{=yF9737v3}Skill Point weight", 0f, 1f, "0.0", Order = 13, RequireRestart = false,
            HintText = "{=TmWbtIGD}When calculating skill based affinity, how much weight to give to raw skill points.")]
        [SettingPropertyGroup("{=1w91iyzc}Hero Affinity Calculation/{=SI9lTZoA}Skill Components", GroupOrder = 4)]
        public float HeroAffinity_SkillpointWeight { get; set; } = 1.0f;

        [SettingPropertyFloatingInteger("{=Rn2nrVMG}Focus Point weight", 0f, 1f, "0.0", Order = 14, RequireRestart = false,
            HintText = "{=phhvYJ8s}When calculating skill based affinity, how much weight to give to focus points.")]
        [SettingPropertyGroup("{=1w91iyzc}Hero Affinity Calculation/{=SI9lTZoA}Skill Components", GroupOrder = 4)]
        public float HeroAffinity_FocusPointWeight { get; set; } = 0.6f;

        [SettingPropertyFloatingInteger("{=mL7wouql}Attribute Weight", 0f, 1f, "0.0", Order = 15, RequireRestart = false,
            HintText = "{=RIOPy7Ux}When calculating skill based affinity, how much weight to give to base attributes.")]
        [SettingPropertyGroup("{=1w91iyzc}Hero Affinity Calculation/{=SI9lTZoA}Skill Components", GroupOrder = 4)]
        public float HeroAffinity_AttributeWeight { get; set; } = 0.3f;

        [SettingPropertyFloatingInteger("{=dLyhZkQh}Personal Culture Weight", 0f, 1f, "0.0", Order = 16, RequireRestart = false,
            HintText = "{=yPMGYqg2}When calculating affinity from the hero's culture, how much weight to give to their personal culture.")]
        [SettingPropertyGroup("{=1w91iyzc}Hero Affinity Calculation/{=bWs7kmyH}Culture Components", GroupOrder = 5)]
        public float HeroAffinity_PersonalCultureWeight { get; set; } = 1.0f;

        [SettingPropertyFloatingInteger("{=bPbAJvKW}Clan Culture Weight", 0f, 1f, "0.0", Order = 17, RequireRestart = false,
            HintText = "{=otVMgg2J}When calculating affinity from the hero's culture, how much weight to give to their clan's culture.")]
        [SettingPropertyGroup("{=1w91iyzc}Hero Affinity Calculation/{=bWs7kmyH}Culture Components", GroupOrder = 5)]
        public float HeroAffinity_ClanCultureWeight { get; set; } = 0.5f;

        [SettingPropertyFloatingInteger("{=8FdP5CRn}Father Culture Weight", 0f, 1f, "0.0", Order = 18, RequireRestart = false,
            HintText = "{=4GyclVxa}When calculating affinity from the hero's culture, how much weight to give to their father's culture.")]
        [SettingPropertyGroup("{=1w91iyzc}Hero Affinity Calculation/{=bWs7kmyH}Culture Components", GroupOrder = 5)]
        public float HeroAffinity_FatherCultureWeight { get; set; } = 0.75f;

        [SettingPropertyFloatingInteger("{=6XFwYECd}Mother Culture Weight", 0f, 1f, "0.0", Order = 19, RequireRestart = false,
            HintText = "{=WjfMddpt}When calculating affinity from the hero's culture, how much weight to give to their mother's culture")]
        [SettingPropertyGroup("{=1w91iyzc}Hero Affinity Calculation/{=bWs7kmyH}Culture Components", GroupOrder = 5)]
        public float HeroAffinity_MotherCultureWeight { get; set; } = 0.75f;

        [SettingPropertyFloatingInteger("{=5UFhxXsc}Shield Affinity Multiplier", 0f, 2f, "0.00", Order = 20, RequireRestart = false,
            HintText = "{=zKhndZIo}Use this to encourage or discourage lords from choosing loadouts that include shields. 1 is neutral. At 0, lords will only use shields to fill slots. At 2, only lords with awful 1h skill will not take a shield.")]
        [SettingPropertyGroup("{=1w91iyzc}Hero Affinity Calculation/{=tjQOtPV1}Advanced", GroupOrder = 6)]
        public float HeroAffinity_ShieldAffinityMul { get; set; } = 1.15f;

        [SettingPropertyFloatingInteger("{=6VQKzFDr}Extra Ammo Affinity Multiplier", 0f, 2f, "0.00", Order = 21, RequireRestart = false,
            HintText = "{=0XW8RHno}Use this to encourage or discourage lords from choosing loadouts that include *extra* ammo. 1 is neutral. At 0, only very over specialised lords will take extra ammo. At 2, lords with good ranged stats will often prioritize ammo over shields etc.")]
        [SettingPropertyGroup("{=1w91iyzc}Hero Affinity Calculation/{=tjQOtPV1}Advanced", GroupOrder = 6)]
        public float HeroAffinity_ExtraAmmoAffinityMul { get; set; } = 0.85f;

        [SettingPropertyFloatingInteger("{=BS6qmVwq}Empty Slot Affinity Multiplier", 0f, 2f, "0.00", Order = 22, RequireRestart = false,
            HintText = "{=U4e9Qy3Q}Use this to encourage or discourage lords from choosing loadouts that contain empty slots. 1 is neutral. At 0, lords will almost never leave slots empty. At 2, lords will be quite likely to leave empty slots to focus on their strongest weapon skills.")]
        [SettingPropertyGroup("{=1w91iyzc}Hero Affinity Calculation/{=tjQOtPV1}Advanced", GroupOrder = 6)]
        public float HeroAffinity_EmptySlotMul { get; set; } = 0.70f;

        [SettingPropertyFloatingInteger("{=tkdmVjtm}Gear Catalog Weight", 0f, 1f, "0.0", Order = 23, RequireRestart = false,
            HintText = "{=fslctQ2y}When calculating a culture's affinity for gear, how much weight to give to the gear tagged as theirs.")]
        [SettingPropertyGroup("{=OxVpG9LI}Culture Affinity Calculation", GroupOrder = 7)]
        public float CultureAffinity_GearCatalogWeight { get; set; } = 1.0f;
        [SettingPropertyFloatingInteger("{=m9NTUJBm}Original Lord Loadout Weight", 0f, 1f, "0.0", Order = 24, RequireRestart = false,
            HintText = "{=QZQKv0P9}When calculating a culture's affinity for gear, how much weight to give to the loadouts assigned to the original lords in the data.")]
        [SettingPropertyGroup("{=OxVpG9LI}Culture Affinity Calculation", GroupOrder = 7)]
        public float CultureAffinity_LordLoadoutWeight { get; set; } = 1.0f;

        [SettingPropertyFloatingInteger("{=X7SgVRH2}Culture Strictness", 0f, 5f, "0.0", Order = 25, RequireRestart = false,
            HintText = "{=9ecMqw2O}How rigidily will lords stick to their culture(s) when picking gear. 0 = no cultural preference at all. 2 = strong bias to own culture gear 5 = will only pick gear outside culture when absolutely necessary.")]
        [SettingPropertyGroup("{=ZY38dMdT}Gear Selection", GroupOrder = 8)]
        public float GearChoice_CultureStrictness { get; set; } = 2.0f;

        [SettingPropertyBool("{=OAI8Bc5n}Apply Item Modifiers", Order = 26, RequireRestart = false,
            HintText = "{=KhEwC2lk}Sometimes the system will pick items that don't perfectly match the target tier. This setting will apply modifiers such as masterwork, balanced, rusty, etc to upgrade or downgrade them to fit better.")]
        [SettingPropertyGroup("{=ZY38dMdT}Gear Selection", GroupOrder = 8)]
        public bool GearChoice_ApplyItemModifiers { get; set; } = false;

        [SettingPropertyInteger("{=qhWutrGS}Random Seed", 0, int.MaxValue, "0", Order = 27, RequireRestart = false,
            HintText = "{=Gwsa4M4u}There is some randomness to gear selection. You can shift this to get different results.")]
        [SettingPropertyGroup("{=ZY38dMdT}Gear Selection", GroupOrder = 8)]
        public int RandomSeed {get; set;} = 0;

        [SettingPropertyButton("{=y7uUdloT}Reselect all hero gear now", Order = 28, Content = "{=VgX5lhdx}GO!", RequireRestart = false,
            HintText = "{=wvh4qXHh}Click to select gear for all NPC heroes right now.")]
        [SettingPropertyGroup("{=IM6PJpKJ}Commands", GroupOrder = 9)]
        public Action ReselectAllGearAction { get; set; } = (() => { Campaign.Current?.GetCampaignBehavior<DynamicLordGearBehavior>()?.SelectGearForAllNPCHeroes(onSessionStart:false); });

        public bool DEBUG_ApplyToPlayer = false;
        public bool DEBUG_ApplyToPlayerClan = false;

        public override IEnumerable<ISettingsPreset> GetBuiltInPresets()
        {
            yield return new MemorySettingsPreset(Id, "fix", "{=y2zMpfSw}Fix Ungeared Lords", () => new DynamicLordGearSettings());

            yield return new MemorySettingsPreset(Id, "balanced", "{=59zwUAtF}Balanced", () => new DynamicLordGearSettings()
            {
                UpdateGearForWellEquippedLords = true,
                DynamicGearSelection = true,
                GearTier_Minimum = 1,
            });

            yield return new MemorySettingsPreset(Id, "skill", "{=tWs6t8OM}Skill Focus", () => new DynamicLordGearSettings
            {
                UpdateGearForWellEquippedLords = true,
                DynamicGearSelection = true,
                HeroAffinity_SkillWeight = 1.0f,
                HeroAffinity_CultureWeight = 0.5f,
                GearTier_Minimum = 1,
            });

            yield return new MemorySettingsPreset(Id, "culture", "{=AgkmthgM}Culture Focus", () => new DynamicLordGearSettings
            {
                UpdateGearForWellEquippedLords = true,
                DynamicGearSelection = true,
                HeroAffinity_SkillWeight = 0.5f,
                HeroAffinity_CultureWeight = 1.0f,
                GearTier_Minimum = 1,
            });

            yield return new MemorySettingsPreset(Id, "optimized", "{=EV54yFHi}Optimized Misfits", () => new DynamicLordGearSettings
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

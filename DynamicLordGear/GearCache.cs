using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ObjectSystem;

namespace DynamicLordGear
{
    //We recatagorize gear diffently to the game. Some are one-to-one but others are a bit different.
    //For now not all item types are supported, as Lords are never seen with Pikes, Slings, etc.
    enum GearCategory
    {
        Null = -1,
        OneHanded = 0,
        TwoHanded,

        //Polearms are split into three types based on how they are used. Some are categorized as a Spear *and* a Lance.
        PolearmSpear, //One handed polearms - excluding those with couch lance mode
        PolearmLance, //One handed polearms optimized for cavalry - many lances are also spears
        PolearmSwing, //Two handed polearms that can swing: glaive, voulge etc

        //Axes + Javelins + Daggers are treated the same, cultural gear lists will skew characters to one or the other.
        Throwing,

        //HorseBows are bows that can be used on horse back. All HorseBows are Bows but not all Bows are HorseBows. Ditto for crossbows.
        Bow,
        HorseBow,
        Arrows,
        Crossbow,
        HorseCrossbow,
        Bolts,

        //All HorseShields are also Shields, but overly large shields that look goofy on horse back (eg Pasive) are not HorseShields
        Shield,
        HorseShield,

        //Excludes Camels and Camel Saddles
        Horse,
        Saddle,

        //Armor pieces - the game distinguishes between chest and body armour, but they both go in the same slot.
        HeadArmor,
        NeckArmor,
        ChestOrBodyArmor,
        HandArmor,
        LegArmor,
    }

    class GearCategoryDetails
    {
        public List<ItemObject> GearList = new List<ItemObject>();
    }

    class CultureGearList
    {
        public Dictionary<GearCategory, GearCategoryDetails> GearCategories = new Dictionary<GearCategory, GearCategoryDetails>();
    }

    class GearFavor
    {
        static private Dictionary<ItemObject.ItemTiers, float> _tierToFavour = new Dictionary<ItemObject.ItemTiers, float>()
            {
                { ItemObject.ItemTiers.Tier1, 0.25f },
                { ItemObject.ItemTiers.Tier2, 0.5f },
                { ItemObject.ItemTiers.Tier3, 0.7f },
                { ItemObject.ItemTiers.Tier4, 0.85f },
                { ItemObject.ItemTiers.Tier5, 0.95f },
                { ItemObject.ItemTiers.Tier6, 1.0f },
            };

        public void CalculateFrom(CultureGearList equipmentList)
        {
            foreach (GearCategory category in (GearCategory[])Enum.GetValues(typeof(GearCategory)))
            {
                GearCategoryDetails? gearCategoryDetails = null;
                if (equipmentList.GearCategories.TryGetValue(category, out gearCategoryDetails))
                {
                    ItemObject.ItemTiers highestTier = gearCategoryDetails.GearList.OrderByDescending(item => item.Tier).First().Tier;
                    Values[category] = _tierToFavour[highestTier];
                }
                else
                {
                    Values[category] = 0.0f;
                }
            }
        }

        public GearFavor(float initVal = 0.0f)
        {
            foreach (GearCategory category in (GearCategory[])Enum.GetValues(typeof(GearCategory)))
            {
                Values.Add(category, initVal);
            }
        }

        public GearFavor Mul(float factor)
        {
            GearFavor multiplied = new GearFavor();

            foreach (GearCategory category in (GearCategory[])Enum.GetValues(typeof(GearCategory)))
            {
                multiplied.Values[category] = Values[category] * factor;
            }

            return multiplied;
        }

        public GearFavor Add(GearFavor other)
        {
            GearFavor added = new GearFavor();

            foreach (GearCategory category in (GearCategory[])Enum.GetValues(typeof(GearCategory)))
            {
                added.Values[category] = Values[category] + other.Values[category];
            }

            return added;
        }

        public Dictionary<GearCategory, float> Values = new Dictionary<GearCategory, float>();
    }

    internal class GearCache
    {
        public Dictionary<string, CultureGearList> CultureGear = new Dictionary<string, CultureGearList>();
        public Dictionary<string, GearFavor> CultureFavourFromGear = new Dictionary<string, GearFavor>();
        public Dictionary<string, GearFavor> CultureFavourFromLoadouts = new Dictionary<string, GearFavor>();

        internal void Initialize()
        {
            CacheGear();
            CacheLordLoadouts();
            CalculateCulturalFavourBasedOnGear();
        }

        private void CacheGear()
        {
            ItemObject.ItemTypeEnum[] interestingItemTypes =
            {
                ItemObject.ItemTypeEnum.Horse,
                ItemObject.ItemTypeEnum.OneHandedWeapon,
                ItemObject.ItemTypeEnum.TwoHandedWeapon,
                ItemObject.ItemTypeEnum.Polearm,
                ItemObject.ItemTypeEnum.Arrows,
                ItemObject.ItemTypeEnum.Bolts,
                ItemObject.ItemTypeEnum.Shield,
                ItemObject.ItemTypeEnum.Bow,
                ItemObject.ItemTypeEnum.Crossbow,
                ItemObject.ItemTypeEnum.Thrown,
                ItemObject.ItemTypeEnum.HeadArmor,
                ItemObject.ItemTypeEnum.BodyArmor,
                ItemObject.ItemTypeEnum.ChestArmor,
                ItemObject.ItemTypeEnum.LegArmor,
                ItemObject.ItemTypeEnum.HandArmor,
                ItemObject.ItemTypeEnum.Cape,
                ItemObject.ItemTypeEnum.HorseHarness,
                ItemObject.ItemTypeEnum.Banner
            };

            foreach (ItemObject item in MBObjectManager.Instance.GetObjectTypeList<ItemObject>())
            {
                if (item.IsCraftedByPlayer || item.NotMerchandise || item.IsStealthItem)
                {
                    continue;
                }

                if (!interestingItemTypes.Contains(item.ItemType))
                {
                    continue;
                }

                //Even if some "real" armor is civilian, we might trick our own hieuristic for "undergeared" by giving it to a lord.
                if ((item.ItemType == ItemObject.ItemTypeEnum.BodyArmor
                    || item.ItemType == ItemObject.ItemTypeEnum.ChestArmor
                    || item.ItemType == ItemObject.ItemTypeEnum.HeadArmor
                    || item.ItemType == ItemObject.ItemTypeEnum.HandArmor
                    || item.ItemType == ItemObject.ItemTypeEnum.LegArmor
                    || item.ItemType == ItemObject.ItemTypeEnum.Cape)
                    && item.IsCivilian)
                {
                    continue;
                }

                //Filter out non-horses because lords don't generally ride them and they are a pain to deal with
                const int HorseFamily = 1;
                if (item.ItemType == ItemObject.ItemTypeEnum.Horse)
                {
                    if (item.HorseComponent == null || item.HorseComponent.Monster == null)
                    {
                        continue;
                    }

                    if (item.HorseComponent.Monster.FamilyType != HorseFamily)
                    {
                        continue;
                    }
                }

                if (item.ItemType == ItemObject.ItemTypeEnum.HorseHarness)
                {
                    if (item.ArmorComponent == null)
                    {
                        continue;
                    }

                    if (item.ArmorComponent.FamilyType != HorseFamily)
                    {
                        continue;
                    }
                }

                ItemObject.ItemTiers itemTier = item.Tier;
                ItemObject.ItemTypeEnum itemType = item.Type;

                //There is an actual culture with this name in vanilla, but there's only a few items in it
                //We'll hijack it to be the name for Culture == NULL items too
                string cultureId = "neutral_culture"; 
                if (item.Culture != null)
                {
                    cultureId = item.Culture.StringId;
                }

                if (item.HasWeaponComponent)
                {
                    //Excludes stuff like artillery projectiles
                    if (item.ItemFlags.HasFlag(ItemFlags.CannotBePickedUp))
                    {
                        continue;
                    }

                    GearCategory[] weaponCategory = GetGearCategoriesForWeapon(item.WeaponComponent);

                    for (int i = 0; i < weaponCategory.Length; ++i)
                    {
                        if (weaponCategory[i] != GearCategory.Null)
                        {
                            if (!CultureGear.ContainsKey(cultureId))
                            {
                                CultureGear.Add(cultureId, new CultureGearList());
                            }

                            if (!CultureGear[cultureId].GearCategories.ContainsKey(weaponCategory[i]))
                            {
                                CultureGear[cultureId].GearCategories.Add(weaponCategory[i], new GearCategoryDetails());
                            }

                            if (!CultureGear[cultureId].GearCategories[weaponCategory[i]].GearList.Contains(item))
                            {
                                CultureGear[cultureId].GearCategories[weaponCategory[i]].GearList.Add(item);
                            }
                        }
                    }
                }
                else
                {
                    GearCategory category = GearCategory.Null;

                    switch (item.ItemType)
                    {
                        case ItemObject.ItemTypeEnum.Horse:
                            category = GearCategory.Horse;
                            break;
                        case ItemObject.ItemTypeEnum.HorseHarness:
                            category = GearCategory.Saddle;
                            break;
                        case ItemObject.ItemTypeEnum.HeadArmor:
                            category = GearCategory.HeadArmor;
                            break;
                        case ItemObject.ItemTypeEnum.ChestArmor:
                        case ItemObject.ItemTypeEnum.BodyArmor:
                            category = GearCategory.ChestOrBodyArmor;
                            break;
                        case ItemObject.ItemTypeEnum.LegArmor:
                            category = GearCategory.LegArmor;
                            break;
                        case ItemObject.ItemTypeEnum.HandArmor:
                            category = GearCategory.HandArmor;
                            break;
                        case ItemObject.ItemTypeEnum.Cape:
                            category = GearCategory.NeckArmor;
                            break;
                        default:
                            break;
                    }

                    if (category != GearCategory.Null)
                    {
                        if (!CultureGear.ContainsKey(cultureId))
                        {
                            CultureGear.Add(cultureId, new CultureGearList());
                        }

                        if (!CultureGear[cultureId].GearCategories.ContainsKey(category))
                        {
                            CultureGear[cultureId].GearCategories.Add(category, new GearCategoryDetails());
                        }

                        CultureGear[cultureId].GearCategories[category].GearList.Add(item);
                    }
                }
            }
        }
    
        private void CalculateCulturalFavourBasedOnGear()
        {
            CultureFavourFromGear.Clear();
            foreach (var pair in CultureGear)
            {
                GearFavor culturalWeaponFavour = new GearFavor();
                culturalWeaponFavour.CalculateFrom(pair.Value);
                CultureFavourFromGear.Add(pair.Key, culturalWeaponFavour);
            }
        }

        private GearCategory[] GetGearCategoriesForWeapon(WeaponComponent weaponComponent)
        {
            GearCategory[] weaponCategory = new GearCategory[2] { GearCategory.Null, GearCategory.Null };

            WeaponComponentData weaponData = weaponComponent.PrimaryWeapon;

            bool canBeUsedOneHanded = !weaponData.WeaponFlags.HasFlag(WeaponFlags.NotUsableWithOneHand);

            //Mount-only usages are things like couch-lance
            bool hasMountOnlyUsage = false;
            //Foot-only usages are things like spear brace and throwing spear throw
            bool hasFootOnlyUsage = false;
            foreach (WeaponComponentData weaponMode in weaponComponent.Weapons)
            {
                ItemObject.ItemUsageSetFlags usageSetFlags = MBItem.GetItemUsageSetFlags(weaponMode.ItemUsage);

                if (usageSetFlags.HasFlag(ItemObject.ItemUsageSetFlags.RequiresMount))
                {
                    hasMountOnlyUsage = true;
                }

                if (usageSetFlags.HasFlag(ItemObject.ItemUsageSetFlags.RequiresNoMount))
                {
                    hasFootOnlyUsage = true;
                }
            }

            if (weaponData.IsPolearm)
            {
                bool canSwing = weaponData.SwingDamage > 0;

                if (canSwing)
                {
                    weaponCategory[0] = GearCategory.PolearmSwing;
                }
                else if (canBeUsedOneHanded)
                {
                    float weaponLength = weaponData.GetRealWeaponLength();

                    //Basic heuristic judging by what else is called "lance" in vanilla and generally given to cav
                    bool isLanceLength = weaponLength >= 1.49 && weaponLength <= 2.01;

                    if ((isLanceLength || hasMountOnlyUsage) && !hasFootOnlyUsage)
                    {
                        weaponCategory[0] = GearCategory.PolearmLance;
                    }

                    if (!hasMountOnlyUsage)
                    {
                        weaponCategory[0] = GearCategory.PolearmSpear;
                    }

                }
            }
            else if (weaponData.IsBow)
            {
                weaponCategory[0] = GearCategory.Bow;

                if (!hasFootOnlyUsage)
                {
                    weaponCategory[1] = GearCategory.HorseBow;
                }
            }
            else if (weaponData.IsCrossBow)
            {
                weaponCategory[0] = GearCategory.Crossbow;

                if (!hasFootOnlyUsage && !weaponData.WeaponFlags.HasFlag(WeaponFlags.CantReloadOnHorseback))
                {
                    weaponCategory[1] = GearCategory.HorseCrossbow;
                }
            }
            else if (weaponData.IsShield)
            {
                weaponCategory[0] = GearCategory.Shield;

                MetaMesh shieldMesh = weaponComponent.Item.GetMultiMesh(false, false, false);

                if (shieldMesh != null)
                {
                    BoundingBox shieldBound = shieldMesh.GetBoundingBox();
                    Vec3 size = shieldBound.max - shieldBound.min;

                    //Basic heuristic judging by vanilla data. Horseman shields tend to be 1m or less.
                    //The long infantry shields etc are usually 1.2m+
                    float bigShieldSize = 1.1f;
                    bool bigShield = size.x > bigShieldSize || size.y > bigShieldSize || size.z > bigShieldSize;

                    if (!hasFootOnlyUsage && !bigShield)
                    {
                        weaponCategory[1] = GearCategory.HorseShield;
                    }
                }
            }
            else
            {
                switch (weaponData.WeaponClass)
                {
                    case WeaponClass.Javelin:
                    case WeaponClass.ThrowingAxe:
                    case WeaponClass.ThrowingKnife:
                        weaponCategory[0] = GearCategory.Throwing;
                        break;

                    case WeaponClass.OneHandedAxe:
                    case WeaponClass.OneHandedSword:
                    case WeaponClass.Mace:
                        weaponCategory[0] = GearCategory.OneHanded;
                        break;

                    case WeaponClass.TwoHandedAxe:
                    case WeaponClass.TwoHandedSword:
                    case WeaponClass.TwoHandedMace:
                        weaponCategory[0] = GearCategory.TwoHanded;
                        break;

                    case WeaponClass.Bolt:
                        weaponCategory[0] = GearCategory.Bolts;
                        break;
                    case WeaponClass.Arrow:
                        weaponCategory[0] = GearCategory.Arrows;
                        break;

                    default:
                        break;
                }
            }

            return weaponCategory;
        }

        private void CacheLordLoadouts()
        {
            CultureFavourFromLoadouts.Clear();
            Dictionary<string, int> numLoadoutsInCulture = new Dictionary<string, int>();

            foreach (MBEquipmentRoster equipmentRoster in MBEquipmentRosterExtensions.All)
            {
                if (!equipmentRoster.IsEquipmentTemplate())
                {
                    continue;
                }
                    
                if (!equipmentRoster.HasEquipmentFlags(EquipmentFlags.IsNobleTemplate) || !equipmentRoster.HasEquipmentFlags(EquipmentFlags.IsCombatantTemplate))
                {
                    continue;
                }

                if (equipmentRoster.HasEquipmentFlags(EquipmentFlags.IsNoncombatantTemplate) 
                    || equipmentRoster.HasEquipmentFlags(EquipmentFlags.IsCivilianTemplate))
                {
                    continue;
                }

                if(equipmentRoster.EquipmentCulture == null)
                {
                    continue;
                }

                string cultureId = equipmentRoster.EquipmentCulture.StringId;

                if (!CultureFavourFromLoadouts.ContainsKey(cultureId))
                {
                    CultureFavourFromLoadouts[cultureId] = new GearFavor(0.0f);
                    numLoadoutsInCulture[cultureId] = 0;
                }

                foreach (Equipment equipment in equipmentRoster.AllEquipments)
                {
                    numLoadoutsInCulture[cultureId]++;

                    EquipmentElement horse = equipment[EquipmentIndex.Horse];

                    if(!horse.IsEmpty)
                    {
                        CultureFavourFromLoadouts[cultureId].Values[GearCategory.Horse] += 1.0f;
                    }

                    for (int weaponIndex = (int)EquipmentIndex.Weapon0; weaponIndex < (int)EquipmentIndex.Weapon3; ++weaponIndex)
                    {
                        EquipmentElement weapon = equipment[weaponIndex];

                        if(weapon.Item == null || weapon.Item.WeaponComponent == null)
                        {
                            continue;
                        }

                        GearCategory[] weaponCategory = GetGearCategoriesForWeapon(weapon.Item.WeaponComponent);

                        for (int categoryIndex = 0; categoryIndex < weaponCategory.Length; ++categoryIndex)
                        {
                            if (weaponCategory[categoryIndex] != GearCategory.Null)
                            {
                                CultureFavourFromLoadouts[cultureId].Values[weaponCategory[categoryIndex]] += 1.0f;
                            }
                        }

                        for (int i = 0; i < weaponCategory.Length; ++i)
                        {
                            if (weaponCategory[i] != GearCategory.Null)
                            {
                                if (!CultureGear.ContainsKey(cultureId))
                                {
                                    CultureGear.Add(cultureId, new CultureGearList());
                                }

                                if (!CultureGear[cultureId].GearCategories.ContainsKey(weaponCategory[i]))
                                {
                                    CultureGear[cultureId].GearCategories.Add(weaponCategory[i], new GearCategoryDetails());
                                }

                                if (!CultureGear[cultureId].GearCategories[weaponCategory[i]].GearList.Contains(weapon.Item))
                                {
                                    CultureGear[cultureId].GearCategories[weaponCategory[i]].GearList.Add(weapon.Item);
                                }
                            }
                        }
                    }
                }
            }
            
            //Normalize the weights
            foreach(var pair in numLoadoutsInCulture)
            {
                int numLoadouts = pair.Value;
                CultureFavourFromLoadouts[pair.Key] = CultureFavourFromLoadouts[pair.Key].Mul(1.0f / numLoadouts);
            }
        }
    }
}

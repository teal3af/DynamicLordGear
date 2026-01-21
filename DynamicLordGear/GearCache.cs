using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
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
        static private float[] _tierToFavour = new float[]
        {
            0.25f,  //Tier 1
            0.5f,   //Tier 2
            0.7f,   //Tier 3
            0.85f,  //Tier 4
            0.95f,  //Tier 5
            1.0f,   //Tier 6
        };

        public void CalculateFrom(CultureGearList equipmentList)
        {
            foreach (GearCategory category in (GearCategory[])Enum.GetValues(typeof(GearCategory)))
            {
                GearCategoryDetails? gearCategoryDetails = null;
                if (equipmentList.GearCategories.TryGetValue(category, out gearCategoryDetails))
                {
                    int highestTier = Math.Min(Math.Max((int)gearCategoryDetails.GearList.OrderByDescending(item => item.Tier).First().Tier, 0), _tierToFavour.Length);
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
        static internal bool HasDecentWeapons(Equipment equipment)
        {
            int weaponCount = 0;
            bool hasTwoHander = false;

            for (int i = (int)EquipmentIndex.Weapon0; i <= (int)EquipmentIndex.Weapon3; ++i)
            {
                EquipmentElement weapon = equipment[i];

                if (weapon.Item == null)
                {
                    continue;
                }

                if (!weapon.Item.HasWeaponComponent)
                {
                    continue;
                }

                weaponCount++;

                if (weapon.Item.WeaponComponent.PrimaryWeapon.IsTwoHanded)
                {
                    hasTwoHander = true;
                }
            }

            return weaponCount > 1 || hasTwoHander;
        }

        static internal bool HasDecentArmour(Equipment equipment)
        {
            EquipmentElement bodyArmor = equipment.GetEquipmentFromSlot(EquipmentIndex.Body);
            return !bodyArmor.IsEmpty && bodyArmor.Item != null && !bodyArmor.Item.IsCivilian;
        }

        public readonly Dictionary<string, CultureGearList> CultureGear = new Dictionary<string, CultureGearList>();
        public readonly Dictionary<string, GearFavor> CultureFavourFromGear = new Dictionary<string, GearFavor>();
        public readonly Dictionary<string, GearFavor> CultureFavourFromLoadouts = new Dictionary<string, GearFavor>();

        //We break the outfits down more than native and use slightly different rules.
        public class CultureLoadoutList
        {
            public readonly List<Equipment> MaleLeaderBattle = new List<Equipment>();
            public readonly List<Equipment> MaleLeaderCivilian = new List<Equipment>();

            public readonly List<Equipment> FemaleLeaderBattle = new List<Equipment>();
            public readonly List<Equipment> FemaleLeaderCivilian = new List<Equipment>();

            //Non-combatant is a civilian outfit for someone who fails is the IsCombatant() test (which 
            public readonly List<Equipment> MaleNobleBattle = new List<Equipment>();
            public readonly List<Equipment> MaleNobleCivilian = new List<Equipment>();
            public readonly List<Equipment> MaleNobleNonCombatant = new List<Equipment>();

            public readonly List<Equipment> FemaleNobleBattle = new List<Equipment>();
            public readonly List<Equipment> FemaleNobleCivilian = new List<Equipment>();
            public readonly List<Equipment> FemaleNobleNonCombatant = new List<Equipment>();
        }

        public readonly Dictionary<string, CultureLoadoutList> CultureStandardLoadouts = new Dictionary<string, CultureLoadoutList>();

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

                if (!IsValidItemForDynamicGearPool(item))
                {
                    continue;
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

                //Excludes stuff like artillery projectiles
                if (item.HasWeaponComponent && item.ItemFlags.HasFlag(ItemFlags.CannotBePickedUp))
                {
                    continue;
                }

                GearCategory[] itemGearCategories = GetGearCategoriesForItem(item);

                for (int i = 0; i < itemGearCategories.Length; ++i)
                {
                    if (itemGearCategories[i] != GearCategory.Null)
                    {
                        if (!CultureGear.ContainsKey(cultureId))
                        {
                            CultureGear.Add(cultureId, new CultureGearList());
                        }

                        if (!CultureGear[cultureId].GearCategories.ContainsKey(itemGearCategories[i]))
                        {
                            CultureGear[cultureId].GearCategories.Add(itemGearCategories[i], new GearCategoryDetails());
                        }

                        if (!CultureGear[cultureId].GearCategories[itemGearCategories[i]].GearList.Contains(item))
                        {
                            CultureGear[cultureId].GearCategories[itemGearCategories[i]].GearList.Add(item);
                        }
                    }
                }
            }
        }

        private bool IsValidItemForDynamicGearPool(ItemObject item)
        {
            //Even if some "real" armor is civilian, we might trick our own hieuristic for "undergeared" by giving it to a lord.
            if ((item.ItemType == ItemObject.ItemTypeEnum.BodyArmor
                || item.ItemType == ItemObject.ItemTypeEnum.ChestArmor
                || item.ItemType == ItemObject.ItemTypeEnum.HeadArmor
                || item.ItemType == ItemObject.ItemTypeEnum.HandArmor
                || item.ItemType == ItemObject.ItemTypeEnum.LegArmor
                || item.ItemType == ItemObject.ItemTypeEnum.Cape)
                && item.IsCivilian)
            {
                return false;
            }

            //Filter out non-horses because lords don't generally ride them and they are a pain to deal with
            const int HorseFamily = 1;
            if (item.ItemType == ItemObject.ItemTypeEnum.Horse)
            {
                if (item.HorseComponent == null || item.HorseComponent.Monster == null)
                {
                    return false;
                }

                if (item.HorseComponent.Monster.FamilyType != HorseFamily)
                {
                    return false;
                }
            }

            if (item.ItemType == ItemObject.ItemTypeEnum.HorseHarness)
            {
                if (item.ArmorComponent == null)
                {
                    return false;
                }

                if (item.ArmorComponent.FamilyType != HorseFamily)
                {
                    return false;
                }
            }

            return true;
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

        private GearCategory[] GetGearCategoriesForItem(ItemObject item)
        {
            if (item.HasWeaponComponent)
            {
                return GetGearCategoriesForWeapon(item.WeaponComponent);
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

                return new GearCategory[] { category };
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
        private void SortBattleAndCivilianEquipments(List<Equipment> battleList, List<Equipment> civilianList, MBReadOnlyList<Equipment> equipments)
        {
            foreach (Equipment equipment in equipments)
            {
                if(HasDecentWeapons(equipment) && HasDecentArmour(equipment))
                {
                    battleList.Add(equipment);
                }
                else
                {
                    civilianList.Add(equipment);
                }
            }
        }

        private void CacheLordLoadouts()
        {
            CultureFavourFromLoadouts.Clear();
            Dictionary<string, int> numLoadoutsInCulture = new Dictionary<string, int>();

            //Cache the valid equipment rosters for nobles for adult nobles for each culture
            foreach (MBEquipmentRoster equipmentRoster in MBEquipmentRosterExtensions.All)
            {
                if (!equipmentRoster.IsEquipmentTemplate())
                {
                    continue;
                }
                    
                if (!equipmentRoster.HasEquipmentFlags(EquipmentFlags.IsNobleTemplate))
                {
                    continue;
                }

                if (equipmentRoster.HasEquipmentFlags(EquipmentFlags.IsChildEquipmentTemplate) || equipmentRoster.HasEquipmentFlags(EquipmentFlags.IsTeenagerEquipmentTemplate))
                {
                    continue;
                }

                if (equipmentRoster.EquipmentCulture == null)
                {
                    continue;
                }

                string cultureId = equipmentRoster.EquipmentCulture.StringId;

                if (!CultureStandardLoadouts.ContainsKey(cultureId))
                {
                    CultureStandardLoadouts.Add(cultureId, new CultureLoadoutList());
                }

                //Combatant seems to be used to differentiate characters, rather than outfits. So a combtant civilian outfit is the civilian outfit
                //  for someone who fights. Whearas a noncombatant civilian outfit is the civilian outfit for someone who doesn't fight.
                bool isCombatantRoster = equipmentRoster.HasEquipmentFlags(EquipmentFlags.IsCombatantTemplate);
                bool isCivilianRoster = equipmentRoster.HasEquipmentFlags(EquipmentFlags.IsCivilianTemplate);
                bool isNoncombatantRoster = equipmentRoster.HasEquipmentFlags(EquipmentFlags.IsNoncombatantTemplate);
                bool isFemaleRoster = equipmentRoster.HasEquipmentFlags(EquipmentFlags.IsFemaleTemplate);

                if (!isCombatantRoster && !isCivilianRoster && !isNoncombatantRoster)
                {
                    MBTextManager.SetTextVariable("DLG_AMBIGEOUS_ROSTER_ID", equipmentRoster.StringId);
                    InformationMessage message = new InformationMessage(new TextObject("{=hQz0ceVt}Found noble roster that is not a combatant, civilian, or non combatant: {DLG_AMBIGEOUS_ROSTER_ID}").ToString(), new Color(1.0f,0.0f,0.0f));
                    InformationManager.DisplayMessage(message);
                    continue;
                }

                bool isForBattleUse = isCombatantRoster && !isCivilianRoster;
                bool isForCivilianUse = isCivilianRoster || isNoncombatantRoster;

                //Bit of a hack, but I can't find a good way of doing this.
                //We could check the leader's of each clan, but the kingdom's aren't loaded in their initial state if going into a save.
                bool isLeaderRoster = equipmentRoster.StringId.Contains("king_template");

                //The medium templates are the ones used for gear for coming of age - don't include the rest as there's stuff like minor faction rosters and all sorts
                //TODO - consider including some extra ones as an option
                if (!isLeaderRoster && isForBattleUse && !equipmentRoster.HasEquipmentFlags(EquipmentFlags.IsMediumTemplate))
                {
                    continue;
                }
                
                /*
                foreach(Equipment equipment in equipmentRoster.AllEquipments)
                {
                    //TW have messed up that tagging on some rosters, resulting in obviously female sets counting as male
                    //Everything that suffers from this issue contains a "dress" of some kind though
                    if (!isFemaleRoster)
                    {
                        EquipmentElement body = equipment[EquipmentIndex.Body];
                        if (!body.IsEmpty && body.Item.Name.ToString().Contains("dress") || body.Item.Name.ToString().Contains("feminine"))
                        {
                            InformationMessage message = new InformationMessage($"Found noble roster that is not flagged as female but contains {body.Item.Name.ToString()}: {equipmentRoster.StringId}", new Color(1.0f, 0.0f, 0.0f));
                            InformationManager.DisplayMessage(message);
                        }
                    }
                }
                */

                if (isLeaderRoster)
                {
                    if (isFemaleRoster)
                    {
                        SortBattleAndCivilianEquipments(CultureStandardLoadouts[cultureId].FemaleLeaderBattle, 
                            CultureStandardLoadouts[cultureId].FemaleLeaderCivilian, 
                            equipmentRoster.AllEquipments);
                    }
                    else
                    {
                        SortBattleAndCivilianEquipments(CultureStandardLoadouts[cultureId].MaleLeaderBattle,
                            CultureStandardLoadouts[cultureId].MaleLeaderCivilian,
                            equipmentRoster.AllEquipments);
                    }
                }
                else
                {
                    if (isFemaleRoster)
                    {
                        SortBattleAndCivilianEquipments(CultureStandardLoadouts[cultureId].FemaleNobleBattle,
                            isNoncombatantRoster ? CultureStandardLoadouts[cultureId].FemaleNobleNonCombatant : CultureStandardLoadouts[cultureId].FemaleNobleCivilian,
                            equipmentRoster.AllEquipments);
                    }
                    else
                    {
                        SortBattleAndCivilianEquipments(CultureStandardLoadouts[cultureId].MaleNobleBattle,
                            isNoncombatantRoster ? CultureStandardLoadouts[cultureId].MaleNobleNonCombatant : CultureStandardLoadouts[cultureId].MaleNobleCivilian,
                            equipmentRoster.AllEquipments);
                    }
                }

                if (!CultureFavourFromLoadouts.ContainsKey(cultureId))
                {
                    CultureFavourFromLoadouts[cultureId] = new GearFavor(0.0f);
                    numLoadoutsInCulture[cultureId] = 0;
                }

                foreach (Equipment equipment in equipmentRoster.AllEquipments)
                {
                    if(!HasDecentWeapons(equipment) || !HasDecentArmour(equipment))
                    {
                        continue;
                    }

                    numLoadoutsInCulture[cultureId]++;

                    EquipmentElement horse = equipment[EquipmentIndex.Horse];

                    if(!horse.IsEmpty)
                    {
                        CultureFavourFromLoadouts[cultureId].Values[GearCategory.Horse] += 1.0f;
                    }

                    for (int equipIndex = 0; equipIndex < (int)EquipmentIndex.NumEquipmentSetSlots; ++equipIndex)
                    {
                        EquipmentElement equipElement = equipment[equipIndex];

                        if (equipElement.Item == null)
                        {
                            continue;
                        }

                        GearCategory[] itemGearCategories = GetGearCategoriesForItem(equipElement.Item);

                        //Weapons alter the preferences 
                        if (equipIndex < (int)EquipmentIndex.NumPrimaryWeaponSlots)
                        {
                            for (int categoryIndex = 0; categoryIndex < itemGearCategories.Length; ++categoryIndex)
                            {
                                if (itemGearCategories[categoryIndex] != GearCategory.Null)
                                {
                                    CultureFavourFromLoadouts[cultureId].Values[itemGearCategories[categoryIndex]] += 1.0f;
                                }
                            }
                        }



                        //We can still use the leader to add their weight to preferences, but we shouldn't add their gear to the pool.
                        //If it's common gear amongst lords, it'll be added anyway.
                        if (!isLeaderRoster && IsValidItemForDynamicGearPool(equipElement.Item))
                        {
                            for (int i = 0; i < itemGearCategories.Length; ++i)
                            {
                                if (itemGearCategories[i] != GearCategory.Null)
                                {
                                    if (!CultureGear.ContainsKey(cultureId))
                                    {
                                        CultureGear.Add(cultureId, new CultureGearList());
                                    }

                                    if (!CultureGear[cultureId].GearCategories.ContainsKey(itemGearCategories[i]))
                                    {
                                        CultureGear[cultureId].GearCategories.Add(itemGearCategories[i], new GearCategoryDetails());
                                    }

                                    if (!CultureGear[cultureId].GearCategories[itemGearCategories[i]].GearList.Contains(equipElement.Item))
                                    {
                                        CultureGear[cultureId].GearCategories[itemGearCategories[i]].GearList.Add(equipElement.Item);
                                    }
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

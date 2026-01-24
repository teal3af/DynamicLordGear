using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.ObjectSystem;

namespace DynamicLordGear
{
    internal class GearSelector
    {
        //We use seeded randoms to pick gear, so it won't change unless inputs change.
        //However certain options may change the number of randoms generated, which will throw off anything that comes after.
        //So we always generate the same set to prevent this.
        internal enum GearRandomNumber : int
        {
            HeadArmor = 0,
            BodyArmor,
            LegArmor,
            HandArmor,
            NeckArmor,
            Horse,
            Saddle,
            Weapon0,
            Weapon1,
            Weapon2,
            Weapon3,
            BattleSet,
            CivilianSet,
            COUNT
        }

        internal class GearSelectionParams
        {
            private int[] _values = new int[(int)GearRandomNumber.COUNT];

            public readonly Dictionary<string, float> CultureEquipmentDiscounts = new Dictionary<string, float>();
            public int TargetGearTier { get; private set; } = 0;

            public Hero SelectForHero { get; private set; }
            public GearCache SelectFromCache { get; private set; }

            internal GearSelectionParams(Hero hero, GearCache gearCache)
            {
                SelectForHero = hero;
                SelectFromCache = gearCache;

                MBFastRandom random = new MBFastRandom(hero.Id.InternalValue + (uint)DynamicLordGearSettings.Instance.RandomSeed);

                for(int i = 0; i < (int)GearRandomNumber.COUNT; ++i)
                {
                    _values[i] = random.Next();
                }

                CalculateTargetGearTier(hero);

                if (hero.Culture != null)
                {
                    AddCultureDiscount(hero.Culture.StringId, DynamicLordGearSettings.Instance.HeroAffinity_PersonalCultureWeight);
                }

                if (hero.Clan != null && hero.Clan.Culture != null)
                {
                    AddCultureDiscount(hero.Clan.Culture.StringId, DynamicLordGearSettings.Instance.HeroAffinity_ClanCultureWeight);
                }

                if (hero.Father != null && hero.Father.Culture != null)
                {
                    AddCultureDiscount(hero.Father.Culture.StringId, DynamicLordGearSettings.Instance.HeroAffinity_FatherCultureWeight);
                }

                if (hero.Mother != null && hero.Mother.Culture != null)
                {
                    AddCultureDiscount(hero.Mother.Culture.StringId, DynamicLordGearSettings.Instance.HeroAffinity_MotherCultureWeight);
                }
            }

            private void AddCultureDiscount(string cultureId, float discount)
            {
                if(discount <= 0)
                {
                    return;
                }

                if (CultureEquipmentDiscounts.ContainsKey(cultureId))
                {
                    CultureEquipmentDiscounts[cultureId] = Math.Max(CultureEquipmentDiscounts[cultureId], discount);
                }
                else
                {
                    CultureEquipmentDiscounts[cultureId] = discount;
                }
            }

            private void CalculateTargetGearTier(Hero hero)
            {
                float gearTier = 0.0f;
                float totalGearTierWieght = 0.0f;

                if (DynamicLordGearSettings.Instance.GearTier_ClanTierWeight > 0.0f)
                {
                    int clanTier = 1;

                    if (hero.Clan != null)
                    {
                        clanTier = hero.Clan.Tier;
                    }
                    else if (hero.IsWanderer)
                    {
                        if (DynamicLordGearSettings.Instance.MatchWandererGearTierToPlayer)
                        {
                            clanTier = Hero.MainHero.Clan.Tier;
                        }
                        else
                        {
                            clanTier = DynamicLordGearSettings.Instance.WandererClanTier;
                        }
                    }

                    gearTier += clanTier * DynamicLordGearSettings.Instance.GearTier_ClanTierWeight;
                    totalGearTierWieght += DynamicLordGearSettings.Instance.GearTier_ClanTierWeight;
                }

                if (DynamicLordGearSettings.Instance.GearTier_HeroSkillWeight > 0.0f)
                {
                    int skillTier = 1;

                    int[] combatSkillValues = new int[]
                        {
                    hero.GetSkillValue(DefaultSkills.OneHanded),
                    hero.GetSkillValue(DefaultSkills.TwoHanded),
                    hero.GetSkillValue(DefaultSkills.Polearm),
                    hero.GetSkillValue(DefaultSkills.Bow),
                    hero.GetSkillValue(DefaultSkills.Crossbow),
                    hero.GetSkillValue(DefaultSkills.Throwing)
                        };

                    int highestSkill = combatSkillValues.Max();

                    skillTier = Math.Min(6, highestSkill / 40);

                    gearTier += skillTier * DynamicLordGearSettings.Instance.GearTier_HeroSkillWeight;
                    totalGearTierWieght += DynamicLordGearSettings.Instance.GearTier_HeroSkillWeight;
                }

                if (totalGearTierWieght > 0.0f)
                {
                    gearTier = gearTier / totalGearTierWieght;
                }

                //Clan tier is 1 - 6 but gear tier is 0 - 5 when it's an int
                TargetGearTier = Math.Max(DynamicLordGearSettings.Instance.GearTier_Minimum, (int)gearTier) - 1;
            }

            internal int GetRandom(GearRandomNumber id, int exclusiveMax)
            {
                if(exclusiveMax <= 0)
                {
                    return 0;
                }

                return _values[(int)id] % exclusiveMax;
            }

            internal int GetRandom(GearRandomNumber id, int inclusiveMin, int exclusiveMax)
            {
                if (exclusiveMax <= inclusiveMin)
                {
                    return inclusiveMin;
                }

                return inclusiveMin + GetRandom(id, exclusiveMax - inclusiveMin);
            }
        }

        private static void AccumulateCultureGearFavor(GearSelectionParams gearSelectionParams, string cultureId, float weight, List<GearFavor> cultureGearFavors, List<float> cultureWeights)
        {
            if (weight > 0.0f)
            {
                GearFavor? cultureFavor = null;
                if (gearSelectionParams.SelectFromCache.CultureFavourFromGear.TryGetValue(cultureId, out cultureFavor))
                {
                    cultureGearFavors.Add(cultureFavor);
                    cultureWeights.Add(weight * DynamicLordGearSettings.Instance.CultureAffinity_GearCatalogWeight);
                }
                if (gearSelectionParams.SelectFromCache.CultureFavourFromLoadouts.TryGetValue(cultureId, out cultureFavor))
                {
                    cultureGearFavors.Add(cultureFavor);
                    cultureWeights.Add(weight * DynamicLordGearSettings.Instance.CultureAffinity_LordLoadoutWeight);
                }
            }
        }

        public static GearFavor? GetCultureGearFavorForHero(GearSelectionParams gearSelectionParams)
        {
            List<GearFavor> cultureGearFavors = new List<GearFavor>();
            List<float> cultureWeights = new List<float>();

            if(gearSelectionParams.SelectForHero.Culture != null)
            {
                AccumulateCultureGearFavor(gearSelectionParams, 
                    gearSelectionParams.SelectForHero.Culture.StringId, DynamicLordGearSettings.Instance.HeroAffinity_PersonalCultureWeight, 
                    cultureGearFavors, cultureWeights);
            }

            if (gearSelectionParams.SelectForHero.Clan != null && gearSelectionParams.SelectForHero.Clan.Culture != null)
            {
                AccumulateCultureGearFavor(gearSelectionParams,
                    gearSelectionParams.SelectForHero.Clan.Culture.StringId, DynamicLordGearSettings.Instance.HeroAffinity_ClanCultureWeight,
                    cultureGearFavors, cultureWeights);
            }

            if (gearSelectionParams.SelectForHero.Father != null && gearSelectionParams.SelectForHero.Father.Culture != null)
            {
                AccumulateCultureGearFavor(gearSelectionParams,
                    gearSelectionParams.SelectForHero.Father.Culture.StringId, DynamicLordGearSettings.Instance.HeroAffinity_FatherCultureWeight,
                    cultureGearFavors, cultureWeights);
            }

            if (gearSelectionParams.SelectForHero.Mother != null && gearSelectionParams.SelectForHero.Mother.Culture != null)
            {
                AccumulateCultureGearFavor(gearSelectionParams,
                    gearSelectionParams.SelectForHero.Mother.Culture.StringId, DynamicLordGearSettings.Instance.HeroAffinity_MotherCultureWeight,
                    cultureGearFavors, cultureWeights);
            }

            if (cultureGearFavors.Count == 0)
            {
                return null;
            }

            GearFavor mergedCulturalFavor = new GearFavor();
            float totalWeight = 0.0f;

            for(int i = 0; i < cultureGearFavors.Count; ++i)
            {
                mergedCulturalFavor = mergedCulturalFavor.Add(cultureGearFavors[i].Mul(cultureWeights[i]));
                totalWeight += cultureWeights[i];
            }

            if(totalWeight <= 0.0f)
            {
                return null;
            }

            return mergedCulturalFavor.Mul(1.0f / totalWeight);
        }

        internal ItemObject? GetAppropriateGear(GearSelectionParams gearSelectionParams, GearRandomNumber rngId, GearCategory primaryCategory, GearCategory secondaryCategory = GearCategory.Null)
        {
            List<ItemObject> itemMatches = new List<ItemObject>();
            float lowestCost = float.MaxValue;

            for(int i = 0; i < 2; ++i)
            {
                GearCategory gearCategory = i == 0 ? primaryCategory : secondaryCategory;

                if(gearCategory == GearCategory.Null)
                {
                    continue;
                }

                foreach (var cultureGear in gearSelectionParams.SelectFromCache.CultureGear)
                {
                    string cultureId = cultureGear.Key;
                    CultureGearList gearList = cultureGear.Value;

                    //Culture contains no gear of this type
                    if (!gearList.GearCategories.ContainsKey(gearCategory))
                    {
                        continue;
                    }

                    float cultureCost = DynamicLordGearSettings.Instance.GearChoice_CultureStrictness;

                    float discount = 0.0f;
                    if (gearSelectionParams.CultureEquipmentDiscounts.TryGetValue(cultureId, out discount))
                    {
                        cultureCost *= 1.0f - discount;
                    }

                    GearCategoryDetails gearCategoryList = gearList.GearCategories[gearCategory];

                    foreach (ItemObject item in gearCategoryList.GearList)
                    {
                        int itemTier = (int)item.Tier;

                        float tierDistance = itemTier - gearSelectionParams.TargetGearTier;

                        float totalCost = cultureCost + Math.Abs((tierDistance > 0 ? tierDistance * 1.5f : tierDistance)); ;

                        if (totalCost <= lowestCost)
                        {
                            if (totalCost < lowestCost)
                            {
                                lowestCost = totalCost;
                                itemMatches.Clear();
                            }

                            itemMatches.Add(item);
                        }

                    }
                }
            }

            if (itemMatches.Count > 0)
            {
                return itemMatches[gearSelectionParams.GetRandom(rngId, itemMatches.Count)];
            }

            return null;
        }

        internal void AddModifiedEquipmentToHero(GearSelectionParams gearSelectionParams, EquipmentIndex targetSlot, ItemObject item)
        {
            ItemModifier? modifier = null;

            if(DynamicLordGearSettings.Instance.GearChoice_ApplyItemModifiers)
            {
                if (gearSelectionParams.TargetGearTier != (int)item.Tier && item.ItemComponent != null && item.ItemComponent.ItemModifierGroup != null)
                {
                    List<ItemModifier> sortedItemModifiers = new List<ItemModifier>();

                    //If we went up in tier, go down in quality.
                    if ((int)item.Tier > gearSelectionParams.TargetGearTier)
                    {
                        //Negative modifiers, from least bad to most bad
                        sortedItemModifiers.AddRange(item.ItemComponent.ItemModifierGroup.ItemModifiers.Where(mod => mod.PriceMultiplier < 1.0f).OrderByDescending(mod => mod.PriceMultiplier).ToList());
                    }
                    else
                    {
                        //Postive modifiers, from least good to most good
                        sortedItemModifiers.AddRange(item.ItemComponent.ItemModifierGroup.ItemModifiers.Where(mod => mod.PriceMultiplier > 1.0f).OrderBy(mod => mod.PriceMultiplier).ToList());
                    }

                    int modifierIndex = Math.Min(sortedItemModifiers.Count - 1, Math.Abs(gearSelectionParams.TargetGearTier - (int)item.Tier) - 1);
                    modifier = sortedItemModifiers[modifierIndex];
                }
            }

            gearSelectionParams.SelectForHero.BattleEquipment[targetSlot] = new EquipmentElement(item, modifier);
        }

        internal void SelectDynamicArmorForHero(GearSelectionParams gearSelectionParams)
        {
            ItemObject? bodyOrChestArmor = GetAppropriateGear(gearSelectionParams, GearRandomNumber.BodyArmor, GearCategory.ChestOrBodyArmor);
            ItemObject? handArmor = GetAppropriateGear(gearSelectionParams, GearRandomNumber.HandArmor, GearCategory.HandArmor);
            ItemObject? legArmor = GetAppropriateGear(gearSelectionParams, GearRandomNumber.LegArmor, GearCategory.LegArmor);
            ItemObject? headArmor = GetAppropriateGear(gearSelectionParams, GearRandomNumber.HeadArmor, GearCategory.HeadArmor);
            ItemObject? neckArmor = GetAppropriateGear(gearSelectionParams, GearRandomNumber.NeckArmor, GearCategory.NeckArmor);

            if (bodyOrChestArmor != null)
            {
                AddModifiedEquipmentToHero(gearSelectionParams, EquipmentIndex.Body, bodyOrChestArmor);
            }

            if (handArmor != null)
            {
                AddModifiedEquipmentToHero(gearSelectionParams, EquipmentIndex.Gloves, handArmor);
            }

            if (legArmor != null)
            {
                AddModifiedEquipmentToHero(gearSelectionParams, EquipmentIndex.Leg, legArmor);
            }

            if (headArmor != null)
            {
                AddModifiedEquipmentToHero(gearSelectionParams, EquipmentIndex.Head, headArmor);
            }

            if (neckArmor != null)
            {
                AddModifiedEquipmentToHero(gearSelectionParams, EquipmentIndex.Cape, neckArmor);
            }
        }

        internal void DowngradeCompanionGear(Equipment equipment)
        {
            ItemModifier armorMod = MBObjectManager.Instance.GetObject<ItemModifier>("companion_armor");
            ItemModifier weaponMod = MBObjectManager.Instance.GetObject<ItemModifier>("companion_weapon");
            ItemModifier horseMod = MBObjectManager.Instance.GetObject<ItemModifier>("companion_horse");

            for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumEquipmentSetSlots; equipmentIndex++)
            {
                EquipmentElement equipmentElement = equipment[equipmentIndex];
                if (equipmentElement.Item != null)
                {
                    if (equipmentElement.Item.ArmorComponent != null)
                    {
                        equipment[equipmentIndex] = new EquipmentElement(equipmentElement.Item, armorMod, null, false);
                    }
                    else if (equipmentElement.Item.HorseComponent != null)
                    {
                        equipment[equipmentIndex] = new EquipmentElement(equipmentElement.Item, horseMod, null, false);
                    }
                    else if (equipmentElement.Item.WeaponComponent != null)
                    {
                        equipment[equipmentIndex] = new EquipmentElement(equipmentElement.Item, weaponMod, null, false);
                    }
                }
            }
        }

        internal void SelectDynamicWeaponsForHero(GearSelectionParams gearSelectionParams, LoadoutArchetypes loadoutArchetpyes)
        {
            //Default to all 1 ie everything has the same mul
            GearFavor gearFavour = new GearFavor(1.0f);

            if (DynamicLordGearSettings.Instance.HeroAffinity_CultureWeight > 0.0f)
            {
                GearFavor? cultureGearFavour = GetCultureGearFavorForHero(gearSelectionParams);
                if(cultureGearFavour != null)
                {
                    gearFavour = new GearFavor(1.0f - DynamicLordGearSettings.Instance.HeroAffinity_CultureWeight).Add(cultureGearFavour.Mul(DynamicLordGearSettings.Instance.HeroAffinity_CultureWeight));
                }
            }

            HeroAffinities heroAffinities = new HeroAffinities(1.0f);

            if(DynamicLordGearSettings.Instance.HeroAffinity_SkillWeight > 0.0f)
            {
                heroAffinities.SetFromHero(gearSelectionParams.SelectForHero, DynamicLordGearSettings.Instance.HeroAffinity_SkillWeight);
            }

            bool preferCrossbow = false;
            LoadoutArchetype chosenArchetype = loadoutArchetpyes.ChooseArchetype(heroAffinities, gearFavour, out preferCrossbow);

            List<ItemObject> weaponItemList = new List<ItemObject>();
            ItemObject? ammoItem = null;
            ItemObject? throwingItem = null;

            for (int i = 0; i < 4; ++i)
            {
                ItemObject? appropriateItem = null;

                switch (chosenArchetype.Loadout[i])
                {
                    case LoadoutWeaponArchetype.OneHanded:
                        {
                            //If we have a shield, AI will use a 1h/2h as a 1h.
                            GearCategory secondaryCategory = chosenArchetype.HasShield ? GearCategory.OneOrTwoHanded : GearCategory.Null;
                            appropriateItem = GetAppropriateGear(gearSelectionParams, GearRandomNumber.Weapon0 + i, GearCategory.OneHanded, secondaryCategory);
                            break;
                        }
                        
                    case LoadoutWeaponArchetype.TwoHanded:
                        {
                            //If we don't have a shield, AI will use a 1h/2h as a 2h.
                            GearCategory secondaryCategory = chosenArchetype.HasShield ? GearCategory.Null : GearCategory.OneOrTwoHanded;
                            appropriateItem = GetAppropriateGear(gearSelectionParams, GearRandomNumber.Weapon0 + i, GearCategory.TwoHanded, secondaryCategory);
                            break;
                        }
                    case LoadoutWeaponArchetype.PolearmThrust:
                        appropriateItem = GetAppropriateGear(gearSelectionParams, GearRandomNumber.Weapon0 + i, chosenArchetype.Mounted ? GearCategory.PolearmLance : GearCategory.PolearmSpear);
                        break;
                    case LoadoutWeaponArchetype.PolearmSwing:
                        appropriateItem = GetAppropriateGear(gearSelectionParams, GearRandomNumber.Weapon0 + i, GearCategory.PolearmSwing);
                        break;
                    case LoadoutWeaponArchetype.Throwing:
                        if (throwingItem == null)
                        {
                            throwingItem = GetAppropriateGear(gearSelectionParams, GearRandomNumber.Weapon0 + i, GearCategory.Throwing);
                        }

                        if (throwingItem != null)
                        {
                            appropriateItem = throwingItem;
                        }
                        break;
                    case LoadoutWeaponArchetype.Ranged:
                        if (preferCrossbow)
                        {
                            appropriateItem = GetAppropriateGear(gearSelectionParams, GearRandomNumber.Weapon0 + i, chosenArchetype.Mounted ? GearCategory.HorseCrossbow : GearCategory.Crossbow);
                        }
                        else
                        {
                            appropriateItem = GetAppropriateGear(gearSelectionParams, GearRandomNumber.Weapon0 + i, chosenArchetype.Mounted ? GearCategory.HorseBow : GearCategory.Bow);
                        }
                        break;
                    case LoadoutWeaponArchetype.Ammo:
                        if (ammoItem == null)
                        {
                            if (preferCrossbow)
                            {
                                ammoItem = GetAppropriateGear(gearSelectionParams, GearRandomNumber.Weapon0 + i, GearCategory.Bolts);
                            }
                            else
                            {
                                ammoItem = GetAppropriateGear(gearSelectionParams, GearRandomNumber.Weapon0 + i, GearCategory.Arrows);
                            }
                        }

                        if (ammoItem != null)
                        {
                            appropriateItem = ammoItem;
                        }
                        break;
                    case LoadoutWeaponArchetype.Shield:
                        {
                            appropriateItem = GetAppropriateGear(gearSelectionParams, GearRandomNumber.Weapon0 + i, chosenArchetype.Mounted ? GearCategory.HorseShield : GearCategory.Shield);
                        }
                        break;
                    case LoadoutWeaponArchetype.Empty:
                    default:
                        break;
                }

                if (chosenArchetype.Loadout[i] != LoadoutWeaponArchetype.Empty && appropriateItem == null)
                {
                    //TODO - show this via info manager
                    MBDebug.ShowWarning($"Couldn't find appropriate {chosenArchetype.Loadout[i].ToString()} for {gearSelectionParams.SelectForHero.Name.Value} when calculating loadout.");
                }

                if (appropriateItem != null)
                {
                    weaponItemList.Add(appropriateItem);
                }
            }

            //Apply items
            for (int i = 0; i < (int)EquipmentIndex.NumPrimaryWeaponSlots; ++i)
            {
                EquipmentIndex index = i + EquipmentIndex.WeaponItemBeginSlot;
                if (i < weaponItemList.Count)
                {
                    AddModifiedEquipmentToHero(gearSelectionParams, index, weaponItemList[i]);
                }
                else
                {
                    gearSelectionParams.SelectForHero.BattleEquipment[index] = EquipmentElement.Invalid;
                }
            }

            if (chosenArchetype.Mounted)
            {
                ItemObject? horse = GetAppropriateGear(gearSelectionParams, GearRandomNumber.Horse, GearCategory.Horse);
                ItemObject? saddle = GetAppropriateGear(gearSelectionParams, GearRandomNumber.Saddle, GearCategory.Saddle);

                gearSelectionParams.SelectForHero.BattleEquipment[EquipmentIndex.Horse] = new EquipmentElement(horse);
                gearSelectionParams.SelectForHero.BattleEquipment[EquipmentIndex.HorseHarness] = new EquipmentElement(saddle);
            }
            else
            {
                gearSelectionParams.SelectForHero.BattleEquipment[EquipmentIndex.Horse] = EquipmentElement.Invalid;
                gearSelectionParams.SelectForHero.BattleEquipment[EquipmentIndex.HorseHarness] = EquipmentElement.Invalid;
            }
        }

        //This is similar to GetEquipmentRostersForHeroComeOfAge() - but flattened out and it won't return non-combtant templates even for women
        private List<Equipment> GetStandardEquipmentSetsForLord(GearSelectionParams gearSelectionParams, bool getCivilianEquipment)
        {
            List<List<Equipment>> listsToTry = new List<List<Equipment>>();

            if (gearSelectionParams.SelectForHero.Culture != null)
            {
                GearCache.CultureLoadoutList? cultureEquipmentRoster = null;
                if (gearSelectionParams.SelectFromCache.CultureStandardLoadouts.TryGetValue(gearSelectionParams.SelectForHero.Culture.StringId, out cultureEquipmentRoster))
                {
                    //Woman can fall back to male outfits but not the other way around
                    if (gearSelectionParams.SelectForHero.IsFemale)
                    {
                        if (gearSelectionParams.SelectForHero.IsKingdomLeader)
                        {
                            listsToTry.Add(getCivilianEquipment ? cultureEquipmentRoster.FemaleLeaderCivilian : cultureEquipmentRoster.FemaleLeaderBattle);
                        }

                        if(getCivilianEquipment)
                        {
                            if(gearSelectionParams.SelectForHero.IsNoncombatant)
                            {
                                listsToTry.Add(cultureEquipmentRoster.FemaleNobleNonCombatant);
                                listsToTry.Add(cultureEquipmentRoster.FemaleNobleCivilian);
                            }
                            else
                            {
                                listsToTry.Add(cultureEquipmentRoster.FemaleNobleCivilian);
                                listsToTry.Add(cultureEquipmentRoster.FemaleNobleNonCombatant);
                            }
                        }
                        else
                        {
                            listsToTry.Add(cultureEquipmentRoster.FemaleNobleBattle);
                        }
                    }

                    if (gearSelectionParams.SelectForHero.IsKingdomLeader)
                    {
                        listsToTry.Add(getCivilianEquipment ? cultureEquipmentRoster.MaleLeaderCivilian : cultureEquipmentRoster.MaleLeaderBattle);
                    }

                    if (getCivilianEquipment)
                    {
                        if (gearSelectionParams.SelectForHero.IsNoncombatant)
                        {
                            listsToTry.Add(cultureEquipmentRoster.MaleNobleNonCombatant);
                            listsToTry.Add(cultureEquipmentRoster.MaleNobleCivilian);
                        }
                        else
                        {
                            listsToTry.Add(cultureEquipmentRoster.MaleNobleCivilian);
                            listsToTry.Add(cultureEquipmentRoster.MaleNobleNonCombatant);
                        }
                    }
                    else
                    {
                        listsToTry.Add(cultureEquipmentRoster.MaleNobleBattle);
                    }
                }
            }

            List<Equipment> returnList = new List<Equipment>();

            foreach(List<Equipment> equipList in listsToTry)
            {
                if (equipList.Count > 0)
                {
                    returnList.AddRange(equipList);
                    break; //Only want the first non-empty list.
                }
            }

            if(returnList.Count == 0)
            {
                MBEquipmentRoster? fallbackFallback = MBEquipmentRosterExtensions.All.Find((MBEquipmentRoster x) => x.StringId == (getCivilianEquipment ? "generic_civ_dummy" : "generic_bat_dummy"));

                if (fallbackFallback != null)
                {
                    returnList.AddRange(fallbackFallback.AllEquipments);
                }
            }

            return returnList;
        }

        private void SelectCivilianGearForWanderer(GearSelectionParams gearSelectionParams)
        {
            //Give wanderers really crappy civilian gear
            //This is the vanilla behavior - but it's also desierable because it keeps their hiring cost down
            List<Equipment> civilianEquipments = new List<Equipment>();

            if (gearSelectionParams.SelectForHero.Template != null)
            {
                civilianEquipments.AddRange(gearSelectionParams.SelectForHero.Template.CivilianEquipments);
            }

            if (civilianEquipments.Count == 0)
            {
                MBEquipmentRoster? fallbackCivilianEquipment = MBEquipmentRosterExtensions.All.Find((MBEquipmentRoster x) => x.StringId == "generic_civ_dummy");

                if (fallbackCivilianEquipment != null)
                {
                    civilianEquipments.AddRange(fallbackCivilianEquipment.AllEquipments);
                }
            }

            if (civilianEquipments.Count > 0)
            {
                Equipment randomCivilianEquipmentSet = civilianEquipments[gearSelectionParams.GetRandom(GearRandomNumber.CivilianSet, civilianEquipments.Count)];

                for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; ++i)
                {
                    gearSelectionParams.SelectForHero.CivilianEquipment[i] = new EquipmentElement(randomCivilianEquipmentSet[i].Item);
                }

                DowngradeCompanionGear(gearSelectionParams.SelectForHero.CivilianEquipment);
            }

            if (DynamicLordGearSettings.Instance.GiveWanderersWornOutGear)
            {
                DowngradeCompanionGear(gearSelectionParams.SelectForHero.BattleEquipment);
            }
        }

        private void SelectStandardBattleEquipmentForHero(GearSelectionParams gearSelectionParams)
        {
            List<Equipment> candidateBattleEquipments = new List<Equipment>();

            //For characters that existed at the start of the game, this will revert them to what was written in the xml
            //Unless what was written in the xml resulted in them having no combat, in which case we still give them some other loadout
            //Kingdom leaders should go through the standard sets. If they are the OG kingdom leader, then they will just get their stuff back anyway.
            //If they are a new kingdom leader, then they should get the king outfit.
            if (!gearSelectionParams.SelectForHero.IsKingdomLeader && gearSelectionParams.SelectForHero.CharacterObject != null && gearSelectionParams.SelectForHero.CharacterObject.IsOriginalCharacter)
            {
                MBReadOnlyList<Equipment>? originalEquipment = Hacks.GetOriginalEquipmentRoster(gearSelectionParams.SelectForHero.CharacterObject);

                if(originalEquipment != null)
                {
                    foreach(Equipment battleEquipment in originalEquipment.WhereQ(e => e.IsBattle))
                    {
                        if (GearCache.HasDecentWeapons(battleEquipment) && GearCache.HasDecentArmour(battleEquipment))
                        {
                            candidateBattleEquipments.Add(battleEquipment);
                        }
                    }
                }
            }

            if (candidateBattleEquipments.Count == 0 && gearSelectionParams.SelectForHero.Template != null)
            {
                List<Equipment> templateBattleEquipments = gearSelectionParams.SelectForHero.Template.BattleEquipments.ToList();

                foreach (Equipment battleEquipment in templateBattleEquipments)
                {
                    if (GearCache.HasDecentWeapons(battleEquipment) && GearCache.HasDecentArmour(battleEquipment))
                    {
                        candidateBattleEquipments.Add(battleEquipment);
                    }
                }
            }
            
            if(candidateBattleEquipments.Count == 0)
            {
                candidateBattleEquipments.AddRange(GetStandardEquipmentSetsForLord(gearSelectionParams, getCivilianEquipment: false));
            }

            if (candidateBattleEquipments.Count > 0)
            {
                Equipment chosenEquipment = candidateBattleEquipments[gearSelectionParams.GetRandom(GearRandomNumber.BattleSet, candidateBattleEquipments.Count)];

                for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; ++i)
                {
                    gearSelectionParams.SelectForHero.BattleEquipment[i] = new EquipmentElement(chosenEquipment[i].Item, chosenEquipment[i].ItemModifier);
                }
            }
        }

        private void SelectStandardCivilianEquipmentForHero(GearSelectionParams gearSelectionParams)
        {
            if (gearSelectionParams.SelectForHero.IsWanderer)
            {
                SelectCivilianGearForWanderer(gearSelectionParams);
            }
            else
            {
                List<Equipment> candidateCivilianEquipments = new List<Equipment>();

                //For characters that existed at the start of the game, this will revert them to what was written in the xml
                //Unless what was written in the xml resulted in them having no combat, in which case we still give them some other loadout
                //Kingdom leaders should go through the standard sets. If they are the OG kingdom leader, then they will just get their stuff back anyway.
                //If they are a new kingdom leader, then they should get the king outfit.
                if (!gearSelectionParams.SelectForHero.IsKingdomLeader && gearSelectionParams.SelectForHero.CharacterObject != null && gearSelectionParams.SelectForHero.CharacterObject.IsOriginalCharacter)
                {
                    MBReadOnlyList<Equipment>? originalEquipment = Hacks.GetOriginalEquipmentRoster(gearSelectionParams.SelectForHero.CharacterObject);

                    if (originalEquipment != null)
                    {
                        foreach (Equipment civilianEquipment in originalEquipment.WhereQ(e => e.IsCivilian))
                        {
                            candidateCivilianEquipments.Add(civilianEquipment);
                        }
                    }
                }

                if (candidateCivilianEquipments.Count == 0 && gearSelectionParams.SelectForHero.Template != null)
                {
                    List<Equipment> templateCivilianEquipments = gearSelectionParams.SelectForHero.Template.CivilianEquipments.ToList();

                    foreach (Equipment civilianEquipment in templateCivilianEquipments)
                    {
                        candidateCivilianEquipments.Add(civilianEquipment);
                    }
                }

                if (candidateCivilianEquipments.Count == 0)
                {
                    candidateCivilianEquipments.AddRange(GetStandardEquipmentSetsForLord(gearSelectionParams, getCivilianEquipment: true));
                }

                if (candidateCivilianEquipments.Count > 0)
                {
                    Equipment chosenEquipment = candidateCivilianEquipments[gearSelectionParams.GetRandom(GearRandomNumber.CivilianSet, candidateCivilianEquipments.Count)];

                    for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; ++i)
                    {
                        gearSelectionParams.SelectForHero.CivilianEquipment[i] = new EquipmentElement(chosenEquipment[i].Item, chosenEquipment[i].ItemModifier);
                    }
                }
            }
        }

        internal void SelectGearForHero(GearCache gearCache, LoadoutArchetypes loadoutArchetypes, Hero hero, bool onSessionStart = false)
        {
            if(hero.IsChild)
            {
                return;
            }

            bool undergeared = false;

            //If a wanderer called this function, they must have passed a different check. Skip undergeared checks as most of them technically *are*.
            if(!hero.IsWanderer)
            {
                //Male lords with bugged gear might still have a sword
                bool hasDecentWeapons = GearCache.HasDecentWeapons(hero.BattleEquipment);

                bool hasDecentArmor = GearCache.HasDecentArmour(hero.BattleEquipment);

                undergeared = !hasDecentWeapons || !hasDecentArmor;

                //On session start, only apply gear to heros that are undergeared or in settlements.
                if (onSessionStart && !(hero.CurrentSettlement != null || undergeared))
                {
                    return;
                }

                if (!DynamicLordGearSettings.Instance.UpdateGearForWellEquippedLords && !undergeared)
                {
                    return;
                }
            }

            GearSelectionParams gearSelectionParams = new GearSelectionParams(hero, gearCache);

            bool doDynamicGear = DynamicLordGearSettings.Instance.DynamicGearSelection;

            if(DynamicLordGearSettings.Instance.ExcludeMinorFactions && hero.Clan != null && hero.Clan.IsMinorFaction)
            {
                doDynamicGear = false;
            }

            if (doDynamicGear)
            {
                //Leaders should still get their "king armor" even if dynamic gear is on.
                if (hero.IsKingdomLeader || (hero.IsLord && DynamicLordGearSettings.Instance.UseStandardLordArmor))
                {
                    SelectStandardBattleEquipmentForHero(gearSelectionParams);
                }
                else
                {
                    SelectDynamicArmorForHero(gearSelectionParams);
                }

                SelectDynamicWeaponsForHero(gearSelectionParams, loadoutArchetypes);
            }
            else
            {
                SelectStandardBattleEquipmentForHero(gearSelectionParams);
            }

            //For now, always apply civilian gear. For bugged lords it may be incorrect. Also, previous versions of this mod occasionally
            //  would give lords slightly wrong civvie gear.
            //This func will restore the hand-picked civilian gear for original characters and pick random stuff for 2nd+ generation.
            SelectStandardCivilianEquipmentForHero(gearSelectionParams);
        }
    }
}

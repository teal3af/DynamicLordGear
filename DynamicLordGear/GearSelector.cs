using Helpers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.WeaponDesign;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.ObjectSystem;

namespace DynamicLordGear
{
    internal class GearSelector
    {
        internal int CalculateTargetGearTier(Hero hero)
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
                else if(hero.IsWanderer)
                {
                    if(DynamicLordGearSettings.Instance.MatchWandererGearTierToPlayer)
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

            if(totalGearTierWieght > 0.0f)
            {
                gearTier = gearTier / totalGearTierWieght;
            }

            //Clan tier is 1 - 6 but gear tier is 0 - 5 when it's an int
            return Math.Max(DynamicLordGearSettings.Instance.GearTier_Minimum, (int)gearTier) - 1;
        }

        private GearFavor? GetCultureGearFavorForHero(GearCache gearCache, Hero hero)
        {
            List<GearFavor> cultureGearFavors = new List<GearFavor>();
            List<float> cultureWeights = new List<float>();

            if (DynamicLordGearSettings.Instance.HeroAffinity_PersonalCultureWeight > 0.0f && hero.Culture != null)
            {
                GearFavor? cultureFavor = null;
                if (gearCache.CultureFavourFromGear.TryGetValue(hero.Culture.StringId, out cultureFavor))
                {
                    cultureGearFavors.Add(cultureFavor);
                    cultureWeights.Add(DynamicLordGearSettings.Instance.HeroAffinity_PersonalCultureWeight * DynamicLordGearSettings.Instance.CultureAffinity_GearCatalogWeight);
                }
                if (gearCache.CultureFavourFromLoadouts.TryGetValue(hero.Culture.StringId, out cultureFavor))
                {
                    cultureGearFavors.Add(cultureFavor);
                    cultureWeights.Add(DynamicLordGearSettings.Instance.HeroAffinity_PersonalCultureWeight * DynamicLordGearSettings.Instance.CultureAffinity_LordLoadoutWeight);
                }
            }

            if (DynamicLordGearSettings.Instance.HeroAffinity_ClanCultureWeight > 0.0f && hero.Clan != null && hero.Clan.Culture != null)
            {
                GearFavor? cultureFavor = null;
                if (gearCache.CultureFavourFromGear.TryGetValue(hero.Clan.Culture.StringId, out cultureFavor))
                {
                    cultureGearFavors.Add(cultureFavor);
                    cultureWeights.Add(DynamicLordGearSettings.Instance.HeroAffinity_ClanCultureWeight * DynamicLordGearSettings.Instance.CultureAffinity_GearCatalogWeight);
                }
                if (gearCache.CultureFavourFromLoadouts.TryGetValue(hero.Clan.Culture.StringId, out cultureFavor))
                {
                    cultureGearFavors.Add(cultureFavor);
                    cultureWeights.Add(DynamicLordGearSettings.Instance.HeroAffinity_ClanCultureWeight * DynamicLordGearSettings.Instance.CultureAffinity_LordLoadoutWeight);
                }
            }

            if (DynamicLordGearSettings.Instance.HeroAffinity_FatherCultureWeight > 0.0f && hero.Father != null && hero.Father.Culture != null)
            {
                GearFavor? cultureFavor = null;
                if (gearCache.CultureFavourFromGear.TryGetValue(hero.Father.Culture.StringId, out cultureFavor))
                {
                    cultureGearFavors.Add(cultureFavor);
                    cultureWeights.Add(DynamicLordGearSettings.Instance.HeroAffinity_FatherCultureWeight * DynamicLordGearSettings.Instance.CultureAffinity_GearCatalogWeight);
                }
                if (gearCache.CultureFavourFromLoadouts.TryGetValue(hero.Father.Culture.StringId, out cultureFavor))
                {
                    cultureGearFavors.Add(cultureFavor);
                    cultureWeights.Add(DynamicLordGearSettings.Instance.HeroAffinity_FatherCultureWeight * DynamicLordGearSettings.Instance.CultureAffinity_LordLoadoutWeight);
                }
            }

            if (DynamicLordGearSettings.Instance.HeroAffinity_MotherCultureWeight > 0.0f && hero.Mother != null && hero.Mother.Culture != null)
            {
                GearFavor? cultureFavor = null;
                if (gearCache.CultureFavourFromGear.TryGetValue(hero.Mother.Culture.StringId, out cultureFavor))
                {
                    cultureGearFavors.Add(cultureFavor);
                    cultureWeights.Add(DynamicLordGearSettings.Instance.HeroAffinity_MotherCultureWeight * DynamicLordGearSettings.Instance.CultureAffinity_GearCatalogWeight);
                }
                if (gearCache.CultureFavourFromLoadouts.TryGetValue(hero.Mother.Culture.StringId, out cultureFavor))
                {
                    cultureGearFavors.Add(cultureFavor);
                    cultureWeights.Add(DynamicLordGearSettings.Instance.HeroAffinity_MotherCultureWeight * DynamicLordGearSettings.Instance.CultureAffinity_LordLoadoutWeight);
                }
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

        internal ItemObject? GetAppropriateGear(MBFastRandom rng, GearCache gearCache, Hero hero, int targetGearTier, GearCategory gearCategory)
        {
            //TODO - Could precaulate this instead of doing it in the call
            //could batch hero, gear tier, rng into one structure - gear selector params

            Dictionary<string, float> cultureEquipmentDiscounts = new Dictionary<string, float>();

            if (DynamicLordGearSettings.Instance.HeroAffinity_PersonalCultureWeight > 0.0f && hero.Culture != null)
            {
                if(cultureEquipmentDiscounts.ContainsKey(hero.Culture.StringId))
                {
                    cultureEquipmentDiscounts[hero.Culture.StringId] = Math.Max(cultureEquipmentDiscounts[hero.Culture.StringId], DynamicLordGearSettings.Instance.HeroAffinity_PersonalCultureWeight);
                }
                else
                {
                    cultureEquipmentDiscounts[hero.Culture.StringId] = DynamicLordGearSettings.Instance.HeroAffinity_PersonalCultureWeight;
                }
            }

            if (DynamicLordGearSettings.Instance.HeroAffinity_ClanCultureWeight > 0.0f && hero.Clan != null && hero.Clan.Culture != null)
            {
                if (cultureEquipmentDiscounts.ContainsKey(hero.Clan.Culture.StringId))
                {
                    cultureEquipmentDiscounts[hero.Clan.Culture.StringId] = Math.Max(cultureEquipmentDiscounts[hero.Clan.Culture.StringId], DynamicLordGearSettings.Instance.HeroAffinity_ClanCultureWeight);
                }
                else
                {
                    cultureEquipmentDiscounts[hero.Clan.Culture.StringId] = DynamicLordGearSettings.Instance.HeroAffinity_ClanCultureWeight;
                }
            }

            if (DynamicLordGearSettings.Instance.HeroAffinity_FatherCultureWeight > 0.0f && hero.Father != null && hero.Father.Culture != null)
            {
                if (cultureEquipmentDiscounts.ContainsKey(hero.Father.Culture.StringId))
                {
                    cultureEquipmentDiscounts[hero.Father.Culture.StringId] = Math.Max(cultureEquipmentDiscounts[hero.Father.Culture.StringId], DynamicLordGearSettings.Instance.HeroAffinity_FatherCultureWeight);
                }
                else
                {
                    cultureEquipmentDiscounts[hero.Father.Culture.StringId] = DynamicLordGearSettings.Instance.HeroAffinity_FatherCultureWeight;
                }
            }

            if (DynamicLordGearSettings.Instance.HeroAffinity_MotherCultureWeight > 0.0f && hero.Mother != null && hero.Mother.Culture != null)
            {
                if (cultureEquipmentDiscounts.ContainsKey(hero.Mother.Culture.StringId))
                {
                    cultureEquipmentDiscounts[hero.Mother.Culture.StringId] = Math.Max(cultureEquipmentDiscounts[hero.Mother.Culture.StringId], DynamicLordGearSettings.Instance.HeroAffinity_MotherCultureWeight);
                }
                else
                {
                    cultureEquipmentDiscounts[hero.Mother.Culture.StringId] = DynamicLordGearSettings.Instance.HeroAffinity_MotherCultureWeight;
                }
            }

            List<ItemObject> itemMatches = new List<ItemObject>();
            float lowestCost = float.MaxValue;

            foreach (var cultureGear in gearCache.CultureGear)
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
                if (cultureEquipmentDiscounts.TryGetValue(cultureId, out discount))
                {
                    cultureCost *= 1.0f - discount;
                }

                GearCategoryDetails gearCategoryList = gearList.GearCategories[gearCategory];

                foreach (ItemObject item in gearCategoryList.GearList)
                {
                    int itemTier = (int)item.Tier;

                    float tierDistance = itemTier - targetGearTier;

                    float totalCost = cultureCost + Math.Abs((tierDistance > 0 ? tierDistance * 1.5f : tierDistance)); ;

                    if(totalCost <= lowestCost)
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

            if (itemMatches.Count > 0)
            {
                return itemMatches[rng.Next(itemMatches.Count)];
            }

            return null;
        }

        internal void AddModifiedEquipmentToHero(Hero hero, EquipmentIndex targetSlot, ItemObject item, int targetGearTier)
        {
            ItemModifier? modifier = null;

            if(DynamicLordGearSettings.Instance.GearChoice_ApplyItemModifiers)
            {
                if (targetGearTier != (int)item.Tier && item.ItemComponent != null && item.ItemComponent.ItemModifierGroup != null)
                {
                    List<ItemModifier> sortedItemModifiers = new List<ItemModifier>();

                    //If we went up in tier, go down in quality.
                    if ((int)item.Tier > targetGearTier)
                    {
                        //Negative modifiers, from least bad to most bad
                        sortedItemModifiers.AddRange(item.ItemComponent.ItemModifierGroup.ItemModifiers.Where(mod => mod.PriceMultiplier < 1.0f).OrderByDescending(mod => mod.PriceMultiplier).ToList());
                    }
                    else
                    {
                        //Postive modifiers, from least good to most good
                        sortedItemModifiers.AddRange(item.ItemComponent.ItemModifierGroup.ItemModifiers.Where(mod => mod.PriceMultiplier > 1.0f).OrderBy(mod => mod.PriceMultiplier).ToList());
                    }

                    int modifierIndex = Math.Min(sortedItemModifiers.Count - 1, Math.Abs(targetGearTier - (int)item.Tier) - 1);
                    modifier = sortedItemModifiers[modifierIndex];
                }
            }

            hero.BattleEquipment[targetSlot] = new EquipmentElement(item, modifier);
        }

        internal void SelectArmorForHero(MBFastRandom rng, GearCache gearCache, Hero hero, int targetGearTier)
        {
            ItemObject? bodyOrChestArmor = GetAppropriateGear(rng, gearCache, hero, targetGearTier, GearCategory.ChestOrBodyArmor);
            ItemObject? handArmor = GetAppropriateGear(rng, gearCache, hero, targetGearTier, GearCategory.HandArmor);
            ItemObject? legArmor = GetAppropriateGear(rng, gearCache, hero, targetGearTier, GearCategory.LegArmor);
            ItemObject? headArmor = GetAppropriateGear(rng, gearCache, hero, targetGearTier, GearCategory.HeadArmor);
            ItemObject? neckArmor = GetAppropriateGear(rng, gearCache, hero, targetGearTier, GearCategory.NeckArmor);

            if (bodyOrChestArmor != null)
            {
                AddModifiedEquipmentToHero(hero, EquipmentIndex.Body, bodyOrChestArmor, targetGearTier);
            }

            if (handArmor != null)
            {
                AddModifiedEquipmentToHero(hero, EquipmentIndex.Gloves, handArmor, targetGearTier);
            }

            if (legArmor != null)
            {
                AddModifiedEquipmentToHero(hero, EquipmentIndex.Leg, legArmor, targetGearTier);
            }

            if (headArmor != null)
            {
                AddModifiedEquipmentToHero(hero, EquipmentIndex.Head, headArmor, targetGearTier);
            }

            if (neckArmor != null)
            {
                AddModifiedEquipmentToHero(hero, EquipmentIndex.Cape, neckArmor, targetGearTier);
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

        internal void SelectWeaponsForHero(MBFastRandom rng, GearCache gearCache, LoadoutArchetypes loadoutArchetpyes, Hero hero,  int targetGearTier)
        {
            //Default to all 1 ie everything has the same mul
            GearFavor gearFavour = new GearFavor(1.0f);

            if (DynamicLordGearSettings.Instance.HeroAffinity_CultureWeight > 0.0f)
            {
                GearFavor? cultureGearFavour = GetCultureGearFavorForHero(gearCache, hero);
                if(cultureGearFavour != null)
                {
                    gearFavour = new GearFavor(1.0f - DynamicLordGearSettings.Instance.HeroAffinity_CultureWeight).Add(cultureGearFavour.Mul(DynamicLordGearSettings.Instance.HeroAffinity_CultureWeight));
                }
            }

            HeroAffinities heroAffinities = new HeroAffinities(1.0f);

            if(DynamicLordGearSettings.Instance.HeroAffinity_SkillWeight > 0.0f)
            {
                heroAffinities.SetFromHero(hero, DynamicLordGearSettings.Instance.HeroAffinity_SkillWeight);
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
                        appropriateItem = GetAppropriateGear(rng, gearCache, hero, targetGearTier, GearCategory.OneHanded);
                        break;
                    case LoadoutWeaponArchetype.TwoHanded:
                        appropriateItem = GetAppropriateGear(rng, gearCache, hero, targetGearTier, GearCategory.TwoHanded);
                        break;
                    case LoadoutWeaponArchetype.PolearmThrust:
                        appropriateItem = GetAppropriateGear(rng, gearCache, hero, targetGearTier, chosenArchetype.Mounted ? GearCategory.PolearmLance : GearCategory.PolearmSpear);
                        break;
                    case LoadoutWeaponArchetype.PolearmSwing:
                        appropriateItem = GetAppropriateGear(rng, gearCache, hero, targetGearTier, GearCategory.PolearmSwing);
                        break;
                    case LoadoutWeaponArchetype.Throwing:
                        if (throwingItem == null)
                        {
                            throwingItem = GetAppropriateGear(rng, gearCache, hero, targetGearTier, GearCategory.Throwing);
                        }

                        if (throwingItem != null)
                        {
                            appropriateItem = throwingItem;
                        }
                        break;
                    case LoadoutWeaponArchetype.Ranged:
                        if (preferCrossbow)
                        {
                            appropriateItem = GetAppropriateGear(rng, gearCache, hero, targetGearTier, chosenArchetype.Mounted ? GearCategory.HorseCrossbow : GearCategory.Crossbow);
                        }
                        else
                        {
                            appropriateItem = GetAppropriateGear(rng, gearCache, hero, targetGearTier, chosenArchetype.Mounted ? GearCategory.HorseBow : GearCategory.Bow);
                        }
                        break;
                    case LoadoutWeaponArchetype.Ammo:
                        if (ammoItem == null)
                        {
                            if (preferCrossbow)
                            {
                                ammoItem = GetAppropriateGear(rng, gearCache, hero, targetGearTier, GearCategory.Bolts);
                            }
                            else
                            {
                                ammoItem = GetAppropriateGear(rng, gearCache, hero, targetGearTier, GearCategory.Arrows);
                            }
                        }

                        if (ammoItem != null)
                        {
                            appropriateItem = ammoItem;
                        }
                        break;
                    case LoadoutWeaponArchetype.Shield:
                        {
                            appropriateItem = GetAppropriateGear(rng, gearCache, hero, targetGearTier, chosenArchetype.Mounted ? GearCategory.HorseShield : GearCategory.Shield);
                        }
                        break;
                    case LoadoutWeaponArchetype.Empty:
                    default:
                        break;
                }

                if (chosenArchetype.Loadout[i] != LoadoutWeaponArchetype.Empty && appropriateItem == null)
                {
                    MBDebug.ShowWarning($"Couldn't find appropriate {chosenArchetype.Loadout[i].ToString()} for {hero.Name.Value} when calculating loadout.");
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
                    AddModifiedEquipmentToHero(hero, index, weaponItemList[i], targetGearTier);
                }
                else
                {
                    hero.BattleEquipment[index] = EquipmentElement.Invalid;
                }
            }

            if (chosenArchetype.Mounted)
            {
                ItemObject? horse = GetAppropriateGear(rng, gearCache, hero, targetGearTier, GearCategory.Horse);
                ItemObject? saddle = GetAppropriateGear(rng, gearCache, hero, targetGearTier, GearCategory.Saddle);

                hero.BattleEquipment[EquipmentIndex.Horse] = new EquipmentElement(horse);
                hero.BattleEquipment[EquipmentIndex.HorseHarness] = new EquipmentElement(saddle);
            }
            else
            {
                hero.BattleEquipment[EquipmentIndex.Horse] = EquipmentElement.Invalid;
                hero.BattleEquipment[EquipmentIndex.HorseHarness] = EquipmentElement.Invalid;
            }
        }

        private bool HasDecentWeapons(Equipment equipment)
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

        private bool HasDecentArmour(Equipment equipment)
        {
            EquipmentElement bodyArmor = equipment.GetEquipmentFromSlot(EquipmentIndex.Body);
            return !bodyArmor.IsEmpty && bodyArmor.Item != null && !bodyArmor.Item.IsCivilian;
        }

        //This is similar to GetEquipmentRostersForHeroComeOfAge() - but flattened out and it won't return non-combtant templates even for women
        private MBList<Equipment> GetStandardBattleEquipmentSetsForLord(Hero hero)
        {
            MBList<Equipment> returnList = new MBList<Equipment>();
            MBList<Equipment> firstChoices = new MBList<Equipment>();
            MBList<Equipment> fallbackChoices = new MBList<Equipment>();

            foreach (MBEquipmentRoster equipmentRoster in MBEquipmentRosterExtensions.All)
            {
                if (!equipmentRoster.IsEquipmentTemplate())
                {
                    continue;
                }

                if (equipmentRoster.EquipmentCulture != hero.Culture)
                {
                    continue;
                }

                if (!equipmentRoster.HasEquipmentFlags(EquipmentFlags.IsNobleTemplate) || !equipmentRoster.HasEquipmentFlags(EquipmentFlags.IsMediumTemplate))
                {
                    continue;
                }

                if (equipmentRoster.HasEquipmentFlags(EquipmentFlags.IsNomadTemplate) || equipmentRoster.HasEquipmentFlags(EquipmentFlags.IsWoodlandTemplate)
                    || equipmentRoster.HasEquipmentFlags(EquipmentFlags.IsChildEquipmentTemplate) || equipmentRoster.HasEquipmentFlags(EquipmentFlags.IsTeenagerEquipmentTemplate)
                    || equipmentRoster.HasEquipmentFlags(EquipmentFlags.IsNoncombatantTemplate))
                {
                    continue;
                }

                bool isFallback = hero.IsFemale && !equipmentRoster.HasEquipmentFlags(EquipmentFlags.IsFemaleTemplate);

                foreach (Equipment equipment in equipmentRoster.AllEquipments)
                {
                    if(HasDecentWeapons(equipment) && HasDecentArmour(equipment))
                    {
                        //We have to give women some armor. One of the main aims of this mod is making sure they aren't laughably underequipped in battle.
                        if (isFallback)
                        {
                            fallbackChoices.Add(equipment);
                        }
                        else
                        {
                            firstChoices.Add(equipment);
                        }
                    }
                }
            }

            if(firstChoices.Count > 0)
            {
                returnList.AddRange(firstChoices);
            }
            else if(fallbackChoices.Count > 0)
            {
                returnList.AddRange(fallbackChoices);
            }
            else
            {
                MBEquipmentRoster fallbackFallback = MBEquipmentRosterExtensions.All.Find((MBEquipmentRoster x) => x.StringId == "generic_bat_dummy");

                if(fallbackFallback != null)
                {
                    returnList.AddRange(fallbackFallback.AllEquipments);
                }
            }

            return returnList;
        }

        private void SelectCivilianGearForWanderer(MBFastRandom rng, Hero hero)
        {
            //Give wanderer's really crappy civilian gear
            //This is the vanilla behavior - but it's also desierable because it keeps their hiring cost down
            List<Equipment> civilianEquipments = new List<Equipment>();

            if (hero.Template != null)
            {
                civilianEquipments.AddRange(hero.Template.CivilianEquipments);
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
                Equipment randomCivilianEquipmentSet = civilianEquipments[rng.Next(0, civilianEquipments.Count)];

                for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; ++i)
                {
                    hero.CivilianEquipment[i] = new EquipmentElement(randomCivilianEquipmentSet[i].Item);
                }

                DowngradeCompanionGear(hero.CivilianEquipment);
            }

            if (DynamicLordGearSettings.Instance.GiveWanderersWornOutGear)
            {
                DowngradeCompanionGear(hero.BattleEquipment);
            }
        }

        private void SelectGearForUndergearedHero(MBFastRandom rng, Hero hero)
        {
            //Even if we are using dynamic gear, civilian set will still be wrong too for bugged characters
            MBList<MBEquipmentRoster> civilianEquipmentRosterList = Campaign.Current.Models.EquipmentSelectionModel.GetEquipmentRostersForHeroComeOfAge(hero, isCivilian: true);

            MBEquipmentRoster? randomCivilianEquipmentRoster = null;

            if (!civilianEquipmentRosterList.IsEmpty())
            {
                randomCivilianEquipmentRoster = civilianEquipmentRosterList[rng.Next(0, civilianEquipmentRosterList.Count)];
            }
            else
            {
                randomCivilianEquipmentRoster = MBEquipmentRosterExtensions.All.Find((MBEquipmentRoster x) => x.StringId == "generic_civ_dummy");
            }

            if (randomCivilianEquipmentRoster != null)
            {
                List<Equipment> allCivilianEquipmentSets = new List<Equipment>(randomCivilianEquipmentRoster.AllEquipments);

                if (allCivilianEquipmentSets.Count > 0)
                {
                    Equipment randomCivilianEquipmentSet = allCivilianEquipmentSets[rng.Next(0, allCivilianEquipmentSets.Count)];

                    for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; ++i)
                    {
                        hero.CivilianEquipment[i] = new EquipmentElement(randomCivilianEquipmentSet[i].Item, randomCivilianEquipmentSet[i].ItemModifier);
                    }
                }
            }

            //If we aren't using dynamic gear, then apply vanilla battle gear to fix underequipped characters
            //This isn't *fully* vanilla, as some culture's have NO female battle gear sets.
            if (!DynamicLordGearSettings.Instance.DynamicGearSelection)
            {
                MBList<Equipment> battleEquipmentSetList = GetStandardBattleEquipmentSetsForLord(hero);

                if (battleEquipmentSetList.Count > 0)
                {
                    Equipment randomEquipmentSet = battleEquipmentSetList[rng.Next(0, battleEquipmentSetList.Count)];

                    for (int i = 0; i < (int)EquipmentIndex.NumEquipmentSetSlots; ++i)
                    {
                        hero.BattleEquipment[i] = new EquipmentElement(randomEquipmentSet[i].Item, randomEquipmentSet[i].ItemModifier);
                    }
                }
            }
        }

        internal void SelectGearForHero(GearCache gearCache, LoadoutArchetypes loadoutArchetypes, Hero hero, bool onSessionStart = false)
        {
            MBFastRandom rng = new MBFastRandom(hero.Id.InternalValue);

            bool undergeared = false;

            //If a wanderer called this function, they must have passed a different check. Skip undergeared checks as most of them technically *are*.
            if(!hero.IsWanderer)
            {
                //Male lords with bugged gear might still have a sword
                bool hasDecentWeapons = HasDecentWeapons(hero.BattleEquipment);

                bool hasDecentArmor = HasDecentArmour(hero.BattleEquipment);

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

            if (DynamicLordGearSettings.Instance.DynamicGearSelection)
            {
                int targetGearTier = CalculateTargetGearTier(hero);

                SelectArmorForHero(rng, gearCache, hero, targetGearTier);

                SelectWeaponsForHero(rng, gearCache, loadoutArchetypes, hero, targetGearTier);
            }

            if(hero.IsWanderer)
            {
                SelectCivilianGearForWanderer(rng, hero);
            }
            else if(undergeared)
            {
                SelectGearForUndergearedHero(rng, hero);
            }
        }
    }
}

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.Core;

namespace DynamicLordGear
{
    [HarmonyPatch(typeof(AgingCampaignBehavior), "OnHeroComesOfAge")]
    static internal class Patch_AgingCampaignBehavior_OnHeroComesOfAge
    {
        [HarmonyPostfix]
        static void Postfix(Hero hero)
        {
            DynamicLordGearBehavior dynamicLordGearBehaviour = TaleWorlds.CampaignSystem.Campaign.Current.CampaignBehaviorManager.GetBehavior<DynamicLordGearBehavior>();

            if (dynamicLordGearBehaviour != null)
            {
                dynamicLordGearBehaviour.OnComesOfAge(hero);
            }
        }
    }

    [HarmonyPatch(typeof(EncyclopediaHeroPageVM), "Refresh")]
    static internal class Patch_EncyclopediaHeroPageVM_Refresh
    {
        [HarmonyPostfix]
        static void Postfix(EncyclopediaHeroPageVM __instance)
        {
            __instance.IsLoadingOver = false;

            Hero? hero = __instance.Obj as Hero;

            if(hero != null)
            {
                //Show heroes in their civilian gear if they are not currently a combatant
                //This means everyone doesn't look like a warrior constantly,
                //but we don't have to deal with the headache of adding and removing battle gear as they transition in and out of
                //being combat leaders
                bool shouldShowAsCivilian = false;

                if(hero.IsNotable)
                {
                    shouldShowAsCivilian = true;
                }

                //When outside a party, they are chilling out somewhere
                if(hero.PartyBelongedTo == null || hero.IsPrisoner)
                {
                    //Clan members should still be in their gear as the player manages that.
                    if(hero.Clan != Hero.MainHero.Clan)
                    {
                        shouldShowAsCivilian = true;
                    }
                }

                __instance.HeroCharacter.FillFrom(hero, -1, shouldShowAsCivilian, true);
                __instance.HeroCharacter.SetEquipment(EquipmentIndex.Horse, default(EquipmentElement));
                __instance.HeroCharacter.SetEquipment(EquipmentIndex.HorseHarness, default(EquipmentElement));
                __instance.HeroCharacter.SetEquipment(EquipmentIndex.Head, default(EquipmentElement));
            }

            __instance.IsLoadingOver = true;
        }
    }

    [HarmonyPatch(typeof(CampaignUIHelper), "GetCharacterCode")]
    static internal class Patch_CampaignUIHelper_GetCharacterCode
    {
        [HarmonyPrefix]
        static bool Prefix(CharacterObject character, ref bool useCivilian)
        {
            useCivilian = false;

            if(character.HeroObject != null)
            {
                if(character.HeroObject.IsNotable)
                {
                    useCivilian = true;
                }

                if (character.HeroObject.PartyBelongedTo == null || character.HeroObject.IsPrisoner)
                {
                    if (character.HeroObject.Clan != Hero.MainHero.Clan)
                    {
                        useCivilian = true;
                    }
                }
            }

            return true;
        }
    }

}

using HarmonyLib;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;

namespace DynamicLordGear
{
    internal class DynamicLordGearBehavior : CampaignBehaviorBase
    {
        private static Harmony? _harmony = null;
        private bool _sessionStarted = false;
        public readonly GearCache GearCache = new GearCache();
        private GearSelector _gearSelector = new GearSelector();
        private LoadoutArchetypes _loadoutArchetypes = new LoadoutArchetypes();

        private void SelectGearForNPCHero(Hero hero, bool onSessionStart = false)
        {
            if (!DynamicLordGearSettings.Instance.DEBUG_ApplyToPlayer && hero.IsHumanPlayerCharacter)
            {
                return;
            }
            else if (!DynamicLordGearSettings.Instance.DEBUG_ApplyToPlayerClan && hero.Clan == Hero.MainHero.Clan)
            {
                return;
            }

            if (hero.IsNotable)
            {
                return;
            }

            if(hero.IsWanderer && !DynamicLordGearSettings.Instance.ApplyToWanderers)
            {
                return;
            }

            _gearSelector.SelectGearForHero(GearCache, _loadoutArchetypes, hero, onSessionStart);
        }

        internal void SelectGearForAllNPCHeroes(bool onSessionStart)
        {
            //On start apply gear to all heroes. This will only impact bugged ones, others will have to wait for one of the events.
            foreach (Hero hero in Hero.AllAliveHeroes)
            {
                SelectGearForNPCHero(hero, onSessionStart);
            }
        }

        public void OnSessionLaunch(CampaignGameStarter cgs)
        {

            if (_harmony == null)
            {
                _harmony = new Harmony("DynamicLordGear");
                _harmony.PatchAll();
            }

            GearCache.Initialize();
            _loadoutArchetypes.PopulateLoadouts();

            _sessionStarted = true;

            SelectGearForAllNPCHeroes(true);
        }

        public void OnEnterSettlement(MobileParty party, Settlement settlement, Hero hero)
        {
            if(!_sessionStarted)
            {
                return;
            }

            bool canGetEquipment = settlement.IsTown || settlement.IsCastle;

            if (!canGetEquipment)
            {
                return;
            }

            if(party != null && party.LeaderHero != null)
            {
                SelectGearForNPCHero(party.LeaderHero);
            }
            else if(hero != null)
            {
                SelectGearForNPCHero(hero);
            }
        }

        public void OnComesOfAge(Hero hero)
        {
            if (!_sessionStarted)
            {
                return;
            }

            //This is called after main method has already given them gear.
            //In 1.3.13 the vanilla gear will likely be incorrect.

            //Do not check for player affiliation. Always apply the gear selection this time.
            _gearSelector.SelectGearForHero(GearCache, _loadoutArchetypes, hero);
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunch);
            CampaignEvents.AfterSettlementEntered.AddNonSerializedListener(this, OnEnterSettlement);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}

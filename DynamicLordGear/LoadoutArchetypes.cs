using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicLordGear
{
    //Used to define loadout archetypes. Different to the above because the loadout doesn't care which specific type of weapon used.
    enum LoadoutWeaponArchetype
    {
        OneHanded,      //Any non-polearm one-hander
        TwoHanded,      //Any non-polearm two-hander
        PolearmThrust,  //One handed polearm. Spear or lance, depending on if the loadout is mounted
        PolearmSwing,   //Two handed polearm that can swing
        Throwing,       //Any throwing weapon
        Ranged,         //Bow, HorseBow, Crossbow, or HorseCrossbow, depending on character skill and if the loadout is mounted
        Ammo,           //Ammo for the chosen ranged weapon
        Shield,         //Shield or HorseShield, depending on if the loadout is mounted
        Empty           //Slots can be left empty to increasce the AI's focus on specific weapons
    }

    internal class LoadoutArchetypes
    {
        private List<LoadoutArchetype> _loadoutArchetypes = new List<LoadoutArchetype>();
        internal void PopulateLoadouts()
        {
            //Archetypes are based on the vanilla loadout "rules" for heroes which seem to be:
            //  Max one ranged weapon type
            //  Max two melee weapons
            //  Max three weapons total, but fourth slot can be used for extra ammo if free
            //  Unlike the player, empty slots are acceptable and sometimes desierable to force the AI to focus on their strengths
            //  Spears and lances must always have a backup weapon - the loadout has to work in sieges etc without modification

            //Foot Melee with shields
            //=========================

            //+Two Hander
            _loadoutArchetypes.Add(new LoadoutArchetype(false, LoadoutWeaponArchetype.OneHanded, LoadoutWeaponArchetype.Shield, LoadoutWeaponArchetype.TwoHanded, LoadoutWeaponArchetype.Throwing));
            _loadoutArchetypes.Add(new LoadoutArchetype(false, LoadoutWeaponArchetype.OneHanded, LoadoutWeaponArchetype.Shield, LoadoutWeaponArchetype.PolearmSwing, LoadoutWeaponArchetype.Throwing));

            //+Spear
            _loadoutArchetypes.Add(new LoadoutArchetype(false, LoadoutWeaponArchetype.OneHanded, LoadoutWeaponArchetype.Shield, LoadoutWeaponArchetype.PolearmThrust, LoadoutWeaponArchetype.Throwing));

            //+More Throwing
            _loadoutArchetypes.Add(new LoadoutArchetype(false, LoadoutWeaponArchetype.OneHanded, LoadoutWeaponArchetype.Shield, LoadoutWeaponArchetype.Throwing, LoadoutWeaponArchetype.Throwing));

            //Foot Melee without shield
            //=========================
            _loadoutArchetypes.Add(new LoadoutArchetype(false, LoadoutWeaponArchetype.TwoHanded, LoadoutWeaponArchetype.OneHanded, LoadoutWeaponArchetype.Throwing, LoadoutWeaponArchetype.Throwing));
            _loadoutArchetypes.Add(new LoadoutArchetype(false, LoadoutWeaponArchetype.PolearmSwing, LoadoutWeaponArchetype.OneHanded, LoadoutWeaponArchetype.Throwing, LoadoutWeaponArchetype.Throwing));

            _loadoutArchetypes.Add(new LoadoutArchetype(false, LoadoutWeaponArchetype.TwoHanded, LoadoutWeaponArchetype.Throwing, LoadoutWeaponArchetype.Throwing, LoadoutWeaponArchetype.Throwing));
            _loadoutArchetypes.Add(new LoadoutArchetype(false, LoadoutWeaponArchetype.PolearmSwing, LoadoutWeaponArchetype.Throwing, LoadoutWeaponArchetype.Throwing, LoadoutWeaponArchetype.Throwing));

            _loadoutArchetypes.Add(new LoadoutArchetype(false, LoadoutWeaponArchetype.TwoHanded, LoadoutWeaponArchetype.Empty, LoadoutWeaponArchetype.Empty, LoadoutWeaponArchetype.Empty));
            _loadoutArchetypes.Add(new LoadoutArchetype(false, LoadoutWeaponArchetype.PolearmSwing, LoadoutWeaponArchetype.Empty, LoadoutWeaponArchetype.Empty, LoadoutWeaponArchetype.Empty));

            //Foot Ranged
            //=========================

            //Shield
            _loadoutArchetypes.Add(new LoadoutArchetype(false, LoadoutWeaponArchetype.OneHanded, LoadoutWeaponArchetype.Shield, LoadoutWeaponArchetype.Ranged, LoadoutWeaponArchetype.Ammo));

            //Extra Ammo
            _loadoutArchetypes.Add(new LoadoutArchetype(false, LoadoutWeaponArchetype.OneHanded, LoadoutWeaponArchetype.Ranged, LoadoutWeaponArchetype.Ammo, LoadoutWeaponArchetype.Ammo));
            _loadoutArchetypes.Add(new LoadoutArchetype(false, LoadoutWeaponArchetype.TwoHanded, LoadoutWeaponArchetype.Ranged, LoadoutWeaponArchetype.Ammo, LoadoutWeaponArchetype.Ammo));
            _loadoutArchetypes.Add(new LoadoutArchetype(false, LoadoutWeaponArchetype.PolearmSwing, LoadoutWeaponArchetype.Ranged, LoadoutWeaponArchetype.Ammo, LoadoutWeaponArchetype.Ammo));

            //Mounted Melee
            //=========================

            //Classic Lancer Cav
            _loadoutArchetypes.Add(new LoadoutArchetype(true, LoadoutWeaponArchetype.OneHanded, LoadoutWeaponArchetype.Shield, LoadoutWeaponArchetype.PolearmThrust, LoadoutWeaponArchetype.Empty));
            _loadoutArchetypes.Add(new LoadoutArchetype(true, LoadoutWeaponArchetype.OneHanded, LoadoutWeaponArchetype.Shield, LoadoutWeaponArchetype.PolearmThrust, LoadoutWeaponArchetype.Throwing));

            //Decicated Glaive Cav
            _loadoutArchetypes.Add(new LoadoutArchetype(true, LoadoutWeaponArchetype.PolearmSwing, LoadoutWeaponArchetype.Empty, LoadoutWeaponArchetype.Empty, LoadoutWeaponArchetype.Empty));

            //Throwing weapon enjoyers
            //=========================
            _loadoutArchetypes.Add(new LoadoutArchetype(true, LoadoutWeaponArchetype.OneHanded, LoadoutWeaponArchetype.Shield, LoadoutWeaponArchetype.Throwing, LoadoutWeaponArchetype.Throwing));
            _loadoutArchetypes.Add(new LoadoutArchetype(true, LoadoutWeaponArchetype.TwoHanded, LoadoutWeaponArchetype.Shield, LoadoutWeaponArchetype.Throwing, LoadoutWeaponArchetype.Throwing));
            _loadoutArchetypes.Add(new LoadoutArchetype(true, LoadoutWeaponArchetype.PolearmSwing, LoadoutWeaponArchetype.Shield, LoadoutWeaponArchetype.Throwing, LoadoutWeaponArchetype.Throwing));
            _loadoutArchetypes.Add(new LoadoutArchetype(true, LoadoutWeaponArchetype.TwoHanded, LoadoutWeaponArchetype.Throwing, LoadoutWeaponArchetype.Throwing, LoadoutWeaponArchetype.Throwing));
            _loadoutArchetypes.Add(new LoadoutArchetype(true, LoadoutWeaponArchetype.PolearmSwing, LoadoutWeaponArchetype.Throwing, LoadoutWeaponArchetype.Throwing, LoadoutWeaponArchetype.Throwing));

            //Mounted ranged variants
            //=========================
            _loadoutArchetypes.Add(new LoadoutArchetype(true, LoadoutWeaponArchetype.OneHanded, LoadoutWeaponArchetype.Shield, LoadoutWeaponArchetype.Ranged, LoadoutWeaponArchetype.Ammo));
            _loadoutArchetypes.Add(new LoadoutArchetype(true, LoadoutWeaponArchetype.OneHanded, LoadoutWeaponArchetype.PolearmThrust, LoadoutWeaponArchetype.Ranged, LoadoutWeaponArchetype.Ammo));
            _loadoutArchetypes.Add(new LoadoutArchetype(true, LoadoutWeaponArchetype.OneHanded, LoadoutWeaponArchetype.PolearmSwing, LoadoutWeaponArchetype.Ranged, LoadoutWeaponArchetype.Ammo));
            _loadoutArchetypes.Add(new LoadoutArchetype(true, LoadoutWeaponArchetype.TwoHanded, LoadoutWeaponArchetype.Ranged, LoadoutWeaponArchetype.Ammo, LoadoutWeaponArchetype.Ammo));
            _loadoutArchetypes.Add(new LoadoutArchetype(true, LoadoutWeaponArchetype.PolearmSwing, LoadoutWeaponArchetype.Ranged, LoadoutWeaponArchetype.Ammo, LoadoutWeaponArchetype.Ammo));

        }

        internal LoadoutArchetype ChooseArchetype(HeroAffinities heroAffinities, GearFavor gearFavor, out bool preferCrossbow)
        {
            int bestIndex = -1;
            float bestAffinity = 0.0f;
            preferCrossbow = false;

            for (int i = 0; i < _loadoutArchetypes.Count; ++i)
            {
                bool loadoutPreferCrossbow = false;
                float affinityWithLoadout = _loadoutArchetypes[i].CalculateTotalAffinity(heroAffinities, gearFavor, out loadoutPreferCrossbow);
                if (affinityWithLoadout > bestAffinity)
                {
                    bestIndex = i;
                    bestAffinity = affinityWithLoadout;
                    preferCrossbow = loadoutPreferCrossbow;
                }
            }

            return bestIndex >= 0 ? _loadoutArchetypes[bestIndex] : _loadoutArchetypes[0];
        }
    }

    internal class LoadoutArchetype
    {
        public LoadoutWeaponArchetype[] Loadout = new LoadoutWeaponArchetype[4];
        public bool Mounted = false; //Mounted loadouts have an affinity with riding - non-mounted with athletics

        public LoadoutArchetype(bool mounted, LoadoutWeaponArchetype slot0, LoadoutWeaponArchetype slot1, LoadoutWeaponArchetype slot2, LoadoutWeaponArchetype slot3)
        {
            Mounted = mounted;
            Loadout[0] = slot0;
            Loadout[1] = slot1;
            Loadout[2] = slot2;
            Loadout[3] = slot3;
        }

        public float CalculateTotalAffinity(HeroAffinities heroAffinities, GearFavor culturalAffinities, out bool preferCrossbow)
        {
            float totalAffinity = 0.0f;
            int numEmpty = 0;

            float bowAffinity = heroAffinities.Values[HeroAffinity.Bow] * (Mounted ? culturalAffinities.Values[GearCategory.HorseBow] : culturalAffinities.Values[GearCategory.Bow]);
            float crossbowAffinity = heroAffinities.Values[HeroAffinity.Bow] * (Mounted ? culturalAffinities.Values[GearCategory.HorseCrossbow] : culturalAffinities.Values[GearCategory.Crossbow]);

            preferCrossbow = crossbowAffinity > bowAffinity;

            float rangedAffinity = preferCrossbow ? crossbowAffinity : bowAffinity;

            for (int i = 0; i < 4; ++i)
            {
                switch (Loadout[i])
                {
                    case LoadoutWeaponArchetype.OneHanded:
                        totalAffinity += heroAffinities.Values[HeroAffinity.OneHanded] * culturalAffinities.Values[GearCategory.OneHanded];
                        break;
                    case LoadoutWeaponArchetype.TwoHanded:
                        totalAffinity += heroAffinities.Values[HeroAffinity.TwoHanded] * culturalAffinities.Values[GearCategory.TwoHanded];
                        break;
                    case LoadoutWeaponArchetype.PolearmThrust:
                        totalAffinity += heroAffinities.Values[HeroAffinity.Polearm] * (Mounted ? culturalAffinities.Values[GearCategory.PolearmLance] : culturalAffinities.Values[GearCategory.PolearmSpear]);
                        break;
                    case LoadoutWeaponArchetype.PolearmSwing:
                        totalAffinity += heroAffinities.Values[HeroAffinity.Polearm] * culturalAffinities.Values[GearCategory.PolearmSwing];
                        break;
                    case LoadoutWeaponArchetype.Throwing:
                        totalAffinity += heroAffinities.Values[HeroAffinity.Throwing] * culturalAffinities.Values[GearCategory.Throwing];
                        break;
                    case LoadoutWeaponArchetype.Ranged:
                        totalAffinity += rangedAffinity;
                        break;
                    case LoadoutWeaponArchetype.Ammo:
                        totalAffinity += rangedAffinity * DynamicLordGearSettings.Instance.Loadout_AmmoAffinityMul;
                        break;
                    case LoadoutWeaponArchetype.Shield:
                        totalAffinity += culturalAffinities.Values[GearCategory.OneHanded] * DynamicLordGearSettings.Instance.Loadout_ShieldAffinityMul;
                        break;
                    case LoadoutWeaponArchetype.Empty:
                        numEmpty++;
                        break;
                }
            }

            if (numEmpty == 4)
            {
                return 0.0f;
            }
            else if (numEmpty > 0)
            {
                int numFilled = 4 - numEmpty;
                float avgAffinityPerSlot = totalAffinity / numFilled;
                totalAffinity += avgAffinityPerSlot * numEmpty * DynamicLordGearSettings.Instance.Loadout_EmptySlotMul;
            }

            float mountedCulturalAffinity = culturalAffinities.Values[GearCategory.Horse];
            float footAffinity = 1.0f - mountedCulturalAffinity;

            if (Mounted)
            {
                totalAffinity += heroAffinities.Values[HeroAffinity.Mounted] * mountedCulturalAffinity;
            }
            else
            {
                totalAffinity += heroAffinities.Values[HeroAffinity.Foot] * footAffinity;
            }

            return totalAffinity;
        }
    }

}

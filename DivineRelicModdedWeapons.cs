using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blender.Content;

namespace DivineRelicWithModdedWeapons
{
    /*
     *  The Divine Relic charm randomly changes your weapon whenever you shoot.
     *  However, it uses a hard-coded list of weapons from the game, so custom weapons do not work with it.
     *  This class alters the behavior of the charm so it also considers custom weapons installed through the Blender API.
     */

    public class DivineRelicModdedWeapons
    {
        public static List<string> customWeapons;
        public static List<int> weapons;

        public void Init()
        {
            On.LevelPlayerWeaponManager.LevelInit += LevelInit;
            On.LevelPlayerWeaponManager.HandleWeaponFiring += HandleWeaponFiring;
        }

        public void LevelInit(On.LevelPlayerWeaponManager.orig_LevelInit orig, LevelPlayerWeaponManager self, PlayerId id)
        {
            PlayerData.PlayerLoadouts.PlayerLoadout playerLoadout = PlayerData.Data.Loadouts.GetPlayerLoadout(self.player.id);
            if (playerLoadout.charm == Charm.charm_curse && self.player.stats.CurseCharmLevel > -1)
            {
                if (customWeapons == null)
                {
                    // Get a list of all custom weapons loaded by Blender
                    customWeapons = EquipRegistries.Weapons.GetNames();
                }

                if (weapons == null)
                {
                    weapons = new List<int>();

                    // Add all the base game weapons to the list
                    for (int i = 0; i < WeaponProperties.CharmCurse.availableWeaponIDs.Length; i++)
                    {
                        weapons.Add(WeaponProperties.CharmCurse.availableWeaponIDs[i]);
                    }
                    // Additionally add all custom weapons to the list as well
                    for (int i = 0; i < customWeapons.Count; i++)
                    {
                        Weapon wep = (Weapon)Enum.Parse(typeof(Weapon), customWeapons[i]);
                        weapons.Add((int)wep);
                    }
                }

                self.currentWeapon = (Weapon)weapons[UnityEngine.Random.Range(0, weapons.Count)];
            }
            else
            {
                self.currentWeapon = playerLoadout.primaryWeapon;
            }
            self.weaponPrefabs.Init(self, self.weaponsRoot);
            self.superPrefabs.Init(self.player);
        }

        private void HandleWeaponFiring(On.LevelPlayerWeaponManager.orig_HandleWeaponFiring orig, LevelPlayerWeaponManager self)
        {
            if (self.player.motor.Dashing || self.player.motor.IsHit)
                return;
            if (self.player.input.actions.GetButtonDown(4) || self.player.motor.HasBufferedInput(LevelPlayerMotor.BufferedInput.Super) || self.player.stats.Loadout.charm == Charm.charm_EX && self.player.input.actions.GetButton(3) && !self.ex.firing)
            {
                self.player.motor.ClearBufferedInput();
                Super super = PlayerData.Data.Loadouts.GetPlayerLoadout(self.player.id).super;
                if ((double)self.player.stats.SuperMeter >= (double)self.player.stats.SuperMeterMax && super != Super.None && !self.player.stats.ChaliceShieldOn && self.allowSuper && self.player.stats.Loadout.charm != Charm.charm_EX)
                {
                    self.StartSuper();
                    return;
                }
                if (self.player.stats.CanUseEx && self.ex.Able)
                {
                    self.StartEx();
                    return;
                }
            }
            if (self.ex.firing || self.player.stats.Loadout.charm == Charm.charm_EX)
                return;
            if (self.basic.firing != self.player.input.actions.GetButton(3))
            {
                if (self.player.input.actions.GetButton(3))
                {
                    if (PlayerData.Data.Loadouts.GetPlayerLoadout(self.player.id).charm == Charm.charm_curse && self.player.stats.CurseCharmLevel > -1)
                    {
                        int currentWeapon = (int)self.currentWeapon;

                        // The only change is here: 
                        // Select a random weapon from the weapons list and switch the current weapon to the chosen weapon
                        while ((Weapon)currentWeapon == self.currentWeapon)
                            currentWeapon = (int)weapons[UnityEngine.Random.Range(0, weapons.Count)];
                        self.SwitchWeapon((Weapon)currentWeapon);
                    }
                    else
                        self.StartBasic();
                }
                else
                    self.EndBasic();
            }
            self.basic.firing = self.player.input.actions.GetButton(3);
        }
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This script will handle the gun upgrading This will talk to the playerState as well, since it'll have to subtract money from that.
 *  Due to this functioning through event calls, only one of these scripts needs to be in the scene.
 */
using System;

public class Unlockables : MonoBehaviour {

	// Used to fill the playerState with the traps that they have
	public GameObject[] listOfTraps;

	public bool[] currUnlockWeapons = new bool[15];	// What traps are currently unlocked right now?
	public int[] weaponLevelReq = new int[15];		// What are the level stipulation for each trap?

	// These keep track of how much it costs to upgrade the gun.
	public int[] gunDamageCost = new int[4];
	public int[] gunAmmoCost = new int[4];
	public int[] gunReloadCost = new int[4];
	public int[] gunFireRateCost = new int[4];

	// How much of an upgraade is each attribute? (every time the player upgrades an attribute, this get's added to it)
	public int gunDamageBoost = 5;
	public int gunAmmoBoost = 20;
	public float gunFireRateBoost = 0.5f;
	public float gunReloadRateBoost = 0.5f;

	private PlayerState player;	// Used as a refrence to the playerState.

	void Awake()
	{		
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerState>();
	}

	// When the "menu screen" boots up, any weapon that the player can get will be unlocked.
	void Start()
	{
		for(int i = 0; i < currUnlockWeapons.Length; i++)
		{
			// We check if the player has reached the level requirement for a weapon. If so, the weapon is unlocked.
			if(player.currLevel >= weaponLevelReq[i] && currUnlockWeapons[i] == false)
			{
				currUnlockWeapons[i] = true;
				print("Congrats! At level " + weaponLevelReq[i] + ", you got a new trap!");
			}
		}
	}

	// If the player has enough money, returns the amount they have left after the purchange. Else, returns the current amount they have.
	float DeductCost(float cost)
	{
		float cashOnHand = player.currCurrency;
		if(cashOnHand - cost >= 0f)
			return cashOnHand - cost;
		else
			return cashOnHand;
	}
		
	// Called in event trigger. Upgrades the gun's damage.
	public void UpgradeGunDamage()
	{
		float purch = DeductCost(gunDamageCost[player.currGunDamageIndex]);
		if(player.currCurrency != purch)
		{
			print("Upgraded the gun's damage power to lv " + (player.currGunDamageIndex + 2));
			player.currWeaponDamage += gunDamageBoost;
			player.currCurrency = purch;
			player.currGunDamageIndex++;
		}
	}

	// Called in event trigger. Upgrades gun's ammo
	public void UpgradeGunAmmo()
	{
		float purch = DeductCost(gunAmmoCost[player.currGunAmmoIndex]);
		if(player.currCurrency != purch)
		{
			print("Upgraded the gun's ammo count to lv " + (player.currGunAmmoIndex + 2));
			player.currWeaponAmmo += gunAmmoBoost;
			player.currCurrency = purch;
			player.currGunAmmoIndex++;
		}
	}

	// Called in event trigger. Upgrades gun's fire rate
	public void UpgradeGunFireRate()
	{
		float purch = DeductCost(gunFireRateCost[player.currGunFireIndex]);
		if(player.currCurrency != purch)
		{
			print("Upgraded the gun's fire rate to lv " + (player.currGunFireIndex + 2));
			player.currWeaponShotRate += gunFireRateBoost;
			player.currCurrency = purch;
			player.currGunFireIndex++;
		}
	}

	// Called in event trigger. Upgrades gun reload rate.
	public void UpgradeGunReloadRate()
	{
		float purch = DeductCost(gunReloadCost[player.currGunReloadIndex]);
		if(player.currCurrency != purch)
		{
			print("Upgraded the gun's reload speed to lv " + (player.currGunReloadIndex + 2));
			player.currWeaponReloadRate += gunReloadRateBoost;
			player.currCurrency = purch;
			player.currGunReloadIndex++;
		}
	}

	// This loads in the equipped traps that the player has unlocked.
	public void LoadEquippedTraps(string[] equippedTrapNames)
	{
		for(int i = 0; i < listOfTraps.Length; i++)
		{
			for(int j = 0; j < equippedTrapNames.Length; j++)
			{
				// Since the string has both the name and the slot it was on, we have to make sure we only compare the name part.
				if(equippedTrapNames[j].Substring(0,equippedTrapNames[j].Length - 1) == listOfTraps[i].name)
				{
					// We then extract out the number part of the string to be used to determine where it was part of originally.
					int slot = Int32.Parse(equippedTrapNames[j].Substring(equippedTrapNames[j].Length - 1, 1));
					player.currEquippedTraps[slot] = listOfTraps[i];
					break;
				}
			}
		}
	}

}

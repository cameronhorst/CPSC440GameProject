﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

/* 
 * Here we declare variables that are going to be saved. 
 * 
 * To save data in the game, we need to save the following:
 * - Player's current level, current EXP, current amount of money and amount of EXP to level up
 * - Which traps are unlocked
 * - Weapon damage level, reload speed, ammo size, and fire rate.
 * - Level highscores
 * 
 * When the player reloads the game, they start out in the space ship, which occurs when they boot up the app. 
*/ 

[Serializable]
public class PlayerStatistics
{
	// Player Stats
	public int Level;
	public float NextLevel;
	public float XP;
	public float Currency;

	// Player's Gun
	public int WeaponDamage;
	public int WeaponAmmo;
	public float WeaponShotRate;
	public float WeaponReloadRate;

	// What are the names of the traps the player currently have equipped?
	public string[] EquippedTraps = new string[3];
    
	// Traps that are currently unlocked. This will be a filled array that has a bool for each one, indicating if it's unlocked.
	public bool[] UnlockWeapons = new bool[15];

	// Level highscores
	public int[] LevelHighScores;

	// This keeps track of the index position that each gun upgrade is at.
	public int GunDamageIndex = 0;
	public int GunAmmoIndex = 0;
	public int GunReloadIndex = 0;
	public int GunFireIndex = 0;

	// The current game state. This essentially keeps track of what part the game is at. Used in controlling when cutscenes play out.
	public int GameState = 0;

}

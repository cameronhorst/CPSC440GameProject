using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script causes any Enemy that's in the radius of this Dummy to "attack" it.

public class DummyTrap : MonoBehaviour {

	public Transform[] distractedAliens;		// Keeps a list of aliens that are distracted by the dummy
	public int distratedAliensIndex;			// Keeps track of the position of the distractedAliens.
	public int drainHealthMultiplier;			// How fast does the dummy's heath drain?

	private GameObject playerLoc;
	private Health health;
	private bool inProgress = false;

	void Awake()
	{
		playerLoc = GameObject.Find("PlayerSpawnLocation");
		health = GetComponent<Health>();
	}

	// When the alien get's close to the dummy, it'll attack it.
	void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Alien")
		{
			distractedAliens[distratedAliensIndex] = other.transform.parent;
			distratedAliensIndex++;

			AlienNavMeshInterface thisAlien = other.GetComponentInParent<AlienNavMeshInterface>();
			SmallAlienAI thisAlienAI = other.GetComponentInParent<SmallAlienAI>();
			thisAlien.target = gameObject;
            thisAlien.SetNavMeshAgentDestination(transform.position);
			thisAlienAI.SetState(SmallAlienAI.States.Navigating);
			thisAlien.attackingPlayer = false;
			health.healthRateMultiplier += drainHealthMultiplier;
		}
	}

	// When the dummy get's destroyed, all of the aliens on it will move back to the player.
	void OnDestroy()
	{
		for(int i = 0; i < distractedAliens.Length; i++)
		{
			if(distractedAliens[i] != null)
			{
				AlienNavMeshInterface thisAlien = distractedAliens[i].GetComponent<AlienNavMeshInterface>();
				SmallAlienAI thisAlienAI = distractedAliens[i].GetComponent<SmallAlienAI>();

				thisAlien.target = playerLoc;
                thisAlien.SetNavMeshAgentDestination(playerLoc.transform.position);
                thisAlienAI.SetState(SmallAlienAI.States.Navigating);
                thisAlien.nmo.enabled = false;
                thisAlien.attackingPlayer = true;
			}
		}
	}
}

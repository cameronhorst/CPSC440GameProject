using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using System.Collections;

public class TrapButton : MonoBehaviour{
    private Marker marker;

    public Animator btnAnimator;

    public GameObject Trap;

    public GameObject TempTrap;

    private bool highlighted;

    public Color startColor;

    public Color highlightedColor;

    private Material btnMat;

    public float highlightTime = 1f;

    private float highlightedTime;

    private float unhighlightedTime;

    private GameObject objectToSpawn;
    private PlayerManager playerManager;
    
    public Material tempMaterial;

	public int representWhatSpot;		// What spot does this section represent?

	// Use this for initialization
	void Start ()
    {
        btnAnimator = gameObject.GetComponent<Animator>();
        highlighted = false;
        btnMat = gameObject.GetComponent<Renderer>().material;
        startColor = btnMat.color;
        marker = GameObject.Find("CircleMarker_Prefab").GetComponent<Marker>();
        playerManager = GameObject.Find("PlayerSpawnLocation").GetComponent<PlayerManager>();        
        
        // Assigns the equipped trap from the playerState to this spot.
        //if(GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerState>() != null)
          //Trap = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerState>().currEquippedTraps[representWhatSpot];
	}

    void Update()
    {
        if(highlightedTime > Time.time)
        {
            //Debug.Log("Fading");
            btnMat.color = Color.Lerp(startColor, highlightedColor, Time.time / highlightedTime);
        }

        if( unhighlightedTime > Time.time)
        {
            btnMat.color = Color.Lerp(highlightedColor, startColor, Time.time / unhighlightedTime);
        }

        if (Input.GetKeyUp(KeyCode.Mouse0) || Input.GetKeyUp(KeyCode.Joystick1Button1))
        {
            if (highlighted)
            {
                finalizeTrapPlacement();
            }
            else
            {
                Destroy(objectToSpawn);
                objectToSpawn = null;
            }
        }

    }

    // Update is called once per frame
    
    public void finalizeTrapPlacement()
    {
        if(objectToSpawn != null)
        {
            Vector3 spawnPos = marker.transform.position + Vector3.up * (Trap.GetComponent<Collider>().bounds.extents.y + .05f);

            GameObject _trap = (GameObject)Instantiate(this.Trap, spawnPos, objectToSpawn.transform.rotation);
            Destroy(objectToSpawn);
            
			// Makes the placement of the trap play its animation
			if(_trap.GetComponent<Trap>() != null)
				_trap.GetComponent<Trap>().PlaceTrap();
			
            objectToSpawn = null;
            playerManager.IncrementNumTraps(false);
        }
    }   

    public void spawnTrap()
    {

        if(Trap != null)
        {
            //Debug.Log("Spawned");

            Vector3 spawnLocation;

            //Debug.Log(Trap.GetComponent<MeshRenderer>().bounds.extents.y);
            spawnLocation = marker.transform.position + Vector3.up * (Trap.GetComponent<Collider>().bounds.extents.y + .05f);

            Vector3 rotationDir = (Camera.main.transform.forward);
            rotationDir.Set(-rotationDir.x, 0, -rotationDir.z);
            GameObject _trap;

            if (TempTrap != null)
            {
                _trap = (GameObject)Instantiate(this.TempTrap, spawnLocation, Quaternion.LookRotation(rotationDir));
                objectToSpawn = _trap;
            }
            else
            {
                _trap = (GameObject)Instantiate(this.Trap, spawnLocation, Quaternion.LookRotation(rotationDir));
                objectToSpawn = _trap;
            }

            if (_trap.GetComponent<Renderer>())
            {
                _trap.GetComponent<Renderer>().material = tempMaterial;
            }

            if (_trap.GetComponent<Health>())
            {
                _trap.GetComponent<Health>().dead = true;
            }

            if (_trap.GetComponent<PlayerThrowableObject>())
            {
                _trap.GetComponent<PlayerThrowableObject>().ready = false;
            }

            Transform[] children = _trap.GetComponentsInChildren<Transform>();
            foreach (Transform t in children)
            {
                if (t.gameObject.GetComponent<Renderer>())
                {
                    t.GetComponent<Renderer>().material = tempMaterial;
                }
            }
            
        }
        
    }


	// Update is called once per frame
	public void toggleButtonHighlighted () 
    {
        highlighted = !highlighted;

        if (highlighted)
        {
            if(objectToSpawn == null)
            {
                spawnTrap();
            }
            else
            {
                objectToSpawn.SetActive(true);
            }

            btnAnimator.SetBool("Highlighted", true);
            highlightedTime = Time.time + highlightTime;
        }
        else
        {
            btnAnimator.SetBool("Highlighted", false);
            unhighlightedTime = Time.time + highlightTime;
            if (objectToSpawn != null)
            {
                objectToSpawn.SetActive(false);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemGroupScript : MonoBehaviour {

    public GameObject[] gems;
    public float gemSpawnRadius;
    public bool highlightingGems;
    public GameObject[] spawnedGems;
    private bool calledFred = false;
    private float startHighlightTime;
    public float highlightFadeTime = 1f;
    public FredWaypoint GemLocationWaypoint;
    ContextualScreenManager ContextualScreen;
    ContextualScreenPage ScanPage;
    FredMovementScript Fred;
    public GameObject GemIconPrefab;
    private Color GemIconStartColor;
    public LayerMask GroundMask;
    private GameObject _gemIcon;
    private Material _gemIconMat;
    public Color InvisibleColor;
    public float IconFadeTime = 0.25f;

	// Use this for initialization
	void Start ()
    {
        ContextualScreen = GameObject.Find("ContextualScreenGroup").GetComponent<ContextualScreenManager>();
        ScanPage = ContextualScreen.ContextualPages[3];
        Fred = GameObject.Find("Fred").GetComponent<FredMovementScript>();
        //get reference to parent (breakableRockScript)
        //get local variables of breakable rock's min and max gems
        //call CreateGems(min, max)

        //CreateGems(2, 6);
        GemIconStartColor = GemIconPrefab.GetComponent<MeshRenderer>().sharedMaterial.color;
	}




    public void CreateGems(int minGems, int maxGems)
    {
		int numberOfGems = Random.Range (minGems, maxGems);
        spawnedGems = new GameObject[numberOfGems];
		for (int i = 0; i < numberOfGems; i++) 
		{
			Vector3 spawnLocation = transform.position;
			Vector3 offset = Random.insideUnitSphere * gemSpawnRadius;
			spawnLocation += offset;

			int gemToSpawn = Random.Range (0, gems.Length - 1);
			GameObject _gem = (GameObject)Instantiate (gems [gemToSpawn], spawnLocation, Quaternion.identity, transform);
			spawnedGems[i] = _gem;
            _gem.GetComponent<GemScript>().GemGroup = this;
		}
    }

    public void WakeGems()
    {
        //call from breakable rock script when rock breaks
        //foreach of the gems, set them to active
		foreach(GameObject gem in spawnedGems)
		{
			gem.SetActive (true);
		}
			
    }



    public void createGemIcon()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down,  out hit, 3f, GroundMask))
        {
            
            _gemIcon = (GameObject)Instantiate(GemIconPrefab, hit.point, Quaternion.identity, transform);
            Quaternion rotation = Quaternion.FromToRotation(_gemIcon.transform.up, hit.normal);
            _gemIcon.transform.rotation = rotation;
            _gemIconMat = _gemIcon.GetComponent<MeshRenderer>().material;
            _gemIconMat.color = InvisibleColor;
            _gemIcon.GetComponent<MeshRenderer>().enabled = false;
        }
    }


    public void Highlight()
    {
        startHighlightTime = Time.time;

        if (!highlightingGems && !calledFred)
        {
            ContextualScreen.SwitchToPage(ScanPage);

            highlightingGems = true;
        }
    }

    public void Unhighlight(bool stopNow = false)
    {
        if (stopNow)
        {
            ContextualScreen.SwitchToDefaultPage(ScanPage);
            highlightingGems = false;
        }
        else
        {
            StopCoroutine(TryToStopHighlighting());
            StartCoroutine(TryToStopHighlighting());
        }

    }



    IEnumerator FadingInIcon()
    {
        float startTime = Time.time;
        _gemIcon.GetComponent<MeshRenderer>().enabled = true;
        while(Time.time < startTime + IconFadeTime)
        {
            _gemIconMat.color = Color.Lerp(InvisibleColor, GemIconStartColor, (Time.time - startTime) / IconFadeTime);
            yield return null;
        }
    }

    public void FadeOutIcon()
    {
        StartCoroutine(FadingOutIcon());
    }

    IEnumerator FadingOutIcon()
    {
        float startTime = Time.time;
        while (Time.time < startTime + IconFadeTime)
        {
            _gemIconMat.color = Color.Lerp(GemIconStartColor, InvisibleColor, (Time.time - startTime) / IconFadeTime);
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        Destroy(_gemIcon);
    }

    IEnumerator TryToStopHighlighting()
    {
        float startUnhighlightTime = Time.time;

        yield return new WaitForSeconds(highlightFadeTime);

        if(startHighlightTime < startUnhighlightTime)
        {
            ContextualScreen.SwitchToDefaultPage(ScanPage);
            highlightingGems = false;
        }

        yield break;
    }

    public void collectGems()
    {
        if (!calledFred)
        {
            calledFred = true;
            Fred.ScanGem(this);
            StartCoroutine(FadingInIcon());
            foreach (GameObject _gem in spawnedGems)
            {
                _gem.GetComponent<SphereCollider>().enabled = false;
            }
        }
        //Call fred - Fred.CollectGems(Vector 3 position, Gem[],WaypointScript waypoint)
        //Disable Sphere Collider on Gems
        //Add game sum mass to current materials in player manager

        


    }

	// Update is called once per frame
	void Update () {
		
	}
}

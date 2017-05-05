using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapCard : MonoBehaviour {

    public Color InvisibleColor;
    public bool visible;
    public bool equipped;				// Is this trap card equipped?
    public Transform trapIconPos;
    public float fadeTime = 0.1f;
    public List<MeshRenderer> ObjectsToFade = new List<MeshRenderer>();
    private Color StartColor;
    public bool StartVisible;
	public static EquipTrapRadial[] trapRadials;	// Refrence to radial buttons. Used to interact with equipTrapRadial
	public GameObject associatedTrap;// What Trap prefab is associated to this spot?
    public float checkFadeTime = .2f;
    public TrapCardSpawner trapCardSpawner;
    public Transform CenterPoint;
    public float bounds = 0.8f;
    public AnimationCurve visibilityFalloff;
    public float slidingVisisbilityBounds = 3f;
    public Color LockedColor;
    public Color HighlightColor;
    private bool sliding = false;
    public TrapCanvasScript trapCanvas;
    bool canvasOpen = false;
    public float flashInterval = 0.2f;
    bool purchaseConfirmation = false;
    public ConfirmationGroup confirmationGroup;

	// Use this for initialization
	void Start ()
    {
        ObjectsToFade.Add(gameObject.GetComponent<MeshRenderer>());
        StartColor = ObjectsToFade[0].material.color;
        StartCoroutine(CheckingToFade());
        FadeOut();
	}


    public void Select()
    {
        if (!associatedTrap.GetComponent<Trap>().unlocked)
        {
            purchaseConfirmation = true;
            StartCoroutine(TrapPurchaseConfirmation());
            confirmationGroup.setTrapCard(this);   
        }
    }

    public void Highlight()
    {
        if (!associatedTrap.GetComponent<Trap>().unlocked && !purchaseConfirmation)
        {
            StopCoroutine(Unhighlighting());
            StartCoroutine(Highlighting());
        }

    }

    public void OpenCanvas()
    {
        if (!canvasOpen && !purchaseConfirmation)
        {
            canvasOpen = true;
            trapCanvas.Open(transform.position);
        }
    }

    IEnumerator Highlighting()
    {
        float startTime = Time.time;
        while(Time.time < startTime + 0.3f)
        {
            for (int i = 0; i < ObjectsToFade.Count; i++)
            {
                ObjectsToFade[i].material.color = Color.Lerp(LockedColor, HighlightColor, (Time.time - startTime) / 0.3f);
            }
            yield return null;
        }
    }

    IEnumerator Unhighlighting()
    {
        float startTime = Time.time;
        while (Time.time < startTime + 0.3f)
        {
            for (int i = 0; i < ObjectsToFade.Count; i++)
            {
                ObjectsToFade[i].material.color = Color.Lerp(HighlightColor, LockedColor, (Time.time - startTime) / 0.3f);
            }
            yield return null;
        }
    }

    public void Purchase()
    {
        StopCoroutine(TrapPurchaseConfirmation());
        StartCoroutine(FadeToStartColor());
    }

    public void Cancel()
    {
        StopCoroutine(TrapPurchaseConfirmation());
        StartCoroutine(FadeToLockedColor());
    }

    IEnumerator FadeToLockedColor()
    {
        float startTime = Time.time;
        Color startColor = ObjectsToFade[0].material.color;

        while(Time.time < startTime + fadeTime)
        {
            for(int i = 0; i < ObjectsToFade.Count; i++)
            {
                ObjectsToFade[i].material.color = Color.Lerp(startColor, LockedColor, (Time.time - startTime) / fadeTime);
            }
            yield return null;
        }

        yield break;
    }

    IEnumerator FadeToStartColor()
    {
        Debug.Log("FadeToStartColor");
        float startTime = Time.time;
        Color startColor = ObjectsToFade[0].material.color;

        while (Time.time < startTime + fadeTime)
        {
            for (int i = 0; i < ObjectsToFade.Count; i++)
            {
                ObjectsToFade[i].material.color = Color.Lerp(startColor, StartColor, (Time.time - startTime) / fadeTime);
            }
            yield return null;
        }

        yield break;
    }

    IEnumerator TrapPurchaseConfirmation()
    {
        confirmationGroup.FadeIn(transform.position);
        float startTime = Time.time;
        Color startColor = ObjectsToFade[0].material.color;
        Color targetColor = LockedColor;
        bool flash = false;
        bool end = false;
        while(true)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Joystick1Button1))
            {
                end = true;
                startTime = Time.time;
                startColor = ObjectsToFade[0].material.color;
                targetColor = LockedColor;
                confirmationGroup.FadeOut();
            }

            for (int i = 0; i < ObjectsToFade.Count; i++)
            {
                ObjectsToFade[i].material.color = Color.Lerp(startColor, targetColor, (Time.time - startTime) / 0.3f);
            }

            if(Time.time > startTime + flashInterval)
            {
                if (end)
                {
                    canvasOpen = false;
                    trapCanvas.Close();
                    purchaseConfirmation = false;
                    yield break;
                }

                startTime = Time.time;
                if (flash)
                {
                    flash = false;
                    targetColor = LockedColor;
                    startColor = HighlightColor;
                } 
                else if (!flash)
                {
                    flash = true;
                    targetColor = HighlightColor;
                    startColor = LockedColor;
                }
            }

            yield return null;
        }

        yield break;
    }


    public void Unhighlight()
    {
        if (!associatedTrap.GetComponent<Trap>().unlocked && !purchaseConfirmation)
        {
            StopCoroutine(Highlighting());
            StartCoroutine(Unhighlighting());
            CloseCanvas();
        }
    }

    public void CloseCanvas()
    {
        if (canvasOpen)
        {
            canvasOpen = false;
            trapCanvas.Close();
        }
    }

    public void FadeOut()
    {
        if (visible)
        {
            visible = false;
            StopCoroutine(FadingIn());
            StartCoroutine(FadingOut());
            GetComponent<BoxCollider>().enabled = false;
        }
        
    }
    
    IEnumerator CheckingToFade()
    {
        yield return new WaitForSeconds(1f);
        while (true)
        {
            if (!trapCardSpawner.sliding)
            {
                

                float dist = Vector3.Distance(transform.position, CenterPoint.position);
                if (sliding)
                {
                    sliding = false;
                    if (dist > bounds)
                    {
                        for (int i = 0; i < ObjectsToFade.Count; i++)
                        {
                            //visible = false;
                            FadeOut();
                        }
                    }
                    else
                    {
                        for (int i = 0; i < ObjectsToFade.Count; i++)
                        {
                            GetComponent<BoxCollider>().enabled = true;
                            ObjectsToFade[i].material.color = StartColor;
                            visible = true;
                        }
                    }
                }

                if (!visible)
                {
                    if (dist < bounds)
                    {
                        Debug.Log("fadeIn");
                        FadeIn();
                    }
                }
                else if (!trapCardSpawner.snapping)
                {
                    if (dist > bounds)
                    {
                        Debug.Log("fadeOut");
                        FadeOut();
                    }
                }
            }
            else
            {
                if (!sliding)
                {
                    sliding = true;
                    visible = true;
                }
                Color lerpColor = Color.Lerp(InvisibleColor, StartColor, visibilityFalloff.Evaluate((1f - (Vector3.Distance(transform.position, CenterPoint.position) / slidingVisisbilityBounds))));
                for (int i = 0; i < ObjectsToFade.Count; i++)
                {
                    //ObjectsToFade[i].enabled = true;
                    ObjectsToFade[i].material.color = lerpColor;
                }
            }
                
            
            yield return new WaitForSeconds(checkFadeTime);

        }
    }

    IEnumerator FadingOut()
    {
        float startTime = Time.time;
        while (true)
        {
            if (Time.time <= startTime + fadeTime)
            {
                Color _startColor = ObjectsToFade[0].material.color;
                for (int i = 0; i < ObjectsToFade.Count; i++)
                {
                    ObjectsToFade[i].material.color = Color.Lerp(_startColor, InvisibleColor, (Time.time - startTime) / fadeTime);
                }
            }
            else
            {
                visible = false;
                for (int i = 0; i < ObjectsToFade.Count; i++)
                {
                    //ObjectsToFade[i].enabled = false;
                }
                yield break;
            }
            yield return null;
        }
    }

    public void FadeIn()
    {
        if (!visible)
        {
            visible = true;
            for (int i = 0; i < ObjectsToFade.Count; i++)
            {
                //ObjectsToFade[i].enabled = true;
            }
            StopCoroutine(FadingOut());
            StartCoroutine(FadingIn());
            GetComponent<BoxCollider>().enabled = true;
        }
    }

    IEnumerator FadingIn()
    {
        float startTime = Time.time;
        while (true)
        {
            if(Time.time <= startTime + fadeTime)
            {
                Color _startColor = ObjectsToFade[0].material.color;
                for (int i = 0; i < ObjectsToFade.Count; i++)
                {
                    ObjectsToFade[i].material.color = Color.Lerp(_startColor, StartColor, (Time.time - startTime) / fadeTime);
                }
            }
            else
            {
                yield break;
            }
            yield return null;
        }
    }


	// This is only needed to be done once
	void Awake()
	{
		if(trapRadials == null)
			trapRadials = GameObject.FindObjectsOfType<EquipTrapRadial>();
	}
	
    public void LoadTrapInSlot(GameObject trap)
    {
		GameObject _trap = (GameObject)Instantiate(trap.GetComponent<Trap>().icon, trapIconPos.position, Quaternion.identity, trapIconPos);
        MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>();
        foreach(MeshRenderer mr in mrs)
        {
            ObjectsToFade.Add(mr);
            if (!trap.GetComponent<Trap>().unlocked)
            {
                mr.material.color = LockedColor;
                StartColor = LockedColor;
            }
        }
        associatedTrap = trap.GetComponent<Trap>().trapPrefab;
	  }
		
	// Update is called once per frame
	void Update () {

	}

	// Checks if any of the trapRadials are selected. If so, puts this trap onto there, and sets this trap as equipped.
	public void EquipTrap()
	{
		if(equipped == false)
		{
			EquipTrapRadial[] trapRadials = GameObject.FindObjectsOfType<EquipTrapRadial>();
			for(int i = 0; i < trapRadials.Length; i++)
			{
				if(trapRadials[i].isSelected == true)
				{
					// If there's already a trap in this spot, this current one get's "deequipped"
					if(trapRadials[i].associatedTrapCard != null)
					{
						trapRadials[i].RemoveTrap();
					}
					trapRadials[i].SetTrap(this);
					equipped = true;
					break;
				}
			}
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapCardSpawner : MonoBehaviour {


    public GameObject TrapCard;
    public float TrapCardSpacing = 0.1f;
    public GameObject[] Traps;
    public PageScript TrapPage;
    private List<TrapCard> TrapCards = new List<TrapCard>();
    private bool trapCardsOpen;
    private Transform Slider;
    private Vector3 sliderStartPos;
    public float SlideDistancePerSecond;
    public bool sliding;
    public float snapDelay;
    public float snapSpeed = 3f;
    public Transform Center;
    public bool snapping;
    public float bounds;
    private float minX;
    private float maxX;
    private bool canSlide;
	public EquipTrapRadial[] trapSlots;			// A refrence to the trap slots that store the traps being equipped.
    public TrapCanvasScript trapCanvasScript;
	private PlayerState player;
    public ConfirmationGroup confirmationGroup;


	// Use this for initialization
	void Start ()
    {
        Slider = transform.FindChild("TrapCardSlider");
        sliderStartPos = Slider.position;
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerState>();
		for(int i = 0; i < Traps.Length; i++)
        {
  
            float width = TrapCard.transform.lossyScale.x + TrapCardSpacing;
            SpawnTrapCard(transform.position + (Vector3.left * i * width), Traps[i]);
           
        }

       
        canSlide = true;
      
    }
	

    IEnumerator SlidingRight(bool left = false)
    {
        if (confirmationGroup.confirming)
        {
            yield break;
        }
        sliding = true;
        while (true)
        {
            if (sliding && canSlide)
            {
                if(left == false)
                {
                    Slider.Translate(Vector3.right * SlideDistancePerSecond * Time.deltaTime);
                }
                else
                {
                    Slider.Translate(Vector3.left * SlideDistancePerSecond * Time.deltaTime);
                }

                if(Slider.transform.localPosition.x < -1.6 || Slider.transform.localPosition.x > (TrapCard.transform.lossyScale.x + TrapCardSpacing) * (TrapCards.Count - 3))
                {
                    canSlide = false;
                }
                else
                {
                    canSlide = true;
                }

            }
            else
            {
                sliding = false;
                break;
            }
            yield return null;
        }

        yield return new WaitForSeconds(snapDelay);

        if (sliding)
        {
            yield break;
        }
        else
        {
            //snapToCenter
            float minDist = 100;
            TrapCard closestCard = null;
            for (int i = 0; i < TrapCards.Count; i++)
            {
                float distance = Mathf.Abs(Vector3.Distance(Center.position, TrapCards[i].transform.position));
                if(distance < minDist && i != 0 && i != TrapCards.Count-1)
                {
                    minDist = distance;
                    closestCard = TrapCards[i];
                }

            }

            Vector3 distanceToMove = closestCard.transform.position - Center.position;
            distanceToMove.Set(distanceToMove.x, 0, 0);
            Vector3 snapPos = Slider.position - distanceToMove;

            while (true)
            {
                //Snapping
                snapping = true;

                if (!sliding)
                {
                    if (Mathf.Abs(Slider.transform.position.x - snapPos.x) >= 0.01f)
                    {
                        Slider.transform.position = Vector3.Lerp(Slider.transform.position, snapPos, Time.fixedDeltaTime * snapSpeed);
                    }
                    else
                    {
                        Slider.position = snapPos;
                        snapping = false;
                        canSlide = true;
                        yield break;
                    }
                }
                else
                {
                    Slider.position = snapPos;
                    canSlide = true;
                    snapping = false;
                    yield break;
                }
                yield return null;
            }


        }


    }

    public void StartSlidingRight(bool left = false)
    {
        if (!sliding && canSlide)
        {
            if (left)
            {
                Debug.Log("slideRight");
                StartCoroutine(SlidingRight(left = true));
            }
            else
            {
                StartCoroutine(SlidingRight(left = false));
            }
        }
    }

    public void StopSlidingRight(bool left = false)
    {
        sliding = false;
    }

    public void OpenTrapCards()
    {
        Slider.transform.position = sliderStartPos;
        for (int i = 0; i < 3; i++)
        {
            TrapCards[i].FadeIn();

        }
    }

    public void CloseTrapCards()
    {
        for (int i = 0; i < 3; i++)
        {
            if (TrapCards[i].visible)
            {
                TrapCards[i].FadeOut();
            }
        }
    }

	// Loads the specified trap into a TrapCard.
	void SpawnTrapCard (Vector3 pos, GameObject trap)
    {
        GameObject _trapCard = (GameObject)Instantiate(TrapCard, pos, transform.rotation, Slider);
        TrapCard thisTrapCard = _trapCard.GetComponent<TrapCard>();
	thisTrapCard.LoadTrapInSlot(trap);
        TrapCards.Add(thisTrapCard);
        thisTrapCard.trapCardSpawner = this;
        thisTrapCard.CenterPoint = Center;
        thisTrapCard.trapCanvas = trapCanvasScript;
        thisTrapCard.confirmationGroup = confirmationGroup;
        // Checks if the player has equipped this trap already. If so, it sets this card to the respective trapRadial spot.
        for(int i = 0; i < trapSlots.Length; i++)
        {
          int trapIndex = trapSlots[i].representWhichSpot;
          if(player.currEquippedTraps[trapIndex] != null && player.currEquippedTraps[trapIndex].name == thisTrapCard.associatedTrap.name)
          {
            thisTrapCard.equipped = true;
            trapSlots[i].associatedTrapCard = thisTrapCard;
            break;
          }
        }
    }
  
}

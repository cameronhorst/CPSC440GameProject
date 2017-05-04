using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringPadScript : MonoBehaviour {

    private Animator SpringPadAnims;

    public float LaunchForce;

    public GameObject Lights;
    private Material LightsMat;
    public Color LightMainColor;
    public Color LightEmissionColor;
    public Color whiteMainColor;
    public Color WhiteEmissionColor;
    private Color startMainColor;
    private Color startEmissionColor;

    public float flashInterval;
    public float springDelayTime;

    private bool triggered = false;
    private BoxCollider SpringPadCollider;
    public LayerMask SpringPadMask;
    private List<Rigidbody> rbsToLaunch = new List<Rigidbody>();
    private List<SmallAlienPhysicsManager> alienPhysicsManagers = new List<SmallAlienPhysicsManager>();
    private List<SmallAlienHealth> alienHealths = new List<SmallAlienHealth>();

    // Use this for initialization
	void Start ()
    {
        LightsMat = Lights.GetComponent<MeshRenderer>().material;
        SpringPadAnims = gameObject.GetComponent<Animator>();
        SpringPadCollider = gameObject.GetComponent<BoxCollider>();
        startEmissionColor = LightsMat.GetColor("_EmissionColor");
        startMainColor = LightsMat.color;

    }


    private void OnTriggerEnter(Collider other)
    {
        if(SpringPadMask == (SpringPadMask | (1 << other.gameObject.layer)))
        {
            rbsToLaunch.Add(other.transform.parent.GetComponent<Rigidbody>());
            alienPhysicsManagers.Add(other.transform.parent.GetComponent<SmallAlienPhysicsManager>());
            alienHealths.Add(other.transform.parent.GetComponent<SmallAlienHealth>());

            if (!triggered)
            {
                StartCoroutine(Triggered());
                triggered = true;
            }
        }
    }


    
    IEnumerator Triggered()
    {
        SpringPadAnims.SetTrigger("PressDown");
        float triggerTime = Time.time;
        bool LightOn = false;
        LightsMat.color = whiteMainColor;
        LightsMat.SetColor("_EmissionColor", WhiteEmissionColor);
        float timer = 0f;

        while(Time.time < triggerTime + springDelayTime)
        {
            timer += 1f * Time.deltaTime;
            if(timer > flashInterval)
            {
                timer = 0;
                if (LightOn)
                {
                    LightOn = false;
                    LightsMat.color = startMainColor;
                    LightsMat.SetColor("_EmissionColor", startEmissionColor);
                }
                else
                {
                    LightOn = true;
                    LightsMat.color = whiteMainColor;
                    LightsMat.SetColor("_EmissionColor", WhiteEmissionColor);
                }
            }
            yield return null;
        }

        LightOn = true;
        LightsMat.color = LightMainColor;
        LightsMat.SetColor("_EmissionColor", LightEmissionColor);
        SpringPadAnims.SetTrigger("SpringUp");
        SpringPadCollider.enabled = false;
        for (int i = 0; i < rbsToLaunch.Count; i++)
        {
            alienPhysicsManagers[i].InAir();
            rbsToLaunch[i].velocity = Vector3.up * LaunchForce;
            alienHealths[i].dealDamage(40);
        }

        yield break;
    }


	// Update is called once per frame
	void Update () {
		
	}
}

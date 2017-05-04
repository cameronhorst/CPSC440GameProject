using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonScript : MonoBehaviour {

    public int hitsToTurn;
    public float turnTime;
    public Transform moon;
    public int turnNum = 0;
    private bool showingFace = false;
    public Animator moonAnims;


	// Use this for initialization
	void Start ()
    {
		
	}



    

    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.layer == 8 && !showingFace)
        {
            showingFace = true;
            moonAnims.SetBool("Turn", true);
        }
    }




    // Update is called once per frame
    void Update () {
		
	}
}

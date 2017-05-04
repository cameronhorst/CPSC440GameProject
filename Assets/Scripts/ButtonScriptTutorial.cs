using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScriptTutorial : MonoBehaviour {

    public CanvasScript demoCanvas;
    public bool back;

	// Use this for initialization
	void Start () {
		
	}


    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == 8)
        {
            if (back)
            {
                demoCanvas.PreviousPage();
            }
            else
            {
                demoCanvas.nextPage();
                
            }
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}

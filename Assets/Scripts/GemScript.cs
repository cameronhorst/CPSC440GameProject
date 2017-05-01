using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemScript : MonoBehaviour {

    public int minDropAmt = 30;
    public int maxDropAmt = 78;
    public int DropAmt;

    public GemGroupScript GemGroup;

	// Use this for initialization
	void Start ()
    {
        DropAmt = Random.Range(minDropAmt, maxDropAmt);
        gameObject.SetActive(false);
	}
	
    public void ScanGem()
    {
        GemGroup.collectGems();
        GemGroup.Unhighlight(true);
    }

    public void Highlight()
    {
        //if GemGroupScript > not highlighted ...  highlight()
        GemGroup.Highlight();
    }

    public void Unhighlight()
    {
        GemGroup.Unhighlight();
    }

	// Update is called once per frame
	void Update () {
		
	}
}

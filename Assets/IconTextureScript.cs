using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconTextureScript : MonoBehaviour {

    public Texture Icon;

	// Use this for initialization
	void Start ()
    {
        gameObject.GetComponent<MeshRenderer>().material.mainTexture = Icon; 	
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapCanvasScript : MonoBehaviour {

    Animator Anims;
    MeshRenderer mr;
    public Transform target;

	// Use this for initialization
	void Start ()
    {
        Anims = gameObject.GetComponent<Animator>();
        mr = gameObject.GetComponentInChildren<MeshRenderer>();
        mr.enabled = false;
	}
	
    public void Open(Vector3 cardPosition)
    {

        cardPosition.Set(cardPosition.x, transform.position.y, transform.position.z);
        transform.position = cardPosition;
        Vector3 dir = target.position - transform.position;
        dir.Set(dir.x, 0, dir.z);
        transform.LookAt(dir);
        mr.enabled = true;
        Anims.SetBool("Open", true);
    }

    public void Close()
    {
        Anims.SetBool("Open", false);
        Invoke("turnOffRenderer", 1.25f);
    }

    void turnOffRenderer()
    {
        mr.enabled = false;
    }

	// Update is called once per frame
	void Update () {
		
	}
}

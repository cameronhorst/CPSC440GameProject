﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableRock : MonoBehaviour {

    public GameObject OuterShell;
    private Material OuterShellMat;
    public GameObject[] Chunks;
    public Color hitEmmisiveColor;
    private Color startEmmisiveColor;
    public float hitEmmisiveFadeTime = 0.35f;
    public float minNumGems;
    public float maxNumGems;
    public GameObject[] Gems;
    public float gemSpawnRadius = 0.75f;
    public float gemSpawnHeighOffset = 0.3f;
    public float gemSpawnDelay = 1;
    public float chunkDeathDelay = 2f;


    private bool fading;
    private int hitTimes = 0;
    public int hitsToBreak = 3;
    public float breakForce = 2f;
    public Color[] hitColors;
    private bool broken;

    // Use this for initialization
    void Start()
    {
        OuterShellMat = OuterShell.GetComponent<MeshRenderer>().material;
        startEmmisiveColor = OuterShellMat.GetColor("_Emission");

        hitColors = new Color[hitsToBreak-1];
        for(int i = 0; i < hitColors.Length; i++)
        {
            hitColors[i] = Color.Lerp(startEmmisiveColor, hitEmmisiveColor, i / (float)hitsToBreak);
        }

        foreach (GameObject chunk in Chunks)
        {
            chunk.GetComponent<MeshRenderer>().material.SetColor("_Emission", hitColors[1]);
        }

    }





    public void BulletImpact(Collision col)
    {
        Debug.Log("hit");
        hitTimes++;
        if (hitTimes <= hitColors.Length && !broken)
        { 
            StopAllCoroutines();
            StartCoroutine(HitFade());
        }
        
        if (hitTimes == hitsToBreak)
        {
            Debug.Log("Break");
            broken = true;
            StopAllCoroutines();
            StartCoroutine(Break(col.contacts[0].point));
        }
    }

    IEnumerator Break(Vector3 impactPos)
    {
        Destroy(OuterShell);
        List<Rigidbody> ChunkRBs = new List<Rigidbody>();

        foreach(GameObject Chunk in Chunks)
        {
            Chunk.SetActive(true);
            ChunkRBs.Add(Chunk.GetComponent<Rigidbody>());
        }

        //yield return new WaitForSeconds(0.05f);

        foreach (Rigidbody rb in ChunkRBs)
        {
            Vector3 force = rb.transform.position - impactPos;
            force.Set(force.x, 0, force.z);
            force = force + ((Vector3.up * force.magnitude) * 0.55f);
            rb.velocity = force * breakForce;
        }


        yield return new WaitForSeconds(chunkDeathDelay);

        foreach(GameObject Chunk in Chunks)
        {
            Destroy(Chunk);
        }
    }

    IEnumerator HitFade()
    {
        int _hittimes = hitTimes-1;
        float startFadeTime = Time.time;
        Color startColor = OuterShellMat.GetColor("_Emission");
        bool fadingToHit = true;
        while (true)
        {
            if (fadingToHit)
            {
                if (Time.time <= startFadeTime + hitEmmisiveFadeTime)
                {
                    OuterShellMat.SetColor("_Emission", Color.Lerp(startColor, hitEmmisiveColor, (Time.time - startFadeTime) / hitEmmisiveFadeTime));
                }
                else
                {
                    fadingToHit = false;
                    startFadeTime = Time.time;
                }
            }
            else if (!fadingToHit)
            {
                if(Time.time <= startFadeTime + hitEmmisiveFadeTime)
                {
                    OuterShellMat.SetColor("_Emission", Color.Lerp(hitEmmisiveColor, hitColors[_hittimes], (Time.time - startFadeTime) / hitEmmisiveFadeTime));
                }
                else
                {
                    yield break;
                }
            }
            yield return null;
        }
    }


    
	
	// Update is called once per frame
	void Update () {
		
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour {

    public int MaxNumTraps = 10;
    public int StartingNumTraps = 20;
    public int CurrentNumTraps;
    public Text NumberOfTraps;
    public string MaterialName = "Triphosphite";
    public string MaterialUnits = "g";
    public float collectedMaterial;
    public float startCollectedMaterial = 0;
    public float scansToFinishLevel = 20;
    public float currentNumberOfScans = 0;
    public GameObject ScanProgressBar;
    private Material ScanProgressBarMat;
    public float barUpdateTime;

    public void ScanData(bool gems = false)
    {
        currentNumberOfScans++;
        if(gems == true)
        {
            currentNumberOfScans++;
        }

        if(currentNumberOfScans <= scansToFinishLevel)
        {
            StopCoroutine(UpdateProgressBar());
            if (gems)
            {
                StartCoroutine(UpdateProgressBar(true));
            }
            else
            {
                StartCoroutine(UpdateProgressBar());
            }
        }
    }

    IEnumerator UpdateProgressBar(bool gems = false)
    {
        float startTime = Time.time;
        while (Time.time < startTime + barUpdateTime)
        {
            float offset;
            if (gems)
            {
                offset = Mathf.Lerp((0.6f - (((currentNumberOfScans - 2) / scansToFinishLevel) * 0.6f)), (0.6f - ((currentNumberOfScans / scansToFinishLevel) * 0.6f)), (Time.time - startTime) / barUpdateTime);
            }
            else
            {
                offset = Mathf.Lerp((0.6f - (((currentNumberOfScans - 1) / scansToFinishLevel) * 0.6f)), (0.6f - ((currentNumberOfScans / scansToFinishLevel) * 0.6f)), (Time.time - startTime) / barUpdateTime);
            }

            ScanProgressBarMat.SetTextureOffset("_MainTex", new Vector2(offset, 0));
            yield return null;
        }
    }




    // Use this for initialization
    void Start ()
    {
        CurrentNumTraps = StartingNumTraps;
        NumberOfTraps.text = CurrentNumTraps.ToString();
        collectedMaterial = startCollectedMaterial;
        ScanProgressBarMat = ScanProgressBar.GetComponent<MeshRenderer>().material;
    }

    public void IncrementNumTraps(bool Increment = true)
    {
        if (!Increment)
        {
            CurrentNumTraps--;
            NumberOfTraps.text = CurrentNumTraps.ToString();
        }
        else
        {
            CurrentNumTraps++;
            NumberOfTraps.text = CurrentNumTraps.ToString();
        }
    }




	// Update is called once per frame
	void Update () {
		
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmationGroup : MonoBehaviour
{

    MeshRenderer[] mrs;
    public float fadeTime;
    Color[] startColors;
    public Color invisibleColor;

	// Use this for initialization
	void Start ()
    {
        mrs = gameObject.GetComponentsInChildren<MeshRenderer>();
        startColors = new Color[mrs.Length];
        for(int i = 0; i < mrs.Length; i++)
        {
            startColors[i] = mrs[i].material.color;
            mrs[i].material.color = invisibleColor;
        }
	}


    public void FadeIn(Vector3 cardPosition)
    {
        cardPosition.Set(cardPosition.x, transform.position.y, transform.position.z);
        transform.position = cardPosition;

        StartCoroutine(FadingIn());

    }


    public void FadeOut()
    {
        StartCoroutine(FadingOut());
    }


    IEnumerator FadingIn()
    {
        float startTime = Time.time;

        while(Time.time < startTime + fadeTime)
        {
            for(int i = 0; i<mrs.Length; i++)
            {
                mrs[i].material.color = Color.Lerp(invisibleColor, startColors[i], (Time.time - startTime) / fadeTime);
            }
            yield return null;
        }


        yield break;
    }

    IEnumerator FadingOut()
    {
        float startTime = Time.time;

        while (Time.time < startTime + fadeTime)
        {
            for (int i = 0; i < mrs.Length; i++)
            {
                mrs[i].material.color = Color.Lerp(startColors[i], invisibleColor, (Time.time - startTime) / fadeTime);
            }
            yield return null;
        }


        yield break;
    }










    // Update is called once per frame
    void Update () {
		
	}
}

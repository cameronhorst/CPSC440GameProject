using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartIconScript : MonoBehaviour {

    public float minScale;
    private float startScale;
    public GameObject BreakingHeart;
    private Material BreakingHeartMat;
    private Material HeartMat;
    public float breakimageTime = .45f;
    public Texture[] breakimages;

	// Use this for initialization
	void Start ()
    {
        startScale = transform.localScale.x;
        HeartMat = gameObject.GetComponent<MeshRenderer>().material;
        BreakingHeartMat = BreakingHeart.GetComponent<MeshRenderer>().material;
        BreakingHeart.SetActive(false);
	}
	


    public void UpdateHeartSize(float heartSizeNormalized)
    {
        heartSizeNormalized = Mathf.Clamp(heartSizeNormalized, 0f, 1f);
        float scale = Mathf.Lerp(minScale, startScale, heartSizeNormalized);
        transform.localScale = new Vector3(scale, scale, scale);
    }

    public void HeartBreak()
    {
        GetComponent<MeshRenderer>().enabled = false;
        BreakingHeart.SetActive(true);
        StartCoroutine(BreakHeart());

    }


    private IEnumerator BreakHeart()
    {
        yield return new WaitForSeconds(1f);
        for(int i = 0; i < breakimages.Length; i++)
        {
            BreakingHeartMat.mainTexture = breakimages[i];
            yield return new WaitForSeconds(breakimageTime);
        }
    }





    // Update is called once per frame
    void Update ()
    {
		
	}
}

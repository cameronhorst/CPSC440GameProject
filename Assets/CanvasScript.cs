using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class CanvasScript : MonoBehaviour
{

    public Text[] TutorialText;

    int currentPage = 0;

    bool endOfTut = false;

    public string SceneName;

	// Use this for initialization
	void Start ()
    {
        //nextPage();
	}
	
    public void PreviousPage()
    {
        currentPage--;
        if(currentPage < 0)
        {
            currentPage = 0;
        }
        TutorialText[currentPage + 1].gameObject.SetActive(false);
        TutorialText[currentPage].gameObject.SetActive(true);
    }

    public void nextPage()
    {
        if (!endOfTut)
        {
            currentPage++;
            if (currentPage == 0)
            {
                TutorialText[0].gameObject.SetActive(true);
            }
            if (currentPage < TutorialText.Length)
            {
                TutorialText[currentPage - 1].gameObject.SetActive(false);
                TutorialText[currentPage].gameObject.SetActive(true);
            }
            else if (!endOfTut)
            {
                endOfTut = true;
                Invoke("LoadLevel1", 1);
            }
        }
        

    }


    void LoadLevel1()
    {
        SceneManager.LoadScene(SceneName);
    }

	// Update is called once per frame
	void Update () {
		
	}
}

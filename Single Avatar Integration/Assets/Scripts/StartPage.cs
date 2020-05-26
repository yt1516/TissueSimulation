using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartPage : MonoBehaviour
{
    Text text;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
        StartBlinking();
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            SceneManager.LoadScene("wmm");
        }
    }



    IEnumerator Blink()
    {
        while (true)
        {
            text.text = "";
            yield return new WaitForSeconds(1f);
            text.text = "Press Any Key to Start";
            yield return new WaitForSeconds(1f);
        }

    }

    void StartBlinking()
    {
        StartCoroutine("Blink");
    }
    void StopBlinking()
    {
        StopCoroutine("Blink");
    }
}

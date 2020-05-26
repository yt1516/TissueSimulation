using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    GameObject[] pauseObjects;
    GameObject[] wmm;
    GameObject[] wmf;

    // Use this for initialization
    void Start()
    {
        Time.timeScale = 1;
        pauseObjects = GameObject.FindGameObjectsWithTag("ShowOnPause");
        hidePaused();
    }

    // Update is called once per frame
    void Update()
    {
        //uses the p button to pause and unpause the game
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (Time.timeScale == 1)
            {
                Time.timeScale = 0;
                showPaused();
            }
            else if (Time.timeScale == 0)
            {
                Debug.Log("high");
                Time.timeScale = 1;
                hidePaused();
            }
        }
    }


    //Reloads the Level
    public void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //controls the pausing of the scene
    public void pauseControl()
    {
        if (Time.timeScale == 1)
        {
            Time.timeScale = 0;
            showPaused();
        }
        else if (Time.timeScale == 0)
        {
            Time.timeScale = 1;
            hidePaused();
        }
    }

    //shows objects with ShowOnPause tag
    public void showPaused()
    {
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<MouseLook>().enabled = false;
        foreach (GameObject g in pauseObjects)
        {
            g.SetActive(true);
        }
    }

    //hides objects with ShowOnPause tag
    public void hidePaused()
    {
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<MouseLook>().enabled = true;
        foreach (GameObject g in pauseObjects)
        {
            g.SetActive(false);
        }
    }

    //change stuff
    public void SpringForce()
    {
        InputField input = GameObject.Find("SpringForce").GetComponent<InputField>();
        //GameObject.Find("face").GetComponent<AbdomenResponse>().LayerDistance= float.Parse(input.text);
        int CubeNumber = GameObject.Find("face").GetComponent<AbdomenResponse>().CubeNumber;
        string i_str;
        for (int i = 1; i <= CubeNumber; i++)
        {
            // Re-order the pre-generated cubes into layers
            if (i <= 9)
            {
                i_str = "00" + i.ToString();
            }
            else if (i >= 10 && i <= 99)
            {
                i_str = "0" + i.ToString();
            }
            else
            {
                i_str = i.ToString();
            }
            string cube_name = "Cube." + i_str;
            GameObject cube = GameObject.Find(cube_name);
            SpringJoint spring = cube.GetComponent<SpringJoint>();
            spring.spring= float.Parse(input.text);
        }
    }

    public void Damping()
    {
        InputField input = GameObject.Find("Damping").GetComponent<InputField>();
        //GameObject.Find("face").GetComponent<AbdomenResponse>().LayerDistance= float.Parse(input.text);
        int CubeNumber = GameObject.Find("face").GetComponent<AbdomenResponse>().CubeNumber;
        string i_str;
        for (int i = 1; i <= CubeNumber; i++)
        {
            // Re-order the pre-generated cubes into layers
            if (i <= 9)
            {
                i_str = "00" + i.ToString();
            }
            else if (i >= 10 && i <= 99)
            {
                i_str = "0" + i.ToString();
            }
            else
            {
                i_str = i.ToString();
            }
            string cube_name = "Cube." + i_str;
            GameObject cube = GameObject.Find(cube_name);
            SpringJoint spring = cube.GetComponent<SpringJoint>();
            spring.damper = float.Parse(input.text);
        }
    }

    public void Sensitivity()
    {
        InputField input = GameObject.Find("Sensitivity").GetComponent<InputField>();
        GameObject.Find("face").GetComponent<AbdomenResponse>().Sensitivity = float.Parse(input.text);
    }

    public void AvartarSelect()
    {
        
        Dropdown avatarlist = GameObject.Find("Avatars").GetComponent<Dropdown>();
        int idx = avatarlist.value;
        if (idx == 0)
        {
            SceneManager.LoadScene("wmm");
        }
        if (idx == 1)
        {
            SceneManager.LoadScene("wmf");
        }
        if (idx == 2)
        {
            SceneManager.LoadScene("aof");
        }
        if (idx == 3)
        {
            SceneManager.LoadScene("aym");
        }
    }
}

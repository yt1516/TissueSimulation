using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AbdomenResponse : MonoBehaviour
{
    public int CubeNumber;
    public float SpringConstant = 400;
    public float damping = 10;
    public float LayerDistance = 0.1f;
    public float Sensitivity = 0.01f;

    private float[] maximum;
    private int layer_num;

    // Start is called before the first frame update
    void Start()
    {
        initiate(CubeNumber);
        StartCoroutine(ExampleCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        float pain_total = 0;
        GameObject face = GameObject.Find("face");
        GameObject brow = GameObject.Find("brow");
        SkinnedMeshRenderer FacialExpression = face.GetComponent<SkinnedMeshRenderer>();
        SkinnedMeshRenderer BrowMovement = brow.GetComponent<SkinnedMeshRenderer>();
        GameObject[] objs;
        for (int h = 1; h <= layer_num; h++)
        {
            objs = GameObject.FindGameObjectsWithTag(h.ToString());
            float max_local = maximum[h-1];
            foreach (GameObject gameObject in objs)
            {
                float y_loc = gameObject.transform.position.y;
                float df = Mathf.Abs(Mathf.Abs(max_local) - Mathf.Abs(y_loc));
                
                gameObject.GetComponent<MeshRenderer>().material.color = GetRgbValues(0, 1.3f*LayerDistance, df);
                pain_total = pain_total + df;
            }
        }
        float pain_intensity = 100 * pain_total * Sensitivity/100;
        if (pain_intensity >= 100)
        {
            pain_intensity = 100f;
        }
        FacialExpression.SetBlendShapeWeight(0, pain_intensity);
        BrowMovement.SetBlendShapeWeight(0, pain_intensity);
    }

    public void initiate(int CubeNumber)
    {
        string i_str;
        double[] y_loc;
        y_loc = new double[CubeNumber];

        for (int i = 1; i <= CubeNumber; i++)
        {
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
            Rigidbody rigidbody = cube.AddComponent<Rigidbody>();
            rigidbody.mass = 0.1f;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

            cube.AddComponent<SpringJoint>();
            cube.AddComponent<BoxCollider>();
            float y_cur = cube.gameObject.transform.position.y;
            y_loc[i - 1] = System.Math.Round(y_cur, 4);
        }

        double[] y_dist = y_loc.Distinct().ToArray();

        System.Array.Sort(y_dist);

        layer_num = y_dist.Length;

        Debug.Log(layer_num.ToString());

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
            float y_cur = cube.gameObject.transform.position.y;
            double y_doub = System.Math.Round(y_cur, 4);
            for (int j = 1; j <= y_dist.Length; j++)
            {
                if (y_dist[j - 1] == y_doub)
                {
                    string c_name = j.ToString();
                    cube.tag = c_name;
                    if (j == 1)
                    {
                        Rigidbody rigidbody = cube.GetComponent<Rigidbody>();
                        rigidbody.isKinematic = true;
                    }
                }
            }
        }

        for (int i = 2; i <= layer_num; i++)
        {
            GameObject[] currentLayer = GameObject.FindGameObjectsWithTag(i.ToString());
            GameObject[] belowLayer = GameObject.FindGameObjectsWithTag((i - 1).ToString());
            foreach (GameObject currentObject in currentLayer)
            {
                foreach (GameObject belowObject in belowLayer)
                {
                    float currentX = currentObject.transform.position.x;
                    float currentZ = currentObject.transform.position.z;
                    double currentXest = System.Math.Round(currentX, 4);
                    double currentZest = System.Math.Round(currentZ, 4);
                    float belowX = belowObject.transform.position.x;
                    float belowZ = belowObject.transform.position.z;
                    double belowXest = System.Math.Round(belowX, 4);
                    double belowZest = System.Math.Round(belowZ, 4);

                    if (currentXest == belowXest && currentZest == belowZest)
                    {
                        Rigidbody below_body = belowObject.GetComponent<Rigidbody>();
                        SpringJoint spring = currentObject.GetComponent<SpringJoint>();
                        spring.connectedBody = below_body;
                        spring.anchor = new Vector3(1f, 0, 0);
                        spring.autoConfigureConnectedAnchor = false;
                        spring.connectedAnchor = new Vector3(-1f, 0, 0);
                        spring.spring = SpringConstant;
                        spring.damper = damping;
                        spring.minDistance = LayerDistance;
                        spring.maxDistance = LayerDistance;
                        spring.enableCollision = true;
                    }

                }
            }
        }
    }

    public IEnumerator ExampleCoroutine()
    {
        //Print the time of when the function is first called.
        Debug.Log("Started Coroutine at timestamp : " + Time.time);

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(5);

        //After we have waited 5 seconds print the time again.
        Debug.Log("Finished Coroutine at timestamp : " + Time.time);
        GameObject[] obj;

        maximum = new float[layer_num];

        for (int h = 1; h <= layer_num; h++)
        {
            string tag_name = h.ToString();
            obj = GameObject.FindGameObjectsWithTag(tag_name);
            maximum[h-1] = obj[1].transform.position.y;
        }


        foreach (var elements in maximum)
        {
            Debug.Log(elements.ToString());
        }
    }

    Color GetRgbValues(float minimum, float maximum, float value)
    {
        var normalizedValue = Normalize(minimum, maximum, value);

        float Blue = Distance(normalizedValue, 0);
        float Green = Distance(normalizedValue, 1);
        float Red = Distance(normalizedValue, 2);
        return new Vector4(Red, Green, Blue);
    }



    float Normalize(float minimum, float maximum, float value)
    {
        return (value - minimum) / (maximum - minimum) * 2;
    }

    int Distance(float value, float color)
    {
        var distance = Mathf.Abs(value - color);

        var colorStrength = 1 - distance;

        if (colorStrength < 0)
            colorStrength = 0;

        return (int)Mathf.Round(colorStrength * 255);
    }
}
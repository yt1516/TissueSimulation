using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AbdomenResponse : MonoBehaviour
{
    public int CubeNumber;
    public float SpringConstant = 400;
    public float damping = 5;
    public float LayerDistance = 0.1f;
    public float Sensitivity = 50f;
    public bool EnableColour = true;

    public Vector2 NoduleLocation = new Vector2(1f, 1f);

    private float[] maximum;
    private int layer_num;
    public Vector2 GridSize;
    double sensitivitycoef;
    private float threshold = 0;

    private double[] x_array;
    private double[] z_array;
    int x_pos;
    int z_pos;
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
        //Text gridsize = GameObject.Find("gridsize").GetComponent<Text>();
        //int x_size = (int)GridSize.x;
        //int y_size = (int)GridSize.y;
        //gridsize.text = y_size.ToString()+","+x_size.ToString();
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
                double x_rounded = System.Math.Round(gameObject.transform.position.x, 4);
                double z_rounded = System.Math.Round(gameObject.transform.position.z, 4);

                for (int i = 0; i < GridSize.x; i++)
                {
                    if (x_array[i] == x_rounded)
                    {
                        x_pos = i + 1;
                    }
                }

                for (int i = 0; i < GridSize.y; i++)
                {
                    if (z_array[i] == z_rounded)
                    {
                        z_pos = i + 1;
                    }
                }

                sensitivitycoef = Probability(x_pos, z_pos, NoduleLocation, Sensitivity);
                float y_loc = gameObject.transform.position.y;
                float df = Mathf.Abs(Mathf.Abs(max_local) - Mathf.Abs(y_loc));

                Vector4 newcolor = GetRgbValues(0, 2f * LayerDistance, df * (float)sensitivitycoef);

                if (EnableColour == true)
                {
                    gameObject.GetComponent<MeshRenderer>().material.color = newcolor;
                    if (x_rounded == x_array[(int)NoduleLocation.x - 1] && z_rounded == z_array[(int)NoduleLocation.y - 1])
                    {
                        gameObject.GetComponent<MeshRenderer>().material.color = new Vector4(255, 255, 255);
                    }

                }
                else
                {
                    gameObject.GetComponent<MeshRenderer>().material.color = new Vector4(255, 255, 255);
                }                
                pain_total = pain_total + df * (float)sensitivitycoef;
            }
        }
        float pain_intensity = 1.5f*(pain_total- threshold);
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
        GameObject last_cube = GameObject.Find("Cube." + CubeNumber.ToString());
        int x_num = 0;
        int z_num = 0;
        double x_prev = System.Math.Round(last_cube.transform.position.x, 4);
        double z_prev = System.Math.Round(last_cube.transform.position.z, 4);

        // LOOP 1: Add components and find layers
        for (int i = 1; i <= CubeNumber; i++)
        {
            // Iterate through all cubes by name
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

            // Add Rigidbody, spring joint and collider
            Rigidbody rigidbody = cube.AddComponent<Rigidbody>();
            rigidbody.mass = 0.1f;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            cube.AddComponent<SpringJoint>();
            cube.AddComponent<BoxCollider>();

            // Find number of layers 
            float y_cur = cube.gameObject.transform.position.y;
            y_loc[i - 1] = System.Math.Round(y_cur, 4);
        }

        // Print number of layers
        double[] y_dist = y_loc.Distinct().ToArray();

        System.Array.Sort(y_dist);

        layer_num = y_dist.Length;

        //Debug.Log(layer_num.ToString());

        // LOOP 2: Tag them by layer
        for (int i = 1; i <= CubeNumber; i++)
        {
            // Iterate through all cubes by name
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

            for (int j = 1; j <= layer_num; j++)
            {
                if (y_dist[j - 1] == y_doub)
                {
                    string c_name = j.ToString();
                    cube.tag = c_name;

                    // 1st layer cubes: get gridsize and fix them
                    if (j == 1)
                    {
                        if (System.Math.Round(cube.transform.position.z,4) == z_prev)
                        {
                            x_num = x_num + 1;
                        }
                        if (System.Math.Round(cube.transform.position.x,4) == x_prev)
                        {
                            z_num = z_num + 1;
                        }
                        Rigidbody rigidbody = cube.GetComponent<Rigidbody>();
                        rigidbody.isKinematic = true;
                    }
                }
            }
            GridSize.x = x_num;
            GridSize.y = z_num;
        }

        x_array = new double[x_num];
        z_array = new double[z_num];

        int x_count = 0;
        int z_count = 0;

        GameObject[] firstlayer = GameObject.FindGameObjectsWithTag("1");

        foreach (GameObject currentObject in firstlayer)
        {
            if (System.Math.Round(currentObject.transform.position.z, 4) == z_prev)
            {
                x_array[x_count]= System.Math.Round(currentObject.transform.position.x, 4);
                x_count = x_count + 1;
            }

            if (System.Math.Round(currentObject.transform.position.x, 4) == x_prev)
            {
                z_array[z_count] = System.Math.Round(currentObject.transform.position.z, 4);
                z_count = z_count + 1;
            }
        }

        System.Array.Sort(x_array);
        System.Array.Sort(z_array);

        //foreach (var elements in x_array)
        //{
        //    Debug.Log(elements.ToString());
        //}

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

        GameObject[] objs;

        for (int h = 1; h <= layer_num; h++)
        {
            objs = GameObject.FindGameObjectsWithTag(h.ToString());
            float max_local = maximum[h - 1];
            foreach (GameObject gameObject in objs)
            {
                sensitivitycoef = Probability(x_pos, z_pos, NoduleLocation, Sensitivity);
                float y_loc = gameObject.transform.position.y;
                float df = Mathf.Abs(Mathf.Abs(max_local) - Mathf.Abs(y_loc));
                threshold = threshold + df;
            }
        }
        Debug.Log(threshold.ToString());

        Text process = GameObject.Find("process").GetComponent<Text>();
        process.text = "Ready";
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

    static double Probability(float x1, float x2, Vector2 NoduleLocation, float sense)
    {
        float xVar = sense / 100 * (1 - 100) + 100;

        float x1Var = xVar;
        float x2Var = xVar;
        float x1X2Cov = 0f;
        float dx1 = x1 - NoduleLocation.x;
        float dx2 = x2 - NoduleLocation.y;
        float det = x1Var * x2Var - x1X2Cov * x1X2Cov;
        double coef = 1f / (2f * Mathf.PI * Mathf.Sqrt(det));
        float dist = (dx1 * dx1 * x2Var - 2f * dx1 * dx2 * x1X2Cov + dx2 * dx2 * x1Var) / det;
        double probability = 600 * coef * Mathf.Exp(-dist / 2f);

        return probability;
    }
}
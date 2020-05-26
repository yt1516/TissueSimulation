using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compression : MonoBehaviour
{
    // These can be modified at the GUI 
    public Vector2 gridSize;
    public float SpringForce;
    public float Damping;
    public float LayerDistance;
    public int LayerNumber;
    public Vector3 VoxelSize;
    public bool CompressionOnly;
    public bool CompressionWithSkin;
    public bool HomogenerousTissue;
    public Vector2 NoduleLocation;
    public int NoduleSize;
    public int StiffnessMultiplier;
    public int DampingMultiplier;

    // Declared before main loops
    private float y;
    private string x_label;
    private string y_label;
    private GameObject next_cube;
    private float[] matrix;
    private float[] maximum;

    Rigidbody rig;
    MeshRenderer col;
    SpringJoint spring;


    // Start is called before the first frame update
    void Start()
    {
        if (CompressionOnly == true)
        {
            simple_compression();
        }
        else if (CompressionWithSkin == true)
        {
            with_skin();
        }
        else if (HomogenerousTissue == true)
        {
            homo();
        }
        StartCoroutine(ExampleCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        GameObject[] objs;
        for (int h = 0; h < LayerNumber; h++)
        {
            string h_label = CreateLabel(h);
            objs = GameObject.FindGameObjectsWithTag(h_label);
            float max_local = maximum[h];
            foreach (GameObject gameObject in objs)
            {
                float y_loc = gameObject.transform.position.y;
                float df = max_local - y_loc;
                float pain_intensity = 100 * df / 0.15f;
                if (pain_intensity >= 100) {
                    pain_intensity = 100f;
                }
                gameObject.GetComponent<MeshRenderer>().material.color = GetRgbValues(0, 0.2f, df);
            }
        }
    }

    void simple_compression()
    {
        // Generate grid
        matrix = new float[LayerNumber];
        GameObject[][] gridofGameObjects;

        for (int h = 0; h < LayerNumber; h++)  // Generate Layer
        {
            string h_label = CreateLabel(h);
            gridofGameObjects = new GameObject[(int)gridSize.x][];

            for (int x = 0; x < gridSize.x; x++)    // Generate Row
            {
                gridofGameObjects[x] = new GameObject[(int)gridSize.y];

                for (int y = 0; y < gridSize.y; y++)    // Generate Column
                {

                    // Create a voxel and lable it by column+row index
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.transform.localScale = VoxelSize;
                    go.transform.position = new Vector3(x * (0.01f + VoxelSize.x), (h + 2) * VoxelSize.y, y * (0.01f + VoxelSize.z));
                    string x_label = CreateLabel(x);
                    string y_label = CreateLabel(y);
                    go.name = h_label + x_label + y_label;

                    // Add collision detection
                    GameObject collide = new GameObject(h_label + x_label + y_label + "c");
                    collide.transform.position = go.transform.position;
                    collide.AddComponent<BoxCollider>();
                    BoxCollider box = collide.GetComponent<BoxCollider>();
                    box.isTrigger = true;
                    box.size = VoxelSize;
                    collide.transform.parent = go.transform;

                    // Add rigid body parameters
                    go.AddComponent<Rigidbody>();
                    go.AddComponent<SpringJoint>();
                    rig = go.GetComponent<Rigidbody>();
                    rig.mass = 0.1f;
                    rig.useGravity = true;
                    rig.isKinematic = false;
                    rig.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

                    // Add spring joint parameters
                    spring = go.GetComponent<SpringJoint>();
                    spring.spring = SpringForce;
                    spring.damper = Damping;
                    spring.anchor = new Vector3(0, -0.5f, 0);
                    spring.autoConfigureConnectedAnchor = false;

                    if (h == 0)     // If bottom layer, anchor it to ground reference (vertical axis = 0)
                    {
                        spring.connectedAnchor = new Vector3(x * (0.01f + VoxelSize.x), 0, y * (0.01f + VoxelSize.z));
                    }
                    else
                    {      // Otherwise anchor it to the voxel below it
                        string h_below = CreateLabel(h - 1);
                        GameObject go_below = GameObject.Find(h_below + x_label + y_label);
                        Rigidbody go_below_rig = go_below.GetComponent<Rigidbody>();
                        spring.connectedBody = go_below_rig;
                        spring.connectedAnchor = new Vector3(0, 0.5f, 0);
                    }
                    spring.minDistance = LayerDistance;    // minDist and maxDist are same to have compression spring behaviour
                    spring.maxDistance = LayerDistance;
                    spring.enableCollision = true;

                    // Add colour for rendering
                    col = go.GetComponent<MeshRenderer>();
                    col.material.color = Color.white;
                    col.receiveShadows = false;
                    col.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    go.tag = h_label;
                    gridofGameObjects[x][y] = go;
                }
            }

            int nod_x = Mathf.RoundToInt(NoduleLocation.x);
            int nod_y = Mathf.RoundToInt(NoduleLocation.y);

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    if (x == nod_x && y == nod_y)
                    {
                        for (int i = -NoduleSize; i <= NoduleSize; i++) {
                            for (int j = -NoduleSize; j <= NoduleSize; j++)
                            {
                                GameObject go = gridofGameObjects[x+i][y+j];
                                spring = go.GetComponent<SpringJoint>();
                                spring.spring = SpringForce * StiffnessMultiplier;
                                spring.damper = Damping * DampingMultiplier;
                            }
                        }
                    }
                }
            }
        }
    }

    void with_skin()
    {
        // Generate grid
        matrix = new float[LayerNumber];
        GameObject[][] gridofGameObjects;

        for (int h = 0; h < LayerNumber; h++)  // Generate Layer
        {
            string h_label = CreateLabel(h);
            gridofGameObjects = new GameObject[(int)gridSize.x][];

            for (int x = 0; x < gridSize.x; x++)    // Generate Row
            {
                gridofGameObjects[x] = new GameObject[(int)gridSize.y];

                for (int y = 0; y < gridSize.y; y++)    // Generate Column
                {
                    // Create a voxel and lable it by column+row index
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.transform.localScale = VoxelSize;
                    go.transform.position = new Vector3(x * (0.01f + VoxelSize.x), (h + 2) * VoxelSize.y, y * (0.01f + VoxelSize.z));
                    string x_label = CreateLabel(x);
                    string y_label = CreateLabel(y);
                    go.name = h_label + x_label + y_label;

                    // Add collision detection
                    GameObject collide = new GameObject(h_label + x_label + y_label + "c");
                    collide.transform.position = go.transform.position;
                    collide.AddComponent<BoxCollider>();
                    BoxCollider box = collide.GetComponent<BoxCollider>();
                    box.isTrigger = true;
                    box.size = VoxelSize;
                    collide.transform.parent = go.transform;

                    // Add rigid body parameters
                    go.AddComponent<Rigidbody>();
                    go.AddComponent<SpringJoint>();
                    rig = go.GetComponent<Rigidbody>();
                    rig.mass = 0.1f;
                    rig.useGravity = true;
                    rig.isKinematic = false;
                    rig.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

                    // Add spring joint parameters
                    spring = go.GetComponent<SpringJoint>();
                    spring.spring = SpringForce;
                    spring.damper = Damping;
                    spring.anchor = new Vector3(0, -0.5f, 0);
                    spring.autoConfigureConnectedAnchor = false;
                    if (h == 0)     // If bottom layer, anchor it to ground reference (vertical axis = 0)
                    {
                        spring.connectedAnchor = new Vector3(x * (0.01f + VoxelSize.x), 0, y * (0.01f + VoxelSize.z));
                    }
                    else
                    {      // Otherwise anchor it to the voxel below it
                        string h_below = CreateLabel(h - 1);
                        GameObject go_below = GameObject.Find(h_below + x_label + y_label);
                        Rigidbody go_below_rig = go_below.GetComponent<Rigidbody>();
                        spring.connectedBody = go_below_rig;
                        spring.connectedAnchor = new Vector3(0, 0.5f, 0);
                    }
                    spring.minDistance = LayerDistance;    // minDist and maxDist are same to have compression spring behaviour
                    spring.maxDistance = LayerDistance;
                    spring.enableCollision = true;

                    // Add colour for rendering
                    col = go.GetComponent<MeshRenderer>();
                    col.material.color = Color.white;
                    col.receiveShadows = false;
                    col.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    go.tag = h_label;
                    gridofGameObjects[x][y] = go;
                }
            }

            int nod_x = Mathf.RoundToInt(NoduleLocation.x);
            int nod_y = Mathf.RoundToInt(NoduleLocation.y);

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    if (x == nod_x && y == nod_y)
                    {
                        for (int i = -NoduleSize; i <= NoduleSize; i++)
                        {
                            for (int j = -NoduleSize; j <= NoduleSize; j++)
                            {
                                GameObject go = gridofGameObjects[x + i][y + j];
                                spring = go.GetComponent<SpringJoint>();
                                spring.spring = SpringForce * StiffnessMultiplier;
                                spring.damper = Damping * DampingMultiplier;
                            }
                        }
                    }
                }
            }

            // Use top layer as skin. Hinge joints connect adjacent voxels
            if (h == LayerNumber - 1)
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    for (int y = 0; y < gridSize.y; y++)
                    {
                        GameObject go = gridofGameObjects[x][y];
                        if (x == 0 & y == 0 | x == gridSize.x - 1 & y == 0 | x == gridSize.x - 1 & y == gridSize.y - 1 | y == gridSize.y - 1 & x == 0)
                        {
                            Rigidbody rigidbody = go.GetComponent<Rigidbody>();
                        }

                        if (x <= gridSize.x - 2 & y <= gridSize.y - 2)
                        {
                            GameObject x1 = gridofGameObjects[x + 1][y];
                            GameObject y1 = gridofGameObjects[x][y + 1];
                            Rigidbody x1_rig = x1.GetComponent<Rigidbody>();
                            Rigidbody y1_rig = y1.GetComponent<Rigidbody>();
                            HingeJoint hingeJoint_x = go.AddComponent<HingeJoint>();
                            hingeJoint_x.connectedBody = x1_rig;
                            hingeJoint_x.anchor = new Vector3(0.5f + 0.00001f / 2, 0.5f + 0.00001f / 2, 0);
                            hingeJoint_x.axis = new Vector3(0, 0, 1f);
                            hingeJoint_x.enableCollision = true;
                            HingeJoint hingeJoint_y = go.AddComponent<HingeJoint>();
                            hingeJoint_y.connectedBody = y1_rig;
                            hingeJoint_y.anchor = new Vector3(0, 0.5f + 0.00001f / 2, 0.5f + 0.00001f / 2);
                            hingeJoint_y.enableCollision = true;
                        }

                        if (x == gridSize.x - 1 & y <= gridSize.y - 2)
                        {
                            GameObject y1 = gridofGameObjects[x][y + 1];
                            Rigidbody y1_rig = y1.GetComponent<Rigidbody>();
                            HingeJoint hingeJoint_y = go.AddComponent<HingeJoint>();
                            hingeJoint_y.connectedBody = y1_rig;
                            hingeJoint_y.anchor = new Vector3(0, 0.5f + 0.00001f / 2, 0.5f + 0.00001f / 2);
                            hingeJoint_y.enableCollision = true;
                        }
                        if (x <= gridSize.x - 2 & y == gridSize.y - 1)
                        {
                            GameObject x1 = gridofGameObjects[x + 1][y];
                            Rigidbody x1_rig = x1.GetComponent<Rigidbody>();
                            HingeJoint hingeJoint_x = go.AddComponent<HingeJoint>();
                            hingeJoint_x.connectedBody = x1_rig;
                            hingeJoint_x.anchor = new Vector3(0.5f + 0.00001f / 2, 0.5f + 0.00001f / 2, 0);
                            hingeJoint_x.axis = new Vector3(0, 0, 1f);
                            hingeJoint_x.enableCollision = true;
                        }
                    }
                }
            }
        }
    }

    void homo()
    {
        // Generate grid
        matrix = new float[LayerNumber];
        GameObject[][] gridofGameObjects;

        for (int h = 0; h < LayerNumber; h++)  // Generate Layer
        {
            string h_label = CreateLabel(h);
            gridofGameObjects = new GameObject[(int)gridSize.x][];

            for (int x = 0; x < gridSize.x; x++)    // Generate Row
            {
                gridofGameObjects[x] = new GameObject[(int)gridSize.y];

                for (int y = 0; y < gridSize.y; y++)    // Generate Column
                {
                    // Create a voxel and lable it by column+row index
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.transform.localScale = VoxelSize;
                    go.transform.position = new Vector3(x * (LayerDistance + VoxelSize.x), (h + 2) * VoxelSize.y, y * (LayerDistance + VoxelSize.z));
                    string x_label = CreateLabel(x);
                    string y_label = CreateLabel(y);
                    go.name = h_label + x_label + y_label;

                    // Add collision detection
                    GameObject collide = new GameObject(h_label + x_label + y_label + "c");
                    collide.transform.position = go.transform.position;
                    collide.AddComponent<BoxCollider>();
                    BoxCollider box = collide.GetComponent<BoxCollider>();
                    box.isTrigger = true;
                    box.size = VoxelSize;
                    collide.transform.parent = go.transform;

                    // Add rigid body parameters
                    go.AddComponent<Rigidbody>();
                    go.AddComponent<SpringJoint>();
                    rig = go.GetComponent<Rigidbody>();
                    rig.mass = 0.1f;
                    rig.useGravity = true;
                    rig.isKinematic = false;
                    rig.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

                    // Add spring joint parameters
                    spring = go.GetComponent<SpringJoint>();
                    spring.spring = SpringForce;
                    spring.damper = Damping;
                    spring.anchor = new Vector3(0, -0.5f, 0);
                    spring.autoConfigureConnectedAnchor = false;
                    if (h == 0)     // If bottom layer, anchor it to ground reference (vertical axis = 0)
                    {
                        spring.connectedAnchor = new Vector3(x * (LayerDistance + VoxelSize.x), 0, y * (LayerDistance + VoxelSize.z));
                    }
                    else
                    {      // Otherwise anchor it to the voxel below it
                        string h_below = CreateLabel(h - 1);
                        GameObject go_below = GameObject.Find(h_below + x_label + y_label);
                        Rigidbody go_below_rig = go_below.GetComponent<Rigidbody>();
                        spring.connectedBody = go_below_rig;
                        spring.connectedAnchor = new Vector3(0, 0.5f, 0);
                    }
                    spring.minDistance = LayerDistance;    // minDist and maxDist are same to have compression spring behaviour
                    spring.maxDistance = LayerDistance;
                    spring.enableCollision = true;

                    // Add colour for rendering
                    col = go.GetComponent<MeshRenderer>();
                    col.material.color = Color.white;
                    col.receiveShadows = false;
                    col.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    go.tag = h_label;
                    gridofGameObjects[x][y] = go;
                }
            }
            int nod_x = Mathf.RoundToInt(NoduleLocation.x);
            int nod_y = Mathf.RoundToInt(NoduleLocation.y);

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    GameObject go = gridofGameObjects[x][y];
                    if (x == 0 & y == 0 | x == gridSize.x - 1 & y == 0 | x == gridSize.x - 1 & y == gridSize.y - 1 | y == gridSize.y - 1 & x == 0)
                    {
                        Rigidbody rigidbody = go.GetComponent<Rigidbody>();
                    }

                    if (x <= gridSize.x - 2 & y <= gridSize.y - 2)
                    {
                        GameObject x1 = gridofGameObjects[x + 1][y];
                        GameObject y1 = gridofGameObjects[x][y + 1];
                        Rigidbody x1_rig = x1.GetComponent<Rigidbody>();
                        Rigidbody y1_rig = y1.GetComponent<Rigidbody>();
                        SpringJoint springjoint_x = go.AddComponent<SpringJoint>();
                        springjoint_x.connectedBody = x1_rig;
                        springjoint_x.anchor = new Vector3(0.5f, 0, 0);
                        springjoint_x.axis = new Vector3(0, 0, 1f);
                        springjoint_x.autoConfigureConnectedAnchor = false;
                        springjoint_x.connectedAnchor = new Vector3(-0.5f, 0, 0);
                        springjoint_x.spring = SpringForce;
                        springjoint_x.damper = Damping;
                        springjoint_x.enableCollision = true;
                        springjoint_x.minDistance = LayerDistance;
                        springjoint_x.maxDistance = LayerDistance;

                        SpringJoint springjoint_y = go.AddComponent<SpringJoint>();
                        springjoint_y.connectedBody = y1_rig;
                        springjoint_y.anchor = new Vector3(0, 0, 0.5f);
                        springjoint_y.autoConfigureConnectedAnchor = false;
                        springjoint_y.connectedAnchor = new Vector3(0, 0, -0.5f);
                        springjoint_y.spring = SpringForce;
                        springjoint_y.damper = Damping;
                        springjoint_y.enableCollision = true;
                        springjoint_y.minDistance = LayerDistance;
                        springjoint_y.maxDistance = LayerDistance;
                    }

                    if (x == gridSize.x - 1 & y <= gridSize.y - 2)
                    {
                        GameObject y1 = gridofGameObjects[x][y + 1];
                        Rigidbody y1_rig = y1.GetComponent<Rigidbody>();
                        SpringJoint springjoint_y = go.AddComponent<SpringJoint>();
                        springjoint_y.connectedBody = y1_rig;
                        springjoint_y.anchor = new Vector3(0, 0, 0.5f);
                        springjoint_y.autoConfigureConnectedAnchor = false;
                        springjoint_y.connectedAnchor = new Vector3(0, 0, -0.5f);
                        springjoint_y.spring = SpringForce;
                        springjoint_y.damper = Damping;
                        springjoint_y.enableCollision = true;
                        springjoint_y.minDistance = LayerDistance;
                        springjoint_y.maxDistance = LayerDistance;
                    }
                    if (x <= gridSize.x - 2 & y == gridSize.y - 1)
                    {
                        GameObject x1 = gridofGameObjects[x + 1][y];
                        Rigidbody x1_rig = x1.GetComponent<Rigidbody>();
                        SpringJoint springjoint_x = go.AddComponent<SpringJoint>();
                        springjoint_x.connectedBody = x1_rig;
                        springjoint_x.anchor = new Vector3(0.5f, 0, 0);
                        springjoint_x.axis = new Vector3(0, 0, 1f);
                        springjoint_x.autoConfigureConnectedAnchor = false;
                        springjoint_x.connectedAnchor = new Vector3(-0.5f, 0, 0);
                        springjoint_x.spring = SpringForce;
                        springjoint_x.damper = Damping;
                        springjoint_x.enableCollision = true;
                        springjoint_x.minDistance = LayerDistance;
                        springjoint_x.maxDistance = LayerDistance;
                    }
                }
            }
        }
    }

    IEnumerator ExampleCoroutine()
    {
        //Print the time of when the function is first called.
        Debug.Log("Started Coroutine at timestamp : " + Time.time);

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(5);

        //After we have waited 5 seconds print the time again.
        Debug.Log("Finished Coroutine at timestamp : " + Time.time);
        GameObject obj;

        maximum = new float[LayerNumber];

        for (int h = 0; h < LayerNumber; h++)
        {
            string h_label = CreateLabel(h);
            obj = GameObject.Find(h_label+"0000");
            maximum[h] = obj.transform.position.y;
        }

        
        foreach (var elements in maximum) {
            Debug.Log(elements.ToString());
        }
    }

    string CreateLabel(int num)
    {
        string num_label;
        if (num < 10)
        {
            num_label = "0" + num.ToString();
        }
        else
        {
            num_label = num.ToString();
        }

        return num_label;
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

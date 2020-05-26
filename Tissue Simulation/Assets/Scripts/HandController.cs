using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandController : MonoBehaviour
{
    public float speed = 5;
    private Rigidbody rb;

    void Start()
    {
        gameObject.AddComponent<Rigidbody>();
        rb = gameObject.GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

    }

    // Update is called once per frame
    void Update()
    {

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        float y = Input.GetAxisRaw("UpDown");
        gameObject.transform.position = new Vector3(transform.position.x + (x * speed), transform.position.y + (y * speed), transform.position.z + (z * speed));
    }

}

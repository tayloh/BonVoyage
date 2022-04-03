using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    //controls camera speed
    public float movementSpeed = 20f;

    //limiters to how far the camera can go in each direction
    //these values can be accessed via the inspector in unity
    public float limiter_x = 100f;
    public float limiter_z = 100f;

    //variable to handle scroll speed for zoom
    public float scrollSpeed = 2000f;
    public float minLimiter_y = 20f;
    public float maxLimiter_y = 120f;

    //Speed of rotation
    public float RotSpeed = 6f;


    // Update is called once per frame
    void Update()
    {
        //stores current coordinate as a variable
        Vector3 CamPos = transform.position;
        //takes inputs for all directions at once and then applies it all at the same time at the end
        if (Input.GetKey("w"))
        {
            CamPos.z += movementSpeed * Time.deltaTime;
        }

        if (Input.GetKey("s"))
        {
            CamPos.z -= movementSpeed * Time.deltaTime;
        }

        if (Input.GetKey("d"))
        {
            CamPos.x += movementSpeed * Time.deltaTime;
        }

        if (Input.GetKey("a"))
        {
            CamPos.x -= movementSpeed * Time.deltaTime;
        }

        //Handles zoom via mousescrolling by checking speed and direction of scroll, uses unitys built in input manager for the scroll variable
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        CamPos.y += scroll * scrollSpeed * Time.deltaTime;
        //zoom limiter
        CamPos.y = Mathf.Clamp(CamPos.y, minLimiter_y, maxLimiter_y);
        //makes sure that the values cant go beyond the limiters
        CamPos.x = Mathf.Clamp(CamPos.x, -limiter_x, limiter_x);
        CamPos.z = Mathf.Clamp(CamPos.z, -limiter_z, limiter_z);

        //applies all changes
        transform.position = CamPos;

        if (Input.GetMouseButton(1))
        {
            transform.eulerAngles += RotSpeed * new Vector3(-Input.GetAxis("Mouse Y"), 0, 0);
            //transform.eulerAngles += RotSpeed * new Vector3(0, Input.GetAxis("Mouse X"), 0);
            //transform.eulerAngles += RotSpeed * new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
        }
    }
}
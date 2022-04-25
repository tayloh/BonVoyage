using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    //controls camera speed
    public float MovementSpeed = 20f;
    public bool EnableTransitions = true;

    //limiters to how far the camera can go in each direction
    //these values can be accessed via the inspector in unity
    public float limiter_x = 100f;
    public float limiter_z = 100f;

    //variable to handle scroll speed for zoom
    public float scrollSpeed = 2000f;
    public float minLimiter_y = 20f;
    public float maxLimiter_y = 120f;

    //Speed of rotation
    public float RotSpeed = 2000f;

    public int RotMaxX = 90;
    public int RotMinX = 0;

    // Transition parameters
    public float TransitionSpeed = 2f;
    public Vector3 ShipCameraOffset = new Vector3(5, 10f, -5);

    private Vector3 _startPos = Vector3.zero;
    private Vector3 _currentLerpGoal = Vector3.zero;
    private Quaternion _startRotation = Quaternion.identity;
    private Quaternion _currentRotationGoal = Quaternion.identity;
    private float _tLerp = 0;
    private bool _isTransitioning = false;

    public void SmoothlyTransitionTo(Vector3 position, Vector3 lookAt)
    {
        _startPos = transform.position;
        _currentLerpGoal = position;
        _startRotation = transform.rotation;
        _currentRotationGoal = Quaternion.LookRotation(lookAt - position);

        _tLerp = 0;
        _isTransitioning = true;
    }

    private void _smoothTransition()
    {
        var distanceLeft = (transform.position - _currentLerpGoal).magnitude;
        var dynamicSpeedModifier = Mathf.Clamp(
            Mathf.Pow(distanceLeft, 1.5f), 
            0.05f, 2);

        var lerpStep = dynamicSpeedModifier * TransitionSpeed * Time.deltaTime;

        _tLerp += lerpStep;

        transform.position = Vector3.Lerp(_startPos, _currentLerpGoal, _tLerp);
        transform.rotation = Quaternion.Slerp(_startRotation, _currentRotationGoal, _tLerp);
        
        // Using smooth damp
        //var velocity = Vector3.zero;
        //var intermdiatePos = Vector3.SmoothDamp(_startPos, _currentLerpGoal, ref velocity, _tLerp, 0.2f); 
        //transform.position = intermediatePos;

        if (_tLerp >= 1)
        {
            _isTransitioning = false;
        }

    }

    // option = 0 => XZ constrained camera forward/back
    // option = 1 => XZ constrained camera left/right
    private Vector3 _getCameraMoveDirection(int option)
    {
        switch (option)
        {
            case 0:
                return new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
            case 1:
                return new Vector3(transform.right.x, 0, transform.right.z).normalized;
            default:
                throw new Exception($"Invalid option {option}");

        }
    }


    private void LateUpdate()
    {

        //if (Input.GetKey(KeyCode.J)) SmoothlyTransitionTo(new Vector3(5, 8, 5), new Vector3(5, 5, 5));

        if (_isTransitioning && EnableTransitions)
        {
            _smoothTransition();
        }

        ShipCameraOffset.y = transform.position.y;

        //stores current coordinate as a variable
        Vector3 CamPos = transform.position;
        //takes inputs for all directions at once and then applies it all at the same time at the end

        Vector3 resultingMoveDir = Vector3.zero;

        if (Input.GetKey("w"))
        {
            resultingMoveDir += _getCameraMoveDirection(0);
            //CamPos += _getCameraMoveDirection(0) * movementSpeed * Time.deltaTime;
        }
        else if (Input.GetKey("s"))
        {
            resultingMoveDir -= _getCameraMoveDirection(0);
            //CamPos -= _getCameraMoveDirection(0) * movementSpeed * Time.deltaTime;
        }

        if (Input.GetKey("d"))
        {
            resultingMoveDir += _getCameraMoveDirection(1);
            //CamPos += _getCameraMoveDirection(1) * movementSpeed * Time.deltaTime;
        }
        else if (Input.GetKey("a"))
        {
            resultingMoveDir -= _getCameraMoveDirection(1);
            //CamPos -= _getCameraMoveDirection(1) * movementSpeed * Time.deltaTime;
        }

        // Calculate new position
        resultingMoveDir = resultingMoveDir.normalized;
        CamPos += resultingMoveDir * MovementSpeed * Time.deltaTime * (CamPos.y + 2 - minLimiter_y) / 10; //modulate depending on zoom

        //Handles zoom via mousescrolling by checking speed and direction of scroll, uses unitys built in input manager for the scroll variable
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (CamPos.y > minLimiter_y && CamPos.y < maxLimiter_y)
        {
            CamPos += transform.forward * scroll * scrollSpeed * Time.deltaTime;
        }
        // If the camera is at the ceiling or the floor, move it only in y direction when scrolling
        else
        {
            CamPos.y += transform.forward.y * scroll * scrollSpeed * Time.deltaTime;
        }


        //zoom limiter
        CamPos.y = Mathf.Clamp(CamPos.y, minLimiter_y, maxLimiter_y);
        //makes sure that the values cant go beyond the limiters
        CamPos.x = Mathf.Clamp(CamPos.x, -limiter_x, limiter_x);
        CamPos.z = Mathf.Clamp(CamPos.z, -limiter_z, limiter_z);

        //applies all changes

        if (!_isTransitioning)
        {
            transform.position = CamPos;
        }

        if (Input.GetMouseButton(1) && !_isTransitioning)
        {
            //transform.eulerAngles += RotSpeed * new Vector3(-Input.GetAxis("Mouse Y"), 0, 0);
            //transform.eulerAngles += RotSpeed * new Vector3(0, Input.GetAxis("Mouse X"), 0);
            Vector3 currentEulerAngles = transform.eulerAngles;

            Vector3 eulerAnglesRotation = Time.deltaTime * RotSpeed * new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
            Vector3 resultingEulerAngles = currentEulerAngles + eulerAnglesRotation;

            resultingEulerAngles.x = Mathf.Clamp(resultingEulerAngles.x, RotMinX, RotMaxX);

            transform.eulerAngles = resultingEulerAngles;
            //transform.eulerAngles += RotSpeed * new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
        }
    }
}
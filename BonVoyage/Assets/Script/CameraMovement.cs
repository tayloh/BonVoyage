using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private Vector3 previousPosition;

    [SerializeField] private PlayerInput playerInput;
    //controls camera speed
    public float MovementSpeed = 20f;
    public bool EnableTransitions = true;
    public bool EnableTransitionRotation = false;
    public bool EnableFreeCamera = false;

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
    public float TransitionTime = 2f;
    public Vector3 ShipCameraOffset = new Vector3(5, 10f, -5);

    private Vector3 _startPos = Vector3.zero;
    private Vector3 _currentLerpGoal = Vector3.zero;
    private Quaternion _startRotation = Quaternion.identity;
    private Quaternion _currentRotationGoal = Quaternion.identity;
    private float _tLerp = 0;
    static public bool _isTransitioning = false;
    static public bool isMoving = false;

    public Ship TreasureShip;
    [SerializeField]
    private Transform _activeShipTransform; // Chace the active ship transform

    public void SmoothlyTransitionTo(Vector3 position, Vector3 lookAt , Transform target )
    {
        _activeShipTransform = target;
        _startPos = transform.position;
        _currentLerpGoal = position;
        _startRotation = transform.rotation;
        _currentRotationGoal = Quaternion.LookRotation(lookAt - position);

        _tLerp = 0;
        _isTransitioning = true;
    }

    public bool IsTransitioning()
    {
        return _isTransitioning;
    }

    private void _smoothTransition()
    {
        var distanceLeft = (transform.position - _currentLerpGoal).magnitude;
        var dynamicSpeedModifier = Mathf.Clamp(
            Mathf.Pow(distanceLeft, 1.5f), 
            1.0f, 3.0f);

        var lerpStep = (1 / TransitionTime) * Time.deltaTime * dynamicSpeedModifier;

        // First frame has a long deltaTime...
        if (Time.deltaTime > 0.1) lerpStep = (1 / TransitionTime) * (1 / 60) * dynamicSpeedModifier;

        _tLerp += lerpStep;

        transform.position = Vector3.Lerp(_startPos, _currentLerpGoal, _tLerp);

        if (EnableTransitionRotation)
        {
            transform.rotation = Quaternion.Slerp(_startRotation, _currentRotationGoal, _tLerp);
        }
        
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

    public void SetOffset(Ship ship)
    {
        ShipCameraOffset = transform.position - ship.transform.position;
    }

    private void ClampAlongLookDir()
    {
        var lookDir = (_activeShipTransform.position - transform.position).normalized;
        var currentDistance = (transform.position - _activeShipTransform.position).magnitude;

        if (currentDistance < minLimiter_y)
        {
            transform.position = _activeShipTransform.position + minLimiter_y * 1.001f * -lookDir;
        }

        if (currentDistance > maxLimiter_y)
        {
            transform.position = _activeShipTransform.position + maxLimiter_y * 0.999f * -lookDir;
        }
    }


    private void LateUpdate()
    {

        //if (Input.GetKey(KeyCode.J)) SmoothlyTransitionTo(new Vector3(5, 8, 5), new Vector3(5, 5, 5));

        if (_isTransitioning && EnableTransitions)
        {
            _smoothTransition();
        }
        else if (_activeShipTransform != null)
        {
            transform.LookAt(_activeShipTransform);
            ClampAlongLookDir();
        }

        //ShipCameraOffset.y = transform.position.y;

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
        if (EnableFreeCamera)
        {
            resultingMoveDir = resultingMoveDir.normalized;
            CamPos += resultingMoveDir * MovementSpeed * Time.deltaTime * (CamPos.y + 2 - minLimiter_y) / 10; //modulate depending on zoom

        }

        //Handles zoom via mousescrolling by checking speed and direction of scroll, uses unitys built in input manager for the scroll variable
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0 && _activeShipTransform != null && !_isTransitioning)
        {
            transform.forward = (_activeShipTransform.position - transform.position).normalized;
            
            var resultingCamPos = CamPos + transform.forward * scroll * scrollSpeed * Time.deltaTime;
            var newDistToAnchor = (resultingCamPos - _activeShipTransform.position).magnitude;

            var currDistToAnchor = (transform.position - _activeShipTransform.position).magnitude;

            bool isValidMove = false;

            if (newDistToAnchor > minLimiter_y && newDistToAnchor < maxLimiter_y)
            {
                isValidMove = true;
            }
            else if (currDistToAnchor > maxLimiter_y && newDistToAnchor < currDistToAnchor)
            {
                isValidMove = true;
            }
            else if (currDistToAnchor < minLimiter_y && newDistToAnchor > currDistToAnchor)
            {
                isValidMove = true;
            }

            if (isValidMove)
            {
                var movementVector = transform.forward * scroll * scrollSpeed * Time.deltaTime;
                movementVector.Normalize();

                CamPos += movementVector;
            }
        }


        // If the camera is at the ceiling or the floor, move it only in y direction when scrolling
        


        //zoom limiter
        //CamPos.y = Mathf.Clamp(CamPos.y, minLimiter_y, maxLimiter_y);
        //makes sure that the values cant go beyond the limiters
        //CamPos.x = Mathf.Clamp(CamPos.x, -limiter_x, limiter_x);
        //CamPos.z = Mathf.Clamp(CamPos.z, -limiter_z, limiter_z);

        
        // Clamp to treasureship when not in transition
        if (TreasureShip != null && !_isTransitioning)
        {
            CamPos.x = Mathf.Clamp(CamPos.x, TreasureShip.transform.position.x - limiter_x, TreasureShip.transform.position.x + limiter_x);
            CamPos.z = Mathf.Clamp(CamPos.z, TreasureShip.transform.position.z - limiter_z, TreasureShip.transform.position.z + limiter_z);
        }
        
        //applies all changes
        // Don't let the player control the camera if there isn't a treasure ship
        // Otherwise they might move the grid off of the ships, which causes problems
        // Edit: xz movement is not controlled by the player anymore
        if (!_isTransitioning) //&& TreasureShip != null)
        {
            transform.position = CamPos;
        }
        
        if(Input.GetMouseButtonDown(1) && !_isTransitioning)
        {
            isMoving = true;
        }

        // Not doing anything currently but leaving the code here...
        if (Input.GetMouseButton(1) && !_isTransitioning)
        {
            //updating cursor 
            playerInput.UpdateCursor(CursorState.RotateCamera);
            //transform.eulerAngles += RotSpeed * new Vector3(-Input.GetAxis("Mouse Y"), 0, 0);
            //transform.eulerAngles += RotSpeed * new Vector3(0, Input.GetAxis("Mouse X"), 0);
            Vector3 currentEulerAngles = transform.eulerAngles;


            //Quaternion rot = Quaternion.AngleAxis(Time.deltaTime * RotSpeed * Input.GetAxis("Mouse X"), anchorShipYAxis);

            Vector3 eulerAnglesRotation = Time.deltaTime * RotSpeed * new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
            Vector3 resultingEulerAngles = currentEulerAngles + eulerAnglesRotation;

            //transform.eulerAngles = resultingEulerAngles;
            //transform.eulerAngles += RotSpeed * new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);

        }
        if (Input.GetMouseButtonUp(1))
        {
            playerInput.UpdateCursor(CursorState.General);
            isMoving = false;
        }

        // Rotation around anchor
        var cam = Camera.main;
        if (Input.GetMouseButtonDown(1) && !_isTransitioning)
        {
            previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButton(1) && !_isTransitioning)
        {
            var currentOffset = (transform.position - _activeShipTransform.position).magnitude;

            Vector3 newPosition = cam.ScreenToViewportPoint(Input.mousePosition);
            Vector3 direction = previousPosition - newPosition;

            float rotationAroundYAxis = -direction.x * 180; // camera moves horizontally
            float rotationAroundXAxis = direction.y * 180; // camera moves vertically

            // Set the camera transform to the object we want to rotate around
            transform.position = _activeShipTransform.position;

            // Rotate around our local x axis (keep in mind we are at "origin")
            transform.Rotate(new Vector3(1, 0, 0), rotationAroundXAxis);

            // Rotate the world y axis (rotating around local y axis won't work since 
            // it got rotated when we rotated locally around x
            transform.Rotate(new Vector3(0, 1, 0), rotationAroundYAxis, Space.World);

            // Clamp rotation around x (super easy since we are at origin)
            var eulerAngles = transform.eulerAngles;
            eulerAngles.x = Mathf.Clamp(eulerAngles.x, 15, 60);

            transform.eulerAngles = eulerAngles;

            // Translate out the distance we were at before the rotation along our z axis
            transform.Translate(new Vector3(0, 0, -currentOffset));

            previousPosition = newPosition;

        }
    }

}
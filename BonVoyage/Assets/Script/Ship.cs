using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    [SerializeField]
    private int movementPoints = 1;
    public int MovementPoints { get => movementPoints; }

    [SerializeField]
    private int fireRange = 3;
    public int FireRange { get => fireRange; }

    [SerializeField]
    private float _movementDuration = 1.0f;
    public float MovementDuration { get => _movementDuration; }
    [SerializeField]
    private float _rotationDuration = 0.5f;

    private GlowHighlight _glowHighlight;

    [SerializeField]
    private HexGrid hexGrid;

    private Queue<Vector3> _pathPositions = new Queue<Vector3>();

    public event Action<Ship> MovementFinished;

    private Vector3Int hexCoord;

    private void Awake()
    {
        _glowHighlight = GetComponent<GlowHighlight>();

        //compute hex coord of the ship and assign the ship to corresponding hex tile
        hexCoord = HexCoordinates.ConvertPositionToOffset(gameObject.transform.position - new Vector3Int(0,1,0));
    }

    private void Start()
    {
        Debug.Log("Ship script: hex coord of the ship " + hexCoord);
        hexGrid.PlaceShip(hexCoord, this);
    }

    public void Deselect()
    {
        _glowHighlight.ToggleGlow(false);
    }

    public void Select()
    {
        _glowHighlight.ToggleGlow();
    }

    public void MoveThroughPath(List<Vector3> path)
    {
        _pathPositions = new Queue<Vector3>(path);
        Vector3 firstTarget = _pathPositions.Dequeue();
        StartCoroutine(RotationCoroutine(firstTarget, _rotationDuration));        
    }

    public IEnumerator RotationCoroutine(Vector3 endPosition, float rotationDuration)
    {
        Quaternion startRotation = transform.rotation;
        endPosition.y = transform.position.y;
        Vector3 direction = endPosition - transform.position;
        Quaternion endRotation = Quaternion.LookRotation(direction, Vector3.up);
        StartCoroutine(MovementCoroutine(endPosition));
        if (Mathf.Approximately(Mathf.Abs(Quaternion.Dot(startRotation, endRotation)), 1.0f) == false) //The rottation is needed only if the ship does not face the good direction
        {
            float timeElapsed = 0;
            while (timeElapsed < rotationDuration)
            {
                timeElapsed += Time.deltaTime;
                float lerpStep = timeElapsed / rotationDuration;
                transform.rotation = Quaternion.Lerp(startRotation, endRotation, lerpStep);
                yield return null;
            }
            transform.rotation = endRotation;
        }
    }

    public IEnumerator MovementCoroutine(Vector3 endPosition)
    {
        Vector3 startPosition = transform.position;
        endPosition.y = startPosition.y;

        float timeElapsed = 0;

        while (timeElapsed < _movementDuration)
        {
            timeElapsed += Time.deltaTime;
            float lerpStep = timeElapsed/_movementDuration;
            transform.position = Vector3.Lerp(startPosition, endPosition, lerpStep);
            yield return null;
        }
        transform.position = endPosition;

        //The following part may not be useful if we want to move 1 cell at a time
        if (_pathPositions.Count > 0)
        {
            Debug.Log("Selecting next position...");
            StartCoroutine(RotationCoroutine(_pathPositions.Dequeue(), _rotationDuration));
        }
        else
        {
            Debug.Log("Movement finished.");
            MovementFinished?.Invoke(this);
        }
        UpdateShipTile(startPosition, endPosition);
    }

    private void UpdateShipTile(Vector3 previousPosition, Vector3 newPosition)
    {        
        //set previoustile.Ship à null et set newtile.ship à ship
        //updat ethe type of hex, obstacle if there is a ship, water if not
        Hex previousTile = hexGrid.GetTileAt(HexCoordinates.ConvertPositionToOffset(previousPosition - new Vector3(0, 1, 0)));
        previousTile.Ship = null;
        previousTile.HexType = HexType.Water;
        Hex newTile = hexGrid.GetTileAt(HexCoordinates.ConvertPositionToOffset(newPosition - new Vector3(0, 1, 0)));
        newTile.Ship = this;
        newTile.HexType = HexType.Obstacle;
        Debug.Log("Ship moved from " + HexCoordinates.ConvertPositionToOffset(previousPosition) + " to " + HexCoordinates.ConvertPositionToOffset(newPosition));
    }

}

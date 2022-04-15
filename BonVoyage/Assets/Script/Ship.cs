using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{

    private int _health = 5;
    public int Health { get => _health; }

    private int _attack = 3;
    public int AttackDamage { get => _attack; }

    private bool _dead = false;
    public bool IsDead { get => _dead; }

    public event Action<Ship> MovementFinished;

    public Vector3Int hexCoord;

    [SerializeField]
    private HexGrid hexGrid;

    [Header("Ship stats") ]
    [SerializeField]
    private int movementPoints = 1;
    public int MovementPoints { get => movementPoints; }

    [SerializeField]
    private int fireRange = 3;
    public int FireRange { get => fireRange; }

    [Header("Movement animation")]
    [SerializeField]
    private float _movementDuration = 1.0f;
    public float MovementDuration { get => _movementDuration; }
    [SerializeField]
    private float _rotationDuration = 0.5f;

    private GlowHighlight _glowHighlight;

    private Queue<Vector3> _pathPositions = new Queue<Vector3>();

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
        if (_dead)
        {
            MovementFinished?.Invoke(this);
            return;
        }

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
            // Start movement after rotation, though there's a bug so commented it out for now
            //StartCoroutine(MovementCoroutine(endPosition));
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
        UpdateShipTile(startPosition, endPosition); // moved this here, need to update pos before invoking move finished
        if (_pathPositions.Count > 0)
        {
            Debug.Log("Selecting next position...");
            StartCoroutine(RotationCoroutine(_pathPositions.Dequeue(), _rotationDuration));
        }
        else
        {
            Debug.Log("Movement finished.");
            
            // Invoke the event after the coordinates have been updated
            MovementFinished?.Invoke(this);

        }
    }

    private void UpdateShipTile(Vector3 previousPosition, Vector3 newPosition)
    {        
        //set previoustile.Ship � null et set newtile.ship � ship
        //update the type of hex, obstacle if there is a ship, water if not
        Hex previousTile = hexGrid.GetTileAt(HexCoordinates.ConvertPositionToOffset(previousPosition - new Vector3(0, 1, 0)));
        previousTile.Ship = null;
        previousTile.HexType = HexType.Water;
        Hex newTile = hexGrid.GetTileAt(HexCoordinates.ConvertPositionToOffset(newPosition - new Vector3(0, 1, 0)));
        newTile.Ship = this;
        newTile.HexType = HexType.Obstacle;

        // Need to put this somewhere else, it's here because I need to make sure the hexcoords are updated in time
        hexCoord = newTile.HexCoords;
        //if (this.tag != "Pirate")
        //{
        //    HighLightAttackableTiles(0);
        //    HighLightAttackableTiles(1);
        //}


        Debug.Log("Ship moved from " + HexCoordinates.ConvertPositionToOffset(previousPosition) + " to " + HexCoordinates.ConvertPositionToOffset(newPosition));

    }

    /// <summary>
    /// Left side = 1, Right side = 0;
    /// </summary>
    public List<Vector3Int> GetAttackableTilesFor(int broadside)
    {
        return hexGrid.GetAttackableTilesFor(hexCoord, broadside, fireRange);
    }

    public void HighLightAttackableTiles(int broadside)
    {
        if (broadside != 0 && broadside != 1)
        {
            throw new Exception("Broadside can only be 0=right, or 1=left");
        }

        //Vector3Int hexPos = hexGrid.GetClosestHex(..);
        Debug.Log("Attempting highlight: " + hexCoord);
        List<Vector3Int> res = hexGrid.GetAttackableTilesFor(hexCoord, broadside, fireRange);

        foreach (var tile in res)
        {
            Hex hex = hexGrid.GetTileAt(tile);
            if (hex != null)
            {
                hex.EnableHighLight();
            }
            else
            {
                Debug.Log("Could not find tile: " + tile);
            }
            //Debug.Log(tile);
        }
    }

    // Requires that the ship is still on the same tile as it was when 
    // HighLightAttackableTiles() was called
    public void RemoveHighLightAttackableTiles(int broadside)
    {
        if (broadside != 0 && broadside != 1)
        {
            throw new Exception("Broadside can only be 0=right, or 1=left");
        }

        List<Vector3Int> res = hexGrid.GetAttackableTilesFor(hexCoord, broadside, fireRange);
        
        foreach (var tile in res)
        {
            Hex hex = hexGrid.GetTileAt(tile);
            if (hex != null)
            {
                hex.DisableHighlight();
            }
        }
    }


    public void TakeDamage(int damage)
    {
        _health -= damage;

        if (_health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(this.name + " died.");

        _dead = true;
        StartCoroutine(ShipSinksAnimation());
    }

    private IEnumerator ShipSinksAnimation()
    {
        while (transform.position.y > -1)
        {
            transform.position -= new Vector3(0, Time.deltaTime, 0);
            yield return null;
        }
    }

}

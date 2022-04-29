using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class Ship : MonoBehaviour
{
    private float _maxhealth;

    [SerializeField]
    private float _health = 5;

    [SerializeField]
    private int _repairPoint = 1; // this how many  ok.
    public float Health { get => _health; }

    [SerializeField]
    private float _attack = 3;
    public float AttackDamage { get => _attack; } // Not used anymore, see list of Cannons

    private bool _dead = false;
    public bool IsDead { get => _dead; }

    // ...
    public bool HasFiredLeft = false;
    public bool HasFiredRight = false;


    public event Action<Ship> DeathAnimationFinished;

    public event Action<Ship> MovementFinished;

    public Vector3Int hexCoord;

    [SerializeField]
    private HexGrid hexGrid;


    [Header("UI")]
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Image _healtSlider;
    [SerializeField] private Text _damageText;
    [SerializeField] private TMP_Text _totalHealthText;
    private TMP_Text _currentHealthText;
    [SerializeField] private float _canvaSizeOnScreen = 1f;

    [Header("Ship stats")]
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

    private List<Cannon> _cannons = new List<Cannon>();

    private void Awake()
    {
        _glowHighlight = GetComponent<GlowHighlight>();

        //compute hex coord of the ship and assign the ship to corresponding hex tile
        hexCoord = HexCoordinates.ConvertPositionToOffset(gameObject.transform.position - new Vector3Int(0, 1, 0));
        //setting up UI
        _maxhealth = _health;
        if (_healtSlider != null)
            _healtSlider.fillAmount = (float)_health / (float)_maxhealth;
        if (_damageText != null) _damageText.gameObject.SetActive(false);
        _currentHealthText = _totalHealthText.transform.GetChild(0).GetComponent<TMP_Text>();
        _totalHealthText.text = "/" + _maxhealth.ToString();
        _currentHealthText.text = _health.ToString();

        _cannons.AddRange(transform.GetComponentsInChildren<Cannon>());
    }

    private void Start()
    {
        Debug.Log("Ship script: hex coord of the ship " + hexCoord);
        hexGrid.PlaceShip(hexCoord, this);
    }

    private void LateUpdate()
    {
        // Make canvas face the camera, always
        Camera camera = Camera.main;
        _canvas.transform.LookAt(_canvas.transform.position + camera.transform.rotation * Vector3.forward, camera.transform.rotation * Vector3.up);
        _canvas.transform.localScale = new Vector3(_canvaSizeOnScreen, _canvaSizeOnScreen, _canvaSizeOnScreen) * Vector3.Dot(camera.transform.position - _canvas.transform.position, -camera.transform.forward);
        
    }
    
    public float[] GetCannonDamageList()
    {
        float[] damagePerCannon = new float[_cannons.Count];
        for (int i = 0; i < damagePerCannon.Length; i++)
        {
            damagePerCannon[i] = _cannons[i].Damage;
        }
        return damagePerCannon;
    }

    public void Deselect()
    {
        //_glowHighlight.ToggleGlow(false);
    }

    public void Select()
    {
        //_glowHighlight.ToggleGlow();
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
            float lerpStep = timeElapsed / _movementDuration;
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


    public void TakeDamage(float damage)
    {
        StartCoroutine(TakeDamageAnimation());

        _health -= damage;
        if (_healtSlider != null)
            _healtSlider.fillAmount = (float)_health / (float)_maxhealth;
        _currentHealthText.text = _health.ToString();
        StartCoroutine(ShowText("-" + damage.ToString(), -1));
        if (_health <= 0)
        {
            Die();
        }

    }

    public bool HasAttackableInRange(string tag)
    {
        if (tag != "PlayerShip" && tag != "Pirate") return false;

        // Get attackable tiles
        List<Vector3Int> attackableRightSide = gameObject.GetComponent<Ship>().GetAttackableTilesFor(0);
        List<Vector3Int> attackableLeftSide = gameObject.GetComponent<Ship>().GetAttackableTilesFor(1);

        List<Vector3Int> attackableTiles = attackableRightSide;
        attackableTiles.AddRange(attackableLeftSide);

        foreach (var tile in attackableTiles)
        {
            Hex currentHex = hexGrid.GetTileAt(tile);
            if (currentHex != null && currentHex.Ship != null && currentHex.Ship.gameObject.CompareTag(tag))
            {
                return true;
            }
        }

        return false;
    }

    IEnumerator ShowText(string damage, int sign = 1)
    {
        if (sign < 0)
            _damageText.color = Color.red;
        else
            _damageText.color = Color.green;

        if (_damageText == null) yield break;
        _damageText.text = damage.ToString();
        _damageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(5f);
        _damageText.gameObject.SetActive(false);

    }

    private IEnumerator TakeDamageAnimation()
    {
        yield return new WaitForSeconds(0.2f);
        transform.Find("ExplosionPS").GetComponentInChildren<ParticleSystem>().Play();
    }

    private void Die()
    {
        Debug.Log(this.name + " died.");

        _dead = true;
        StartCoroutine(ShipSinksAnimation());
    }

    private IEnumerator ShipSinksAnimation()
    {
        yield return new WaitForSeconds(1.1f);
        while (transform.position.y > -1)
        {
            transform.position -= new Vector3(0, Time.deltaTime, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(new Vector3(60, 0, 0)), Time.deltaTime);
            yield return null;
        }
        DeathAnimationFinished?.Invoke(this);
    }

    public void Repair()
    {
        if (_health == _maxhealth) return;
        _health += _repairPoint;
        _health = Mathf.Clamp(_health, 0, _maxhealth);

        if (_healtSlider != null)
        {
            _healtSlider.fillAmount = (float)_health / (float)_maxhealth;
        }
        _currentHealthText.text = _health.ToString();

        StartCoroutine(ShowText("+" + _repairPoint.ToString()));
    }

    public int GetNumberOfCannons()
    {
        return _cannons.Count;
    }
}


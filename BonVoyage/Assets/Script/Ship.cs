using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[SelectionBase]
public class Ship : MonoBehaviour
{
    public bool DisableSway = false;
    public float RockingSpeedMultiplier = 0.45f;
    public float RockingAngle = 11.0f;

    public ShipType _shipType;

    private bool _dead = false;
    public bool IsDead { get => _dead; }

    // ...
    public bool HasFiredLeft = false;
    public bool HasFiredRight = false;
    public bool IsFiring = false;

    public event Action<Ship> DeathAnimationFinished;
    public event Action<Ship> ShipIsOutOfGame;

    public event Action<Ship> MovementFinished;

    public Vector3Int hexCoord;

    private bool isAttackable = false;
    private bool isPlaying = false;
    public bool IsPlaying { set => isPlaying = value; }

    [SerializeField]
    private HexGrid hexGrid;
    private PlayerInput playerInput;
    [SerializeField]
    private GameManager gameManager;
    [SerializeField] 
    private MovementSystem movementSystem;
    private Collider _collider;


    [Header("UI")]
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Image _healtSlider;
    [SerializeField] private Text _damageText;
    [SerializeField] private TMP_Text _totalHealthText;
    private TMP_Text _currentHealthText;
    [SerializeField] private float _canvaSizeOnScreen = 1f;

    [Header("Ship stats")]
    private float _maxhealth;
    public float MaxHealth { get => _maxhealth; }
    [SerializeField]
    private float _health = 5;
    [SerializeField]
    private int movementPoints = 1;
    public int MovementPoints { get => movementPoints; }
    [SerializeField]
    private int _repairPoint = 1; // this how many  ok.
    public float Health { get => _health; }
    [SerializeField]
    private float _attack = 3;
    public float AttackDamage { get => _attack; } // Not used anymore, see list of Cannons

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

    private AudioSource _audioSource;

    private List<float> _cannonWaitFireDurations = new List<float> {
        0.14f, 0.15f, 0.16f, 0.17f, 0.18f, 0.19f,
        0.1f, 0.12f, 0.14f, 0.16f, 0.18f, 0.2f,
        0.1f, 0.12f, 0.14f, 0.16f, 0.18f, 0.2f,
        0.1f, 0.12f, 0.14f, 0.16f, 0.18f, 0.2f,
        0.1f, 0.12f, 0.14f, 0.16f, 0.18f, 0.2f,
        0.2f, 0.21f, 0.22f, 0.23f, 0.24f, 0.25f,
        0.26f, 0.27f, 0.28f, 0.29f, 0.30f, 0.31f,
        0.32f, 0.33f, 0.34f, 0.35f, 0.36f, 0.37f};

    private Hex tile;
    public Hex Tile { get => tile; }

    public int NumCannons
    {
        get => _cannons.Count;
    }

    private void Awake()
    {
        _glowHighlight = GetComponent<GlowHighlight>();
        _collider = GetComponent<Collider>();
        _audioSource = GetComponent<AudioSource>();

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

        playerInput = GameObject.Find("PlayerInput").GetComponent<PlayerInput>(); //not the most efficient way to find it 
    }

    private void Start()
    {
        Debug.Log("Ship script: hex coord of the ship " + hexCoord);
        hexGrid.PlaceShip(hexCoord, this);
        tile = hexGrid.GetTileAt(HexCoordinates.ConvertPositionToOffset(transform.position - Vector3.up));

        if (!DisableSway)
        {
            StartCoroutine(SwayAnimation());
        }
    }

    private void LateUpdate()
    {
        // Make canvas face the camera, always
        //var canvas = gameObject.transform.Find("Canvas");

        Camera camera = Camera.main;
        _canvas.transform.LookAt(_canvas.transform.position + camera.transform.rotation * Vector3.forward, camera.transform.rotation * Vector3.up);
        _canvas.transform.localScale = new Vector3(_canvaSizeOnScreen, _canvaSizeOnScreen, _canvaSizeOnScreen) * Vector3.Dot(camera.transform.position - _canvas.transform.position, -camera.transform.forward);
    }

    public List<float> GetCannonWaitFireDurations()
    {
        // Make a copy of the list, it doesn't return the updated one otherwise....
        List<float> result = new List<float>();
        foreach (var item in _cannonWaitFireDurations)
        {
            result.Add(item);
        }
        return result;
    }

    public float[] GetLeftSideCannonDamageList()
    {
        var leftCannons = transform.GetChild(0).GetComponentsInChildren<Cannon>();
        float[] damagePerCannon = new float[leftCannons.Length];
        for (int i = 0; i < damagePerCannon.Length; i++)
        {
            damagePerCannon[i] = leftCannons[i].Damage;
        }
        return damagePerCannon;

    }

    public float[] GetRightSideCannonDamageList()
    {
        var rightCannons = transform.GetChild(1).GetComponentsInChildren<Cannon>();
        float[] damagePerCannon = new float[rightCannons.Length];
        for (int i = 0; i < damagePerCannon.Length; i++)
        {
            damagePerCannon[i] = rightCannons[i].Damage;
        }
        return damagePerCannon;
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
        _collider.enabled = false;

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
                Quaternion rotationToApply = Quaternion.Lerp(startRotation, endRotation, lerpStep);
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, rotationToApply.eulerAngles.y, transform.rotation.eulerAngles.z);
                yield return null;
            }
            endRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, endRotation.eulerAngles.y, transform.rotation.eulerAngles.z);
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
        tile = hexGrid.GetTileAt(HexCoordinates.ConvertPositionToOffset(transform.position - Vector3.up));
        new WaitForEndOfFrame();
        _collider.enabled = true;
    }

    private void UpdateShipTile(Vector3 previousPosition, Vector3 newPosition)
    {
        //set previoustile.Ship à null et set newtile.ship à ship
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
        //Debug.Log("Attempting highlight: " + hexCoord);
        List<Vector3Int> res = hexGrid.GetAttackableTilesFor(hexCoord, broadside, fireRange);

        foreach (var tile in res)
        {
            Hex hex = hexGrid.GetTileAt(tile);
            if (hex != null)
            {
                if (isPlaying)
                {
                    hex.EnableHighLight();
                    Ship targetShip = hex.Ship;
                    if (targetShip != null && targetShip.tag != this.tag)
                    {
                        targetShip.isAttackable = true;
                    }
                    else if (targetShip != null && targetShip.tag == this.tag)
                    {
                        // Disable highlight for ships of same type as yourself.
                        hex.DisableHighlight();
                    }
                }
                else if (CameraMovement.isMoving == false && (gameManager.state == GameState.PlayerMove || gameManager.state == GameState.PlayerFire))
                {
                    hex.HighlightHexOfFiringArc(transform.tag);
                }
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

    public void ResetAttackableShips()
    {
        for (int broadside = 0; broadside < 2; broadside++)
        {
            List<Vector3Int> res = hexGrid.GetAttackableTilesFor(hexCoord, broadside, fireRange);
            foreach (var tile in res)
            {
                Hex hex = hexGrid.GetTileAt(tile);
                if (hex != null && hex.Ship != null)
                {
                    hex.Ship.isAttackable = false;
                }
            }
        }
    }

    public void TakeDamage(float[] damageList)
    {
        // If we want separate animations, damage numbers, etc
        // per shot, start here!

        // Shuffle the cannon wait duration list
        _cannonWaitFireDurations.Shuffle();

        StartCoroutine(TakeDamagePerShot(damageList));
    }

    private IEnumerator TakeDamagePerShot(float[] damageList)
    {
        var color = Color.red;
        
        var totalDmg = 0f;

        var inParenthesisText = "";

        var hitsAndMissCountText = "";

        var misses = 0;
        var hits = 0;

        var hasSternBonus = false;
        var hasBowBonus = false; // not really a bonus

        var index = 0;
        foreach (var dmg in damageList)
        {
            var totalDmgText = "";
            

            totalDmg += dmg;
            if (dmg == 0)
            {
                inParenthesisText += "- ";
                misses++;
            }
            else if (Mathf.Approximately(dmg, Mathf.CeilToInt(DamageModel.SternDamageAmplifier * _cannons[0].Damage)))
            {
                inParenthesisText += dmg.ToString(); //+ "! ";
                hasSternBonus = true;
                hits++;
                StartCoroutine(TakeDamageAnimation());
            }
            else if (Mathf.Approximately(dmg, Mathf.CeilToInt(DamageModel.BowDamageAmplifier * _cannons[0].Damage)))
            {
                inParenthesisText += dmg.ToString(); //+ "! ";
                hasBowBonus = true;
                hits++;
                StartCoroutine(TakeDamageAnimation());
            }
            else
            {
                inParenthesisText += dmg.ToString() + " ";
                hits++;
                StartCoroutine(TakeDamageAnimation());
            }
            totalDmgText = totalDmg.ToString();

            hitsAndMissCountText = "Hit x" + hits + "  " + "Miss x" + misses;

            var fullText = "";
            if (hasSternBonus)
            {
                fullText = totalDmgText + " Dmg (Stern)" + "\n" + hitsAndMissCountText;
            }
            else if (hasBowBonus)
            {
                fullText = totalDmgText + " Dmg (Bow)" + "\n" + hitsAndMissCountText;
            }
            else
            {
                fullText = totalDmgText + " Dmg (Side)" + "\n" + hitsAndMissCountText;
            }

            _health -= dmg;
            _health = Mathf.Clamp(_health, 0, _health);

            if (_healtSlider != null)
            {
                _healtSlider.fillAmount = (float)_health / (float)_maxhealth;
            }
            _currentHealthText.text = _health.ToString();

            if (_health <= 0 && !_dead)
            {
                Die();
            }

            _damageText.fontSize = 140;
            _damageText.text = fullText;
            _damageText.color = color;
            _damageText.gameObject.SetActive(true);

            var waitDuration = _cannonWaitFireDurations[index % _cannonWaitFireDurations.Count];
            yield return new WaitForSeconds(waitDuration);
            index++;
        }
        yield return new WaitForSeconds(1.5f);
        _damageText.gameObject.SetActive(false);
    }

    public void TakeDamage(float damage)
    {
        if (damage > 0)
        {
            StartCoroutine(TakeDamageAnimation());
        }
        

        _health -= damage;
        _health = Mathf.Clamp(_health, 0, _health);
        if (_healtSlider != null)
        {
            _healtSlider.fillAmount = (float)_health / (float)_maxhealth;
        }
        _currentHealthText.text = _health.ToString();

        if (damage > 0)
        {
            StartCoroutine(ShowText("-" + damage.ToString(), -1));
        }
        else
        {
            StartCoroutine(ShowText("Miss!", 0));
        }
        
        if (_health <= 0 && !_dead)
        {
            Die();
        }

    }

    public bool HasAttackableInRange(string tag)
    {
        if (tag != "PlayerShip" && tag != "Pirate") return false;

        // Get attackable tiles                 
        List<Vector3Int> attackableRightSide = GetAttackableTilesFor(0);
        List<Vector3Int> attackableLeftSide = GetAttackableTilesFor(1);

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

    //0 = right, 1 = left
    public bool CheckIfShipIsInBroadSide(Ship otherShip, int broadside)
    {
        var attackableTiles = GetAttackableTilesFor(broadside);
        foreach (var tile in attackableTiles)
        {
            Hex currentHex = hexGrid.GetTileAt(tile);
            if (currentHex != null && currentHex.Ship != null &&
                currentHex.Ship.gameObject.GetInstanceID() == otherShip.gameObject.GetInstanceID())
            {
                return true;
            }
        }
        return false;
    }

    IEnumerator ShowText(string damage, int sign = 1)
    {
        if (sign < 0)
        {
            _damageText.color = Color.red;
        }   
        else if (sign > 0)
        {
            _damageText.color = Color.green;
        }
        else if (sign == 0)
        {
            _damageText.color = Color.white;
        }   

        if (_damageText == null) yield break;
        _damageText.text = damage.ToString();
        _damageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2.5f);
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

        // Make the hexagon it stood on a non obstacle.
        hexGrid.GetTileAt(this.hexCoord).HexType = HexType.Water;
        ShipIsOutOfGame?.Invoke(this);
        _dead = true;
        StartCoroutine(ShipSinksAnimation());
    }

    private IEnumerator ShipSinksAnimation()
    {
        _audioSource.Play();
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

    private void OnMouseOver()
    {
        if (!CameraMovement.isMoving && !CameraMovement._isTransitioning)
        {
            // For active ship
            if (this.GetInstanceID() == gameManager.GetActualShip().GetInstanceID())
            {
                if (gameManager.state == GameState.PlayerMove)
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        hexGrid.DisableHighlightOfAllHexes();
                        HighLightAttackableTiles(0);
                        HighLightAttackableTiles(1);
                    }
                }
            }

            if (isPlaying && !IsFiring)
            {
                playerInput.UpdateCursor(CursorState.SkipTurn);
            }
            else if (HasFiredLeft || HasFiredRight)
            {                
            }
            else if (gameManager.state == GameState.PlayerMove || gameManager.state == GameState.PlayerFire)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    HighLightAttackableTiles(0);
                    HighLightAttackableTiles(1);
                }
            }
            
            if (isAttackable)
            {
                playerInput.UpdateCursor(CursorState.AttackTarget);
            }
        }

        ShipStatsPanel.Instance.Show();
        ShipStatsPanel.Instance.UpdatePanel(this);
    }

    private void OnMouseExit()
    {
        // For active ship
        if (this.GetInstanceID() == gameManager.GetActualShip().GetInstanceID())
        {
            if (gameManager.state == GameState.PlayerMove)
            {

                hexGrid.DisableHighlightOfAllHexes();
                movementSystem.ShowRange(gameManager.GetActualShip(), hexGrid);

            }
        }

        playerInput.UpdateCursor(CursorState.General);
        if(!isPlaying)
        {
            RemoveHighLightAttackableTiles(0);
            RemoveHighLightAttackableTiles(1);

            //highlight again the tiles of the actual ship in case some of them were hidden
            Ship actualShip = gameManager.GetActualShip();
            if(!actualShip.HasFiredLeft && !actualShip.HasFiredRight)
            {
                switch (gameManager.state)
                {
                    case GameState.PlayerMove:
                        movementSystem.ShowRange(gameManager.GetActualShip(), hexGrid);
                        break;
                    case GameState.PlayerFire:
                        actualShip.HighLightAttackableTiles(0);
                        actualShip.HighLightAttackableTiles(1);
                        break;
                }
            }
                        
        }

        ShipStatsPanel.Instance.Hide();
    }

    private IEnumerator SwayAnimation()
    {
        Quaternion startRotation = transform.rotation;
        while (!_dead)
        {

            float f = Mathf.Sin(Time.time * RockingSpeedMultiplier) * RockingAngle;

            Quaternion rotationToApply = startRotation * Quaternion.AngleAxis(f, Vector3.forward);

            transform.rotation = Quaternion.Euler(rotationToApply.eulerAngles.x, transform.rotation.eulerAngles.y, rotationToApply.eulerAngles.z);
            

            yield return null;
        }
        yield break;
    }

    public int GetNumberOfCannons()
    {
        return _cannons.Count;
    }

    public int GetNumberOfCannonsPerSide()
    {
        return Mathf.RoundToInt(_cannons.Count / 2);
    }
}


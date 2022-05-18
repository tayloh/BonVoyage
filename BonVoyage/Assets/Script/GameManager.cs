using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState state; 
    private bool isGameOver = false;
    private bool aiWon = false;
    public static event Action<GameState> OnGameStateChanged;
    public UnityEvent<List<Ship>, int> OnTurnChanged;

    public Ship TreasureShip;

    private List<Ship> playerShips = new List<Ship>();
    private List<Ship> pirateShips = new List<Ship>();

    [SerializeField]
    private GameObject playerShipsParent;
    [SerializeField]
    private GameObject pirateShipsParent;
    [SerializeField]
    private GameObject fireButton;
    [SerializeField]
    private GameObject skipButton;
    [SerializeField]
    private GameObject gameOverText;
    [SerializeField]
    private GameObject victoryText;

    [SerializeField]
    private GameObject gameStatePanel;

    private List<Ship> shipsTurn = new List<Ship>();
    private int actualShipIndex = 0;

    [SerializeField]
    private ShipManager shipManager;
    [SerializeField]
    private TurnQueue turnQueue;

    [SerializeField]
    private CameraMovement cameraMovement;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        foreach(Ship ship in playerShipsParent.GetComponentsInChildren<Ship>())
        {
            playerShips.Add(ship);

            ship.DeathAnimationFinished += CleanupDeadShip;
            ship.ShipIsOutOfGame += RemoveDeadShipFromQueue;
        }
        foreach (Ship pship in pirateShipsParent.GetComponentsInChildren<Ship>())
        {
            pirateShips.Add(pship);

            pship.DeathAnimationFinished += CleanupDeadShip;
            pship.ShipIsOutOfGame += RemoveDeadShipFromQueue;
        }

        //Generating the random order of ships for both player and pirates
        PrepareTurn();
        turnQueue.CreateCards(shipsTurn);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void PrepareTurn()
    {
        shipsTurn.AddRange(playerShips);
        shipsTurn.AddRange(pirateShips);
        shipsTurn.Shuffle();
        StartCoroutine(FirstTurn());
    }

    private IEnumerator FirstTurn()
    {
        //cameraMovement.SmoothlyTransitionTo(new Vector3(0, 40, 0), new Vector3(0, 0, 0));
        yield return new WaitForSecondsRealtime(1.0f);
        //cameraMovement.SmoothlyTransitionTo(new Vector3(0, 10, 0), new Vector3(0, 0, 10));
        //yield return new WaitForSecondsRealtime(0.3f);

        NextTurn();
        yield return null;
    }

    private void UpdateGameStatePanelText(GameState state)
    {
        var text = gameStatePanel.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        switch (state)
        {
            case GameState.PlayerMove:
                text.text = "Player Move";
                text.color = Color.yellow;
                break;

            case GameState.PlayerFire:
                text.text = "Player Fire";
                text.color= Color.red;
                break;

            case GameState.PirateTurn:
                text.text = "Enemy Turn";
                text.color = Color.black;
                break;

            default:
                text.text = state.ToString();
                text.color = Color.black;
                break;

        }
    }

    public void UpdateGameState(GameState newState)
    {
        state = newState;

        UpdateGameStatePanelText(newState);

        switch (newState)
        {
            case GameState.PlayerMove:
                Debug.Log("It is Player's turn to move"); 
                fireButton.SetActive(false);
                skipButton.SetActive(true);
                //NextTurnPlayer();
                break;
            case GameState.PlayerFire:
                Debug.Log("It is Player's turn to fire");
                //fireButton.SetActive(true);
                skipButton.SetActive(true);
                break;
            case GameState.PirateTurn:
                Debug.Log("It is Pirates' turn");
                fireButton.SetActive(false);
                skipButton.SetActive(false);
                break;
            case GameState.Upgrade:
                //player can upgrade his fleet
                break;
            case GameState.Victory:
                victoryText.SetActive(true);
                skipButton.SetActive(false);
                break;
            case GameState.Defeat:
                gameOverText.SetActive(true);
                skipButton.SetActive(false);
                break;
        }
        OnGameStateChanged?.Invoke(newState);
    }


    private void CameraTransition(Ship ship)
    {
        cameraMovement.SmoothlyTransitionTo(
            ship.transform.position + cameraMovement.ShipCameraOffset,
            ship.transform.position , ship.transform);
    }

    public bool IsCameraTransitioning()
    {
        return cameraMovement.IsTransitioning();
    }
    
    public bool CheckForWinCondition()
    {
        Debug.Log("Checking for win...");
        Debug.Log(pirateShips.Count + " remaining pships");
        if (pirateShips.Count == 0)
        {
            UpdateGameState(GameState.Victory);
            isGameOver = true;
        }
        else if (playerShips.Count == 0)
        {
            UpdateGameState(GameState.Defeat);
            isGameOver = true;
            aiWon = true;
        }
        else if (this.TreasureShip == null || this.TreasureShip.IsDead)
        {
            UpdateGameState(GameState.Defeat);
            isGameOver = true;
            aiWon = true;
        }

        return isGameOver;
    }

    public void NextTurn()
    {
        // Check for win condition right before getting next ship (in case ai won)
        // but there are player ships left (treasure ship sunk).
        CheckForWinCondition();

        Ship nextShip = GetNextShipForTurn();

        // If the AI won, let it play until all player ships are dead :)
        if (aiWon)
        {
            while (!nextShip.CompareTag("Pirate") && !nextShip.IsDead)
            {
                nextShip = GetNextShipForTurn();
            }
        }

        if (nextShip.IsDead) nextShip = GetNextShipForTurn();

        CameraTransition(nextShip);

        StartCoroutine(DelayNextTurn(nextShip));// calling the delay function
    }
    // This function will help delay the  next ship in the turn  so the camera can fully repositioned
    IEnumerator DelayNextTurn(Ship nextShip)
    {
        skipButton.SetActive(false);
        yield return new WaitForSeconds(cameraMovement.TransitionTime - 0.2f);
        if (!nextShip.CompareTag("Pirate"))
        {
            UpdateGameState(GameState.PlayerMove);
            shipManager.StartPlayerTurn(nextShip);
        }
        else
        {
            UpdateGameState(GameState.PirateTurn);
            shipManager.StartPirateTurn(nextShip);
            //shipManager.MovePirateShip(nextShip);
        }
        OnTurnChanged.Invoke(shipsTurn, actualShipIndex);        
    }

    private Ship GetNextShipForTurn()
    {
        if (shipsTurn.Count - 1 > actualShipIndex)
        {
            actualShipIndex += 1;
        }
        else
        {
            actualShipIndex = 0;
        }
        return shipsTurn[actualShipIndex];
    }

    private void RemoveDeadShipFromQueue(Ship deadShip)
    {
        turnQueue.Remove(deadShip);
        shipsTurn.Remove(deadShip);
    }

    public void CleanupDeadShip(Ship ship)
    {
        if (ship.gameObject.CompareTag("Pirate"))
        {
            pirateShips.Remove(ship);
        }
        else
        {
            playerShips.Remove(ship);
        }
        //shipsTurn.Remove(ship);

        //turnQueue.Remove(ship);

        Destroy(ship.gameObject);

        // Check for win condition after removing a ship
        CheckForWinCondition();
    }

    public List<Ship> GetPlayerShips()
    {
        // Does this give a reference to the internal list???
        // Editing the list returned by this method causes big trouble ._. 
        // (will remove this method asap since it's only used in one place)
        return playerShips;
    }

    public void DisableSkipButton()
    {
        skipButton.SetActive(false);
    }

    public List<Vector3> GetShipWorldPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        foreach (Ship ship in pirateShips)
        {
            positions.Add(ship.transform.position);
        }
        foreach (Ship ship in playerShips)
        {
            positions.Add(ship.transform.position);
        }
        return positions;
    }

    public List<Vector3> GetPlayerShipWorldPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        foreach (Ship ship in playerShips)
        {
            positions.Add(ship.transform.position);
        }
        return positions;
    }

    public List<Vector3> GetPirateShipWorldPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        foreach (Ship ship in pirateShips)
        {
            positions.Add(ship.transform.position);
        }
        return positions;
    }

    public List<Vector3Int> GetPirateShipOffsetPositions()
    {
        List<Vector3Int> positions = new List<Vector3Int>();
        foreach (Ship ship in pirateShips)
        {
            positions.Add(ship.hexCoord);
        }
        return positions;
    }

    public List<Vector3Int> GetPlayerShipOffsetPositions()
    {
        List<Vector3Int> positions = new List<Vector3Int>();
        foreach (Ship ship in playerShips)
        {
            positions.Add(ship.hexCoord);
        }
        return positions;
    }

    public List<Ship> GetPirateShips()
    {
        return pirateShips;
    }

    private void DisplayShipList(List<Ship> list)
    {
        Debug.Log("the list " + list.ToString()+(" contains:"));
        for(int i=0; i<list.Count; i++)
        {
            Debug.Log(list[i].name);
        }
    }

    public Ship GetActualShip()
    {
        return shipsTurn[actualShipIndex];
    }

}

//Utitlities for lists
public static class ListExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        System.Random rnd = new System.Random();
        for (var i = 0; i < list.Count; i++)
            list.Swap(i, rnd.Next(i, list.Count));
    }

    public static void Swap<T>(this IList<T> list, int i, int j)
    {
        var temp = list[i];
        list[i] = list[j];
        list[j] = temp;
    }
}

public enum GameState
{
    PlayerMove,
    PlayerFire,
    PirateTurn,
    PirateFire,
    Upgrade,
    Victory,
    Defeat
}
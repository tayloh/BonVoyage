using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState state; 
    private bool isGameOver = false;
    public static event Action<GameState> OnGameStateChanged;

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

    private List<Ship> shipsTurn = new List<Ship>();
    private int actualShipIndex = 0;

    [SerializeField]
    private ShipManager shipManager;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        foreach(Ship ship in playerShipsParent.GetComponentsInChildren<Ship>())
        {
            playerShips.Add(ship);
        }
        foreach (Ship pship in pirateShipsParent.GetComponentsInChildren<Ship>())
        {
            pirateShips.Add(pship);
        }

        //Generating the random order of ships for both player and pirates
        PrepareTurn();
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
        new WaitForSecondsRealtime(1.0f);
        NextTurn();
        yield return null;
    }

    public void UpdateGameState(GameState newState)
    {
        state = newState;

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
                fireButton.SetActive(true);
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
                break;
            case GameState.Defeat:
                gameOverText.SetActive(true);
                break;
        }
        OnGameStateChanged?.Invoke(newState);
    }

    
    
    public void NextTurn()
    {
        Ship nextShip = GetNextShipForTurn();
        if (!nextShip.CompareTag("Pirate"))
        {
            UpdateGameState(GameState.PlayerMove);
            shipManager.StartPlayerTurn(nextShip);
        }
        else
        {
            UpdateGameState(GameState.PirateTurn);
            shipManager.MovePirateShip(nextShip);
        }
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

    private void DisplayShipList(List<Ship> list)
    {
        Debug.Log("the list " + list.ToString()+(" contains:"));
        for(int i=0; i<list.Count; i++)
        {
            Debug.Log(list[i].name);
        }
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
    Upgrade,
    Victory,
    Defeat
}


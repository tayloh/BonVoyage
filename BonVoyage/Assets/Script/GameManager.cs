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

    private List<Ship> playerShipsTurn = new List<Ship>();
    private List<Ship> pirateShipsTurn = new List<Ship>();
    private int actualPlayerShipIndex = -1;
    private int actualPirateShipIndex = -1;

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

        //Generating the random order of ships for both player and pirates
        PrepareTurn();
        //Starting the game with first turn of the player
        UpdateGameState(GameState.PlayerMove);
    }

    private void PrepareTurn()
    {
        playerShipsTurn.AddRange(playerShips);
        playerShipsTurn.Shuffle();
        pirateShipsTurn.AddRange(pirateShips);
        pirateShipsTurn.Shuffle();

        UpdateGameState(GameState.PlayerMove);
    }

    public void AddPlayerShip(Ship shipToAdd)
    {
        playerShips.Add(shipToAdd);
    }

    public void UpdateGameState(GameState newState)
    {
        state = newState;

        switch (newState)
        {
            case GameState.PlayerMove:
                Debug.Log("This is Player's turn to move");
                NextTurnPlayer();
                break;
            case GameState.PlayerFire:
                Debug.Log("This is Player's turn to fire");

                break;
            case GameState.PirateTurn:

                break;
            case GameState.Victory:

                break;
            case GameState.Defeat:

                break;
        }
        OnGameStateChanged?.Invoke(newState);
    }
    
    private void NextTurnPlayer()
    {//sets the next turn with one of the player's ship
        if(playerShipsTurn.Count > actualPlayerShipIndex)
        {
            shipManager.StartPlayerTurn(playerShipsTurn[actualPlayerShipIndex + 1]);
        }
        else
        {
            shipManager.StartPlayerTurn(playerShipsTurn[0]);
        }
    }

    private void NextTurnPirate()
    {

    }

    private List<Ship> TurnBothList()
    {
        List<Ship> turnShipsList = new List<Ship>();
        turnShipsList.AddRange(pirateShips);
        turnShipsList.AddRange(playerShips);
        turnShipsList.Shuffle(); //This shuffle randomly the list of all ships
        
        return turnShipsList;
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
    Victory,
    Defeat
}


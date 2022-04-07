using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState state; 
    private bool isGameOver = false;
    private List<Ship> playerShips = new List<Ship>();
    private List<Ship> pirateShips = new List<Ship>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {

        /*//testing turn list function
        Ship ship1 = new Ship();
        ship1.name = "ship1";
        playerShips.Add(ship1);
        Ship ship2 = new Ship();
        ship2.name = "ship2";
        playerShips.Add(ship2);

        Ship ship3 = new Ship();
        ship3.name = "ship3";
        pirateShips.Add(ship3);
        Ship ship4 = new Ship();
        ship4.name = "ship4";
        pirateShips.Add(ship4);

        DisplayShipList(playerShips);
        DisplayShipList(pirateShips);
        DisplayShipList(TurnList());
        DisplayShipList(playerShips);
        DisplayShipList(pirateShips);*/
    }

    public void AddPlayerShip(Ship shipToAdd)
    {
        playerShips.Add(shipToAdd);
    }

    private List<Ship> TurnList()
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
    PlayerTurn,
    PirateTurn,
    Victory,
    Defeat
}

//Elements to connect to the script:
public class Ship
{
    private int health;
    private bool isAlive;
    //for debugging only:
    public string name;
}

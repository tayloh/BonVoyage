using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnQueue : MonoBehaviour
{
    private Dictionary<Ship,ShipCard> cardsDict = new Dictionary<Ship, ShipCard>();
    [SerializeField]
    private GameObject shipCardPrefab;
    private float panelLength;
    public float offsetBetweenCards = 2;
    private float cardSize = 100;

    // Start is called before the first frame update
    void Start()
    {
        panelLength = this.GetComponent<RectTransform>().rect.width;
    }

    public void CreateCards(List<Ship> list)
    {
        int numberOfShips = list.Count;
        cardSize = panelLength / numberOfShips - offsetBetweenCards;
        for(int i =0; i<numberOfShips; i++)
        {
            GameObject go = Instantiate(shipCardPrefab, this.transform);
            ShipCard shipCard = go.GetComponent<ShipCard>();
            shipCard.Ship = list[i];
            cardsDict.Add(list[i], shipCard);
            shipCard.SetInitialAspect(offsetBetweenCards, cardSize, i);
            //go.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, (cardSize + offsetBetweenCards) * i, cardSize);
        }
    }

    public void UpdatePanel(List<Ship> list, int index)
    {
        foreach(ShipCard card in cardsDict.Values)
        {
            card.Translate(panelLength, list.Count - 1);
        }
    }

    internal void Remove(Ship ship)
    {
        throw new NotImplementedException();
        //When a ship is sunk, remove the card in the queue and fill the gap in the queue
        //TODO
        //
        //
        //
    }
}

public enum ShipType
{
    TreasureShip,
    Brig,
    Frigate,
    ShipOfTheLine
}
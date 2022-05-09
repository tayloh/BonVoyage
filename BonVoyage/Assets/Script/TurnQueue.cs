using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnQueue : MonoBehaviour
{
    private Dictionary<Ship,ShipCard> cardsDict = new Dictionary<Ship, ShipCard>();
    [SerializeField]
    private GameObject shipCardPrefab;
    [SerializeField]
    private GameObject queueCardModel;
    private GameObject queueCard;
    private Queue<ShipCard> queue;
    private TextMeshProUGUI queueText;
    private float panelLength;
    public float offsetBetweenCards = 2;    //the distance between 2 cards
    private float cardSize = 100;
    int numberOfVisibleCards;               //the number of  cards actually being displayed
    private int cardNumberMax = 10;         //the max number of cards we can display on the panel, the others will be in queue
    private float cardSizeMin = 100;    
    private float cardSizeMax = 140;

    // Start is called before the first frame update
    void Start()
    {
        panelLength = this.GetComponent<RectTransform>().rect.width;
    }

    public void CreateCards(List<Ship> list)
    {
        int numberOfShips = list.Count;
        numberOfVisibleCards = Mathf.Min(numberOfShips, cardNumberMax);
        cardSize = panelLength / numberOfShips - offsetBetweenCards;
        cardSize = Mathf.Min(Mathf.Max(cardSize, cardSizeMin), cardSizeMax);
        offsetBetweenCards = panelLength / numberOfVisibleCards - cardSize;        
        ShipCard.CardNumberMax = numberOfVisibleCards;
        ShipCard.Offset = offsetBetweenCards;
        ShipCard.Length = cardSize;
        //TODO : compute the sizeof the cards and the max number of cards given the number of ships and panel length/screen resolution
        
        
        for(int i =0; i<numberOfVisibleCards-1; i++)
        {
            GameObject go = Instantiate(shipCardPrefab, this.transform);
            ShipCard shipCard = go.GetComponent<ShipCard>();
            cardsDict.Add(list[i], shipCard);
            shipCard.Ship = list[i];
            shipCard.SetInitialAspect(i);
        }
        if (numberOfVisibleCards < numberOfShips)
        {
            CreateQueue();
            for (int j = numberOfVisibleCards - 1; j < numberOfShips; j++)
            {
                GameObject go = Instantiate(shipCardPrefab, this.transform);
                ShipCard shipCard = go.GetComponent<ShipCard>();
                cardsDict.Add(list[j], shipCard);
                shipCard.Ship = list[j];
                shipCard.SetInitialAspect(j);
                shipCard.RectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, panelLength - cardSize - offsetBetweenCards / 2f, cardSize);
                AddToQueue(shipCard);
            }
        }
        else if (numberOfVisibleCards == numberOfShips)//display the last card instead of the queue card
        {
            GameObject go = Instantiate(shipCardPrefab, this.transform);
            ShipCard shipCard = go.GetComponent<ShipCard>();
            cardsDict.Add(list[numberOfVisibleCards - 1], shipCard);
            shipCard.Ship = list[numberOfVisibleCards - 1];
            shipCard.SetInitialAspect(numberOfVisibleCards - 1);
        }
    }


    private void CreateQueue()
    {
        queue = new Queue<ShipCard>();
        queueCard = Instantiate(queueCardModel, transform);  
        queueText = queueCard.GetComponentInChildren<TextMeshProUGUI>();      
        queueCard.transform.GetChild(0).GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, panelLength - cardSize - offsetBetweenCards / 2f, cardSize);
        
    }
    private void AddToQueue(ShipCard shipCard)
    {
        queue.Enqueue(shipCard);
        queueText.text = "+" + queue.Count.ToString();
        shipCard.Image.enabled = false;
        shipCard.Background.enabled = false;
    }

    public void UpdatePanel(List<Ship> list, int index)
    {        
        for (int i = index; i<index + numberOfVisibleCards-1; i++)
        {
            ShipCard card = cardsDict[list[i % list.Count]];
            card.rank = i - index+1;
            StartCoroutine(cardsDict[list[i % list.Count]].MoveLeft());
        }
        if(queue != null)
        {
            UpdateQueue(cardsDict[list[(index == 0) ? list.Count - 1 : index - 1]]);
        }
        else
        {
            ShipCard card = cardsDict[list[(index == 0)? list.Count - 1 : index - 1]];
            StartCoroutine(card.MoveBackToRight(panelLength - cardSize - offsetBetweenCards / 2f, numberOfVisibleCards, cardSize));
            //card.rank = numberOfVisibleCards-1;
            //card.RectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, panelLength - cardSize - offsetBetweenCards / 2f, cardSize);
        }
    }


    private void UpdateQueue(ShipCard cardToEnqueue)
    {
        //bring the first card into the queue
        queue.Enqueue(cardToEnqueue);
        cardToEnqueue.RectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, panelLength - cardSize - offsetBetweenCards / 2f, cardSize);
        cardToEnqueue.Image.enabled = false;
        cardToEnqueue.Background.enabled = false;
        cardToEnqueue.rank = cardsDict.Count;
        
        //reveal the first card in the queue
        ShipCard newCard = queue.Dequeue();
        newCard.Image.enabled = true;
        newCard.Background.enabled = true;
        StartCoroutine(newCard.MoveLeft());
        //updtae the ranks 
        foreach(ShipCard card in queue)
        {
            card.rank -= 1;
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
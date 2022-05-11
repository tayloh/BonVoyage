using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnQueue : MonoBehaviour
{
    private Dictionary<Ship, ShipCard> cardsDict = new Dictionary<Ship, ShipCard>();
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
    private bool removeInProgress = false;

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

        for (int i = 0; i < numberOfVisibleCards - 1; i++)
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
        StartCoroutine(WaitForEndOfRemove(list, index));        
    }

    private IEnumerator WaitForEndOfRemove(List<Ship> list, int index)
    {
        yield return new WaitUntil(() => (removeInProgress == false));
        for (int i = index; i < index + numberOfVisibleCards - 1; i++)
        {
            ShipCard card = cardsDict[list[i % list.Count]];
            card.rank = i - index + 1;
            StartCoroutine(cardsDict[list[i % list.Count]].MoveLeft());
        }
        if (queue != null)
        {
            UpdateQueue(cardsDict[list[(index == 0) ? list.Count - 1 : index - 1]]);
        }
        else
        {
            ShipCard card = cardsDict[list[(index == 0) ? list.Count - 1 : index - 1]];
            StartCoroutine(card.MoveBackToRight(panelLength - cardSize - offsetBetweenCards / 2f, numberOfVisibleCards, cardSize));
        }
    }

    private void UpdateQueue(ShipCard cardToEnqueue)
    {
        //bring the first card into the queue
        queue.Enqueue(cardToEnqueue);
        cardToEnqueue.RectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, panelLength - cardSize - offsetBetweenCards / 2f, cardSize);
        cardToEnqueue.Image.enabled = false;
        cardToEnqueue.Background.enabled = false;
        cardToEnqueue.rank = cardsDict.Count+1;//will be updated -1 just after

        //reveal the first card in the queue
        ShipCard newCard = queue.Dequeue();
        newCard.Image.enabled = true;
        newCard.Background.enabled = true;
        StartCoroutine(newCard.MoveLeft());
        //updtae the ranks 
        foreach (ShipCard card in queue)
        {
            card.rank -= 1;
        }
    }
    
    internal void Remove(Ship sunkShip)
    {
        removeInProgress = true;
        //When a ship is sunk, remove the card in the queue and fill the gap in the queue
        ShipCard sunkCard = cardsDict[sunkShip];
        //get the rank of the sunk ships
        //translate all cards behind this rank to fill the gap (/!\ DO NOT translate cards in queue)
        if (cardsDict.Count <= numberOfVisibleCards)//when there is no stack
        {
            foreach (ShipCard card in cardsDict.Values)
            {
                //Update the ranks
                if (card.rank > sunkCard.rank)
                {
                    card.rank -= 1;
                    StartCoroutine(card.MoveLeft());
                }
            }
        }    
        else
        {
            //if there is a stack
            foreach (ShipCard card in cardsDict.Values)
            {
                //Update the ranks
                if (card.rank > sunkCard.rank)
                {
                    card.rank -= 1;
                    if (card.Image.enabled == true) //if not in queue
                    {
                        StartCoroutine(card.MoveLeft());
                    }
                }
            }
            //check if dead ship is in queue, remove it 
            bool shipRemoved = RemoveFromQueue(sunkCard);
            if(shipRemoved == false) //if the ship was not in queue, dequeue the first card in queue
            {
                ShipCard newCard = queue.Dequeue();
                newCard.Image.enabled = true;
                newCard.Background.enabled = true;
                StartCoroutine(newCard.MoveLeft());
            }
            queueText.text = "+" + queue.Count.ToString();
            //If no more cards in queue, delete the queue card and replace it with the last one
            if (queue.Count == 1)
            {
                ShipCard lastCard = queue.Dequeue();
                lastCard.Image.enabled = true;
                lastCard.Background.enabled = true;
                queue = null;
                queueCard.SetActive(false);
            }
        }
        
        //remove from dict, destroy the card
        cardsDict.Remove(sunkShip);
        Destroy(sunkCard.gameObject);
        if (cardsDict.Count < numberOfVisibleCards)
        {
            numberOfVisibleCards -= 1; //does not do anything here alone
        }
        removeInProgress = false;
        //if necessary, update numberOfVisibleCards (should also change the position of the last card)
    }

    private bool RemoveFromQueue(ShipCard sunkCard)
    {
        if (queue.Contains(sunkCard))
        {
            List<ShipCard> list = new List<ShipCard>(queue);
            list.Remove(sunkCard);
            queue = new Queue<ShipCard>(list);
            return true;
        }
        return false;
    }
}

public enum ShipType
{
    TreasureShip,
    Brig,
    Frigate,
    ShipOfTheLine
}
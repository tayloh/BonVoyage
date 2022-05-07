using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipCard : MonoBehaviour
{
    private Image image;
    private Ship ship;
    public Ship Ship { set => ship = value; get => ship; }
    [SerializeField]
    private int rank;
    private RectTransform rectTransform;
    private float length = 20f;
    private float offset = 10f;
    public float translationDuration = 2;

    private void Awake()
    {
        image = GetComponent<Image>();
        //TODO set rank
        //
        //
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetInitialAspect(float offsetBetweenCards, float cardSize, int rankImg)
    {
        rank = rankImg;
        length = cardSize;
        offset = offsetBetweenCards;
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, offset/2f +(length + offset) * rank, length);
        
        //TODO : Define a ship type in the ship class and use it according to the 3d model of the ship
        /*switch (shipType)
        {
            case ShipType.Brig:
                image = Resources.Load<Image>("Brig.png");
                break;
            case ShipType.Frigate:
                image = Resources.Load<Image>("Frigate.png");
                break;
            case ShipType.ShipOfTheLine:
                image = Resources.Load<Image>("ShipOfTheLine.png");
                break;
            case ShipType.TreasureShip:
                image = Resources.Load<Image>("TreasureShip.png");
                break;
        }*/
    }

    public void Translate(float panelLength, int maxRank)
    {
        if(rank == 0)
        {
            //goes back to the end of the queue
            StartCoroutine(BackToQueue(panelLength));
            rank = maxRank;
        }
        else
        {
            StartCoroutine(MoveLeft());
            rank -= 1;
        }
    }

    private IEnumerator BackToQueue(float panellength)
    {
        float endPos = panellength - length - offset/2f;
        image.enabled = false;
        yield return new WaitForSeconds(translationDuration);
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, endPos, length);
        image.enabled = true;
    }

    private IEnumerator MoveLeft()
    {
        float startPos = offset / 2f + (length + offset) * rank;
        float endPos = startPos - (length + offset);
        float timeElapsed = 0;
        float lerpstep = 0;
        while(timeElapsed < translationDuration)
        {
            timeElapsed += Time.deltaTime;
            lerpstep = timeElapsed / translationDuration;
            rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, startPos*(1f-lerpstep)+endPos*lerpstep, length);
            yield return null;
        }
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, endPos, length);
    }
}

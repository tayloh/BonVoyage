using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipCard : MonoBehaviour
{
    private Image image;
    private Image background;
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
        image = transform.GetChild(0).GetComponent<Image>();
        background = GetComponent<Image>();
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
        
        switch (ship._shipType)
        {
            case ShipType.Brig:
                image.sprite = Resources.Load<Sprite>("ShipTypesImages/Brig");
                break;
            case ShipType.Frigate:
                image.sprite = Resources.Load<Sprite>("ShipTypesImages/Frigate");
                break;
            case ShipType.ShipOfTheLine:
                image.sprite = Resources.Load<Sprite>("ShipTypesImages/ShipOfTheLine");
                break;
            case ShipType.TreasureShip:
                image.sprite = Resources.Load<Sprite>("ShipTypesImages/TreasureShip");
                break;
            default:
                throw new Exception("Type of ship not supported");
                break;
        }
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
        background.enabled = false;
        yield return new WaitForSeconds(translationDuration);
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, endPos, length);
        background.enabled = true;
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

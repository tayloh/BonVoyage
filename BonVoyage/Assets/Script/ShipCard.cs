using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
[SelectionBase]
public class ShipCard : MonoBehaviour
{
    private Image image;
    public Image Image { get => image; set => image = value; }
    private Image background;
    public Image Background { get => background; set => background = value; }
    [SerializeField]
    private Ship ship;
    public Ship Ship { set => ship = value; get => ship; }
    [SerializeField]
    public int rank;
    private RectTransform rectTransform;
    public RectTransform RectTransform { get => rectTransform; }
    static private float length = 20f;
    static public float Length { set => length = value; get => length; }
    static private float offset = 10f;
    static public float Offset { get => offset; set => offset = value; }

    public float translationDuration = 2;
    static private int cardNumberMax;
    static public int CardNumberMax { get => cardNumberMax; set => cardNumberMax = value; }

    static private Queue<ShipCard> queue = new Queue<ShipCard>();

    private void Awake()
    {
        image = transform.GetChild(0).GetComponent<Image>();
        background = GetComponent<Image>();
        //TODO set rank
        //
        //
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetInitialAspect(int rankImg)
    {
        rank = rankImg;
        if(rankImg <cardNumberMax)
        {
            rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, offset / 2f + (length + offset) * rank, length);
        }
        else //put in queue
        {
            Queue();
        }

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

    private void Queue()
    {
        queue.Enqueue(this);

    }

    public IEnumerator MoveBackToRight(float finalPos, int numberOfVisibleCards, float cardSize)
    {
        yield return new WaitForFixedUpdate();
        rank = numberOfVisibleCards - 1;
        image.enabled = false;
        Background.enabled = false;
        yield return new WaitForSeconds(translationDuration);
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, finalPos, cardSize);
        image.enabled = true;
        Background.enabled = true;
    }

    public IEnumerator BackToQueue(float panellength)
    {
        float endPos = panellength - length - offset/2f;
        image.enabled = false;
        background.enabled = false;
        yield return new WaitForSeconds(translationDuration);
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, endPos, length);
        background.enabled = true;
        image.enabled = true;
    }

    public IEnumerator MoveLeft()
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

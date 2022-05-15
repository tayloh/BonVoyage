using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

[SelectionBase]
public class ShipCard : MonoBehaviour
{
    private Image image;
    public Image Image { get => image; set => image = value; }
    private Image background;
    public Image Background { get => background; set => background = value; }
    [SerializeField]
    private Ship ship;
    private GlowHighlight shipHighlight;
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
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        //adding callback when mouse pointer enters the image, OnPointerEnter()
        var pEnter = new EventTrigger.Entry();
        pEnter.eventID = EventTriggerType.PointerEnter;
        pEnter.callback.AddListener((eventData) => { OnPointerEnter(); });
        this.GetComponent<EventTrigger>().triggers.Add(pEnter);

        //adding callback when mouse pointer enters the image, OnPointerExit()
        var pExit = new EventTrigger.Entry();
        pExit.eventID = EventTriggerType.PointerExit;
        pExit.callback.AddListener((eventData) => { OnPointerExit(); });
        this.GetComponent<EventTrigger>().triggers.Add(pExit);
    }

    public void SetInitialAspect(int rankImg)
    {
        rank = rankImg+1;
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
        shipHighlight = ship.GetComponent<GlowHighlight>();
    }

    private void Queue()
    {
        queue.Enqueue(this);

    }

    public IEnumerator MoveBackToRight(float finalPos, int numberOfVisibleCards, float cardSize)
    {
        rank = numberOfVisibleCards;
        image.enabled = false;
        Background.enabled = false;
        yield return new WaitForSeconds(translationDuration);
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, offset / 2f + (length + offset) * (numberOfVisibleCards-1), cardSize);
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

    private void OnMouseEnter()
    {
        ship.GetComponent<GlowHighlight>().ToggleGlow();
    }

    private void OnPointerEnter()
    {
        //shipHighlight.ToggleGlow();
        ship.Tile.GetComponent<GlowHighlight>().DisplayAsQueueCard();
    }

    private void OnPointerExit()
    {
        //shipHighlight.ToggleGlow();
        ship.Tile.GetComponent<GlowHighlight>().ResetHighlight();
    }
}

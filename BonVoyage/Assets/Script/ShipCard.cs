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
    static private float horizontalOffset = 1;
    private int rank;
    private RectTransform rectTransform;

    private void Awake()
    {
        image = GetComponent<Image>();
        //TODO set rank
        //
        //
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetInitialAspect(float offsetBetweenCards, float cardSize, int rank)
    {
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, (cardSize + offsetBetweenCards) * rank, cardSize);
    }


}

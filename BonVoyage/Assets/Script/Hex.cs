using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Hex : MonoBehaviour
{
    [SerializeField]
    private GlowHighlight highlight;

    private HexCoordinates hexCoordinates;

    [SerializeField]
    private Ship ship;
    public Ship Ship
    {
        get
        { return ship; }
        set
        { ship = value; }
    }

    public Vector3Int HexCoords => hexCoordinates.GetHexCoords();

    [SerializeField]
    private HexType hexType;

    public int GetCost()
    {
        switch (hexType)
        {
            case HexType.Water:
                return 1;
                
            case HexType.Stormy:
                return 2;
                
            case HexType.Obstacle:
                return 10;
                
            default:
                throw new Exception($"Hex of type {hexType} not supported.");
        }
    }

    public bool IsObstacle()
    {
        return hexType == HexType.Obstacle;
    }

    private void Awake()
    {
        hexCoordinates = GetComponent<HexCoordinates>();
        highlight = GetComponent<GlowHighlight>();
    }

    public void EnableHighLight()
    {
        highlight.ToggleGlow(true);
    }

    public void DisableHighlight()
    {
        highlight.ToggleGlow(false);
    }
}

public enum HexType
{
    None,
    Water, // default
    Stormy,
    Obstacle
}

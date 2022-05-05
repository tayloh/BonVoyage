using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Hex : MonoBehaviour
{
    [SerializeField]
    private GlowHighlight highlight;
    [SerializeField]

    private HexCoordinates hexCoordinates;

    public bool moveHereCursor = false;

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

    public PlayerInput playerInput;
    public PlayerInput PlayerInput { set => playerInput = value; }

    public HexType HexType { set => hexType = value; }

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
        //highlight.ToggleGlow(true);
        highlight.DisplayDefaultGlow();
    }

    public void DisableHighlight()
    {
        //highlight.ToggleGlow(false);
        highlight.ResetHighlight();
    }

    public void HighlightHexOfFiringArc(string tag)
    {
        switch (tag)
        {
            case "Pirate":
                //highlight.ToggleGlow(false);
                highlight.DisplayAsPirateFiringArc();
                break;
            case "PlayerShip":
                //highlight.ToggleGlow(false);
                highlight.DisplayAsPlayerFiringArc();
                break;
            default:
                throw new Exception("tag non accepted");
        }
    }

    public void EnableHighlightInvalid()
    {
        highlight.ToggleGlowInvalid(true);
    }

    internal void ResetHighlight()
    {
        highlight.ResetGlowHighlight();
    }

    public void DisableHighlightInvalid()
    {
        highlight.ToggleGlowInvalid(false);
    }

    internal void HighlightPath()
    {
        highlight.HighlightValidPath();
    }

    private void OnMouseOver()
    {
        if(moveHereCursor && !CameraMovement.isMoving)
        {
            playerInput.UpdateCursor(CursorState.MoveHere);
        }        
    }

    private void OnMouseExit()
    {
        playerInput.UpdateCursor(CursorState.General);
    }
}

public enum HexType
{
    None,
    Water, // default
    Stormy,
    Obstacle
}

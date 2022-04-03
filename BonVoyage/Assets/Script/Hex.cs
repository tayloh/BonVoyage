using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Hex : MonoBehaviour
{
    private HexCoordinates hexCoordinates;

    public Vector3Int HexCoords; 
    private void Awake()
    {
        hexCoordinates = GetComponent<HexCoordinates>();
        HexCoords = hexCoordinates.GetHexCoords();
    }
}

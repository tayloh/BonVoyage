using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCoordinates : MonoBehaviour
{
    //Offset between hex tiles, can be checked in Edit->Grid and snap settings
    private static float xOffset = 2;
    private static float yOffset = 1;

    private static float zOffset = 1.73f;

    [Header("Offset coordinates")]
    [SerializeField]
    private Vector3Int offsetCoordinates;

    public Vector3Int GetHexCoords()
    {
        return offsetCoordinates;
    }

    private void Awake()
    {
        offsetCoordinates = ConvertPositionToOffset(transform.position);
    }    

    public static Vector3Int ConvertPositionToOffset(Vector3 position)
        //convert actual position to int coordinates
    {        
        int x = Mathf.CeilToInt(position.x / xOffset);
        int y = Mathf.RoundToInt(position.y / yOffset);
        int z = Mathf.RoundToInt(position.z / zOffset);
        return new Vector3Int(x, y, z);
    }
    public static Vector3Int ConvertVectorToOffset(Vector3 vector)
    {
        if(vector == new Vector3(-1,0,0))
        {
            return new Vector3Int(-1, 0, 0); //exception 
        }
        int x = Mathf.CeilToInt(vector.x / xOffset);
        int y = Mathf.RoundToInt(vector.y / yOffset);
        int z = Mathf.RoundToInt(vector.z / zOffset);
        return new Vector3Int(x, y, z);
    }
}

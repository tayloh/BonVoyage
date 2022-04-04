using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphSearch : MonoBehaviour
{
   /* public static BFSResult BFSGetRange(HexGrid hexGrid, Vector3Int startPoint, Vector3Int forwardStart, int movementPoints)
    {
        Dictionary<Vector3Int, Vector3Int?> visitedNodes = new Dictionary<Vector3Int, Vector3Int?>();
        Dictionary<Vector3Int, int> costSoFar = new Dictionary<Vector3Int, int>();
        Queue<Vector3Int> nodesToVisitQueue = new Queue<Vector3Int>();

        nodesToVisitQueue.Enqueue(startPoint);
        costSoFar.Add(startPoint, 0);
        visitedNodes.Add(startPoint, null);

        while (nodesToVisitQueue.Count > 0)
        {
            Vector3Int currentNode = nodesToVisitQueue.Dequeue();
        }
    }*/
}

public struct BFSResult
{

}

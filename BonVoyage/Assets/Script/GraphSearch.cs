using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GraphSearch : MonoBehaviour
{
    public static BFSResult BFSGetRange(HexGrid hexGrid, Vector3Int startPoint, int movementPoints)
    {
        Dictionary<Vector3Int, Vector3Int?> visitedNodes = new Dictionary<Vector3Int, Vector3Int?>();
        Dictionary<Vector3Int, int> costSoFar = new Dictionary<Vector3Int, int>();
        Queue<Vector3Int> nodesToVisitQueue = new Queue<Vector3Int>();

        nodesToVisitQueue.Enqueue(startPoint);
        costSoFar.Add(startPoint, 0);
        visitedNodes.Add(startPoint, null);
              
        Vector3 forward = hexGrid.GetTileAt(startPoint).Ship.gameObject.transform.forward;
        
        while (nodesToVisitQueue.Count > 0)
        {
            Vector3Int currentNode = nodesToVisitQueue.Dequeue();
            //TODO calculate forward
            
            foreach (Vector3Int neighbourPosition in hexGrid.GetAccessibleNeighboursFor(currentNode, forward)) //Forward is not updated, accurate for only 1 movement
            {
                if(hexGrid.GetTileAt(neighbourPosition).IsObstacle())
                {
                    continue;
                }
                int nodeCost = hexGrid.GetTileAt(neighbourPosition).GetCost();
                int currentCost = costSoFar[currentNode];
                int newCost = currentCost + nodeCost;

                if (newCost <= movementPoints)
                {
                    if (!visitedNodes.ContainsKey(neighbourPosition))
                    {
                        visitedNodes[neighbourPosition] = currentNode;
                        costSoFar[neighbourPosition] = newCost;
                        //nodesToVisitQueue.Enqueue(neighbourPosition); //not necessary if one tile at a time
                    }
                    else if (costSoFar[neighbourPosition]>newCost)
                    {
                        costSoFar[neighbourPosition] = newCost;
                        visitedNodes[neighbourPosition] = currentNode;
                    }
                }
            }
            //Update vector forward
            //this function does not return orientation of the ship => need to change the structure ==> update froward
        }
        return new BFSResult { visitedNodesDict = visitedNodes };
    }

    public static List<Vector3Int> GeneratePathBFS(Vector3Int current, Dictionary<Vector3Int, Vector3Int?> visitedNodesDict)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        path.Add(current);
        while (visitedNodesDict[current] != null)
        {
            path.Add(visitedNodesDict[current].Value);
            current = visitedNodesDict[current].Value;
        }
        path.Reverse();
        return path.Skip(1).ToList();
    }
}

public struct BFSResult
{
    public Dictionary<Vector3Int, Vector3Int?> visitedNodesDict;

    public List<Vector3Int> GetPathTo(Vector3Int destination) //Would require to also get orientation
    {
        if (visitedNodesDict.ContainsKey(destination) == false)
        {
            return new List<Vector3Int>();
        }
        return GraphSearch.GeneratePathBFS(destination, visitedNodesDict);
    }

    public bool IsHexPositionInRange(Vector3Int position)
    {
        return visitedNodesDict.ContainsKey(position);
    }

    public IEnumerable<Vector3Int> GetRangePositions()
    {
        /*foreach (KeyValuePair<Vector3Int, Vector3Int?> kvp in visitedNodesDict)
            Debug.Log("Key = "+kvp.Key +", Value = "+kvp.Value);*/
        return visitedNodesDict.Keys;
    }
}

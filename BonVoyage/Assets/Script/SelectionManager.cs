using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SelectionManager : MonoBehaviour
{

    [SerializeField]
    private Camera mainCamera;

    public LayerMask selectionMask;

    //public HexGrid hexGrid;

    //private List<Vector3Int> neighbours = new List<Vector3Int>();

    private Hex previousHighligthedHex;

    public UnityEvent<GameObject> OnShipSelected;
    public UnityEvent<GameObject> TerrainSelected;
    [SerializeField] private LayerMask hexLayermask;
    private GlowHighlight lastHexpointed;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);//Using Raycasting to perform a raycast out into the scene


        if (Physics.Raycast(ray, out hit, 1000, hexLayermask))
        {
            GlowHighlight highLight = hit.collider.GetComponent<GlowHighlight>();
            if (highLight.isGlowing) return;
            if (lastHexpointed != null && lastHexpointed != highLight)
            {
                lastHexpointed.DisableGlow();

            }
            lastHexpointed = highLight;
            lastHexpointed.EnableGlow();

        }

    }
    public void HandleClick(Vector3 mousePosition)
    {
        GameObject result;
        if (FindTarget(mousePosition, out result))
        {
            if (ShipSelected(result))
            {
                Debug.Log("Clic on ship detected");
                OnShipSelected?.Invoke(result);
            }
            else
            {
                Debug.Log("clic on terrain detected");
                TerrainSelected?.Invoke(result);
            }
        }
    }

    private void HandleClickDuringTurn(Vector3 mousePosition)
    {
        GameObject result;
        if (FindTarget(mousePosition, out result))
        {
            if (ShipSelected(result))
            {
                Debug.Log("Clic on ship detected");
                OnShipSelected?.Invoke(result);
            }
            else
            {
                Debug.Log("clic on terrain detected");
                TerrainSelected?.Invoke(result);
            }
        }
    }

    private bool ShipSelected(GameObject result)
    {
        return result.GetComponent<Ship>() != null;
    }

    private bool FindTarget(Vector3 mousePosition, out GameObject result)
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out hit, selectionMask))
        {
            result = hit.collider.gameObject;
            return true;
        }
        result = null;
        return false;
    }
}

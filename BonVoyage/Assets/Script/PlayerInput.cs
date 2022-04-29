using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInput : MonoBehaviour
{
    public UnityEvent<Vector3> PointerClick;
    [SerializeField]
    private Camera camera;
    [SerializeField]
    private Texture2D cursorAttackTarget;
    [SerializeField]
    private Texture2D cursorMoveHere;
    [SerializeField]
    private Texture2D cursorRotateCamera;
    [SerializeField]
    private Texture2D cursorSkipTurn;

    private void Update()
    {
        DetectMouseClick();
        if (Input.GetKeyDown(KeyCode.T))
        {
            UpdateCursor(CursorState.AttackTarget);
        }
    }

    private void DetectMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            PointerClick?.Invoke(mousePos);
        }
    }

    private void UpdateCursor(CursorState state)
    {
        switch (state)
        {
            case CursorState.General:
                Cursor.SetCursor(null, new Vector2(0,0), CursorMode.Auto);
                break;
            case CursorState.AttackTarget:
                Cursor.SetCursor(cursorAttackTarget, new Vector2(0, 0), CursorMode.Auto);
                Debug.Log("cursor updated ");
                break;
            case CursorState.MoveHere:

                break;
            case CursorState.RotateCamera:

                break;
            case CursorState.SkipTurn:

                break;
            default:
                new ArgumentNullException("state", "cursor state incorrect");
                break;
        }
    }
}

enum CursorState
{
    General,
    AttackTarget,
    MoveHere,
    RotateCamera,
    SkipTurn
}

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
    static private Texture2D cursorAttackTarget;
    [SerializeField]
    static private Texture2D cursorMoveHere;
    [SerializeField]
    static private Texture2D cursorRotateCamera;
    [SerializeField]
    static private Texture2D cursorSkipTurn;

    private void Awake()
    {
        cursorAttackTarget = Resources.Load<Texture2D>("CursorAttack");
        cursorMoveHere = Resources.Load<Texture2D>("CursorMove");
        cursorRotateCamera = Resources.Load<Texture2D>("CursorView");
        cursorSkipTurn = Resources.Load<Texture2D>("CursorSkip");
    }

    private void Update()
    {
        DetectMouseClick();        
    }    

    private void DetectMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            PointerClick?.Invoke(mousePos);
        }
    }

    public void UpdateCursor(CursorState state)
    {
        switch (state)
        {
            case CursorState.General:
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                break;
            case CursorState.AttackTarget:
                Cursor.SetCursor(cursorAttackTarget, new Vector2(16, 16), CursorMode.Auto);
                break;
            case CursorState.MoveHere:
                Cursor.SetCursor(cursorMoveHere, new Vector2(16, 32), CursorMode.Auto);
                break;
            case CursorState.RotateCamera:
                Cursor.SetCursor(cursorRotateCamera, new Vector2(16, 16), CursorMode.Auto);
                break;
            case CursorState.SkipTurn:
                Cursor.SetCursor(cursorSkipTurn, new Vector2(16, 16), CursorMode.Auto);
                break;
            default:
                new ArgumentNullException("state", "cursor state incorrect");
                break;
        }
    }
}

public enum CursorState
{
    General,
    AttackTarget,
    MoveHere,
    RotateCamera,
    SkipTurn
}

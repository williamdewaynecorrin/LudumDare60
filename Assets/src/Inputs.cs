using UnityEngine;

public static class Inputs
{
    public static float HorizontalMove()
    {
        float horiz = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) ? 1.0f : 0.0f;
        horiz += (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) ? -1.0f : 0.0f;
        return horiz;
    }

    public static float VerticalMove()
    {
        float vert = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) ? 1.0f : 0.0f;
        vert += (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) ? -1.0f : 0.0f;
        return vert;
    }

    public static bool MovementTriggered()
    {
        return Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.Space) ||
               Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.RightArrow);
    }

    public static bool JumpTriggered()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }

    public static bool RotateCameraHold()
    {
        return true;
    }

    public static float RotateCameraYAW()
    {
        return Input.mousePositionDelta.x;
    }

    public static float RotateCameraPitch()
    {
        return -Input.mousePositionDelta.y;
    }

    public static bool ToggleBoom()
    {
        return Input.GetMouseButtonDown(0);
    }
}

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
}

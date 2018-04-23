using UnityEngine;
using System.Collections;

public class Bounds : MonoBehaviour
{
    public Transform BottomLeft;
    public Transform TopRight;

    public Rect Rect
    {
        get
        {
            return Rect.MinMaxRect(
                BottomLeft.position.x, BottomLeft.position.y,
                TopRight.position.x, TopRight.position.y);
        }
    }

}

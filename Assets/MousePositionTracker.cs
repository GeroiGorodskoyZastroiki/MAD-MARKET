using UnityEngine;

public class MousePositionTracker : MonoBehaviour
{
    void Update()
    {
        var mousePosition = Input.mousePosition;
        mousePosition.z = -1;
        transform.position = Camera.main.ScreenToWorldPoint(mousePosition);
    }
}
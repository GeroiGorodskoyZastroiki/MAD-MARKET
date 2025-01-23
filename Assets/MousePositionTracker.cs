using UnityEngine;

public class MousePositionTracker : MonoBehaviour
{
    void Update()
    {
        if (!Application.isFocused) return;
        var mousePosition = Input.mousePosition;
        mousePosition.z = -1;
        transform.position = Camera.main.ScreenToWorldPoint(mousePosition);
    }
}
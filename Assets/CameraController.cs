using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    CinemachineTargetGroup targetGroup;
    
    public float playerWeight;
    public float mousePositionWeight;
    public float mousePositionFocusedWeight;
    
    private bool isCursorPriorityActive = false;

    private void Start() 
    {
        targetGroup = GetComponent<CinemachineTargetGroup>();
    }

    void Update()
    {
        targetGroup.m_Targets[1].weight = isCursorPriorityActive ? mousePositionFocusedWeight : mousePositionWeight;
    }

    public void SetCursorPriority(bool isActive)
    {
        isCursorPriorityActive = isActive;
    }
}
using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    private CinemachineTargetGroup targetGroup;
    
    [SerializeField] private float playerWeight;
    [SerializeField] private  float currentMousePositionWeight;
    [SerializeField] private float mousePositionWeight;
    [SerializeField] private float mousePositionFocusedWeight;
    [SerializeField] private float focusTime;

    private void Awake() 
    {
        targetGroup = GetComponent<CinemachineTargetGroup>();
        currentMousePositionWeight = mousePositionWeight;
    }

    void Update()
    {
        targetGroup.m_Targets[1].weight = currentMousePositionWeight;
    }

    public void SetFocused(bool isActive)
    {
        float targetWeight = isActive ? mousePositionFocusedWeight : mousePositionWeight;

        DOTween.To(() => currentMousePositionWeight, 
                   x => currentMousePositionWeight = x, 
                   targetWeight, 
                   focusTime);
    }
}
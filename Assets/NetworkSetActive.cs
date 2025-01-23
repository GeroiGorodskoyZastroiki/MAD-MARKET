using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkSetActive : MonoBehaviour
{
    [SerializeField] private List<GameObject> objectsToDisable;
    [SerializeField] private List<MonoBehaviour> componentsToDisable;

    private void Start()
    {
        if (!GetComponentInParent<NetworkObject>().IsLocalPlayer)
        {
            foreach (var obj in objectsToDisable)
                if (obj != null) obj.SetActive(false);
            foreach (MonoBehaviour obj in componentsToDisable)
                if (obj != null) obj.enabled = false;
        }
    }
}

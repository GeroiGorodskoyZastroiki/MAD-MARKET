using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkNotOwnedRestrict : MonoBehaviour
{
    [SerializeField] private List<GameObject> objectsToDeactivate;
    [SerializeField] private List<MonoBehaviour> componentsToDisable;

    private void Start()
    {
        if (!GetComponent<NetworkObject>().IsLocalPlayer)
        {
            foreach (var go in objectsToDeactivate)
                if (go != null) go.SetActive(false);

            foreach (MonoBehaviour comp in componentsToDisable)
                if (comp != null) comp.enabled = false;
        }
    }
}

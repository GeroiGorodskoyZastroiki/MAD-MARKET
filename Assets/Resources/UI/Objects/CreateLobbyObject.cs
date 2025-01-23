using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CreateLobbyObject : MonoBehaviour
{
    private async void OnMouseDown() 
    {
        await SteamworksManager.Instance.StartHost(4);
    }
}

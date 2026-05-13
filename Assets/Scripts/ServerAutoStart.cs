using UnityEngine;
using FishNet;

public class ServerAutoStart : MonoBehaviour
{
    private void Start()
    {
        if (Application.isBatchMode)
        {
            InstanceFinder.ServerManager.StartConnection();
        }
    }
}

using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    public float Speed = 5;
    
    private PlayerNetwork _playerNetwork;

    public override void OnNetworkSpawn()
    {
        _playerNetwork = GetComponent<PlayerNetwork>();
    }

    private void Update()
    {
        if (!IsOwner || !IsSpawned) return;

        if (!_playerNetwork.IsAlive.Value) return;

        var multiplier = Speed * Time.deltaTime;
        
        if (Keyboard.current.aKey.isPressed)
        {
            transform.position += new Vector3(-multiplier, 0, 0);
        }
        if (Keyboard.current.dKey.isPressed)
        {
            transform.position += new Vector3(multiplier, 0, 0);
        }
        if (Keyboard.current.wKey.isPressed)
        {
            transform.position += new Vector3(0, 0, multiplier);
        }
        if (Keyboard.current.sKey.isPressed)
        {
            transform.position += new Vector3(0, 0, -multiplier);
        }
    }
}

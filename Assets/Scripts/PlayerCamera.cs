using Unity.Netcode;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] private Vector3 offset = new(0f, 8f, -6f);

    private Camera _cam;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }
        _cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (!_cam) return;
        _cam.transform.position = transform.position + offset;
        _cam.transform.LookAt(transform.position);
    }
}

using Unity.Netcode;

public class DisableLocalTag : NetworkBehaviour
{
    private void Start()
    {
        if (IsOwner) gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!IsOwner)
            gameObject.SetActive(true);
    }
}

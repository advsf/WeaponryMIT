using Unity.Netcode;
using UnityEngine;

public class HideLocalPlayer : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject playerObj;
    [SerializeField] private SpectatorMode spectator;
    [SerializeField] private Health healthScript;
    [SerializeField] private AnimationController animationControllerScript;

    private void Update()
    {
        // if the health is zero, disable for everyone
        if (healthScript.currentHealth <= 0f)
        {
            Renderer[] childRenderers = playerObj.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in childRenderers)
                renderer.enabled = false;

            return;
        }

        // allows other people to look at this player object
        if (!IsOwner || animationControllerScript.isDancing)
        {
            // enable all mesh renderers in the child objects to show the object to other players
            Renderer[] childRenderers = playerObj.GetComponentsInChildren<Renderer>();
            
            foreach (Renderer renderer in childRenderers)
                // assume there's always going to be a render for performance boost
                renderer.enabled = true;
        }

        // disable the body meshes
        else
        {
            // disable all mesh renderers in the child objects to hide the object from the local player's camera
            Renderer[] childRenderers = playerObj.GetComponentsInChildren<Renderer>();
            
            foreach (Renderer renderer in childRenderers)
                renderer.enabled = false;
        }
    }
}
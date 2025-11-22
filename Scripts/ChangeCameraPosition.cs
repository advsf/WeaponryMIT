using UnityEngine;
using Unity.Netcode;

public class ChangeCameraPosition : NetworkBehaviour
{
    [SerializeField] private AnimationController animationScript;

    // store the first person camera position and the third person camera positon
    [SerializeField] private Vector3 firstPersonPos;
    [SerializeField] private Vector3 thirdPersonPos;

    private void Start()
    {
        if (!IsOwner) return;

        // make the player third person if they're dancing so that they could see themselves dancing LOL
        if (animationScript.isDancing)
            transform.position = firstPersonPos;
        else
            transform.position = thirdPersonPos;
    }
}

using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private AnimationController animationScript;

    [Header("Pos")]
    [SerializeField] private Transform firstPersonPos;
    [SerializeField] private Transform thirdPersonPos;

    [Header("References")]
    [SerializeField] private MovementScript movement;

    private void Update()
    {
        // adjust the camera pos when dancing
        if (!animationScript.isDancing)
            transform.position = firstPersonPos.position;
        else if (animationScript.isDancing)
            transform.position = Vector3.Lerp(transform.position, thirdPersonPos.position, Time.deltaTime * 5f);

        // push the camera down a little bit while sliding to give enhanced feel
        if (movement.state == MovementScript.MovementState.sliding)
            transform.position = new(transform.position.x, transform.position.y * 0.9f, transform.position.z);
    }
}

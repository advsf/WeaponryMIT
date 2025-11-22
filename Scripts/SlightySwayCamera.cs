using UnityEngine;

public class SlightySwayCamera : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool hasAnimation = true;
    [SerializeField] private float swayAmount = 0.1f; 
    [SerializeField] private float swaySpeed = 1f;
    [SerializeField] private float moveSpeed;
    [SerializeField] private Vector3 initialPosition;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private float animatorDisableTime;

    private bool canMove = false;

    private void Start()
    {
        if (hasAnimation)
            Invoke(nameof(EnableAfterAnimation), animatorDisableTime);
        else
            canMove = true;
    }

    private void Update()
    {
        if (!canMove) return;

        // calculate sway amount using a sine wave
        float xSway = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
        float ySway = Mathf.Sin(Time.time * swaySpeed * 0.5f) * swayAmount * 0.5f;

        // apply transformations
        Vector3 swayPosition = initialPosition + new Vector3(xSway, ySway, 0f);
        transform.position = Vector3.Lerp(transform.position, swayPosition, moveSpeed * Time.deltaTime);
    }

    // because we have an animator for the camera
    private void EnableAfterAnimation()
    {
        animator.enabled = false;
        canMove = true;
    }
}


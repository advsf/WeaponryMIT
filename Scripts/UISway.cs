using UnityEngine;

public class UISway : MonoBehaviour
{
    [SerializeField] private Transform rectTransform;
    [SerializeField] private Camera cam;
    [SerializeField] private float xPosition;
    [SerializeField] private float yPosition;
    [SerializeField] private float zPosition;
    [SerializeField] private float lerpSpeed = 2.0f;

    void Update()
    {
        // this essentially makes the hud "lag back", or smoothly lerp to the correct position while the player turns if you know what i mean
        Vector3 targetPosition = cam.ViewportToWorldPoint(new Vector3(xPosition, yPosition, zPosition));

        rectTransform.position = Vector3.Lerp(rectTransform.position, targetPosition, lerpSpeed * Time.deltaTime);
    }
}

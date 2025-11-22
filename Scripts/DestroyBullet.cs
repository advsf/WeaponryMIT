using UnityEngine;

public class DestroyBullet : MonoBehaviour
{
    [SerializeField] private float timeBeforeDestruction;

    private void Start() => Invoke(nameof(DestroyBulletAfterCooldown), timeBeforeDestruction);

    private void DestroyBulletAfterCooldown() => Destroy(gameObject);

    private void OnCollisionEnter(Collision collision)
    {
        Invoke(nameof(DestroyBulletAfterCooldown), 0.025f);
    }
}
using UnityEngine;

public class NonPlayerHealth : MonoBehaviour
{
    [SerializeField] private float currentHealth;
    [SerializeField] private float maxHealth = 100f;

    public void DecreaseHealth(float damage) => currentHealth -= damage;

    private void Update()
    {
        if (currentHealth <= 0.0f)
        {
            currentHealth = maxHealth;

            RespawnBot();
        }
    }

    private void RespawnBot()
    {
        transform.position = new(Random.Range(6.65f, 19.2f), 0.5f, Random.Range(-16.5f, 15f));
        transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
    }
}

using UnityEngine;

public class AutoDeleteMessagePrefab : MonoBehaviour
{
    public void DeleteMessage() => Destroy(gameObject);
}

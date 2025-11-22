using System.Collections;
using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager instance;

    [Header("References")]
    [SerializeField] private GameObject transitionObj;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else
            Destroy(gameObject);
    }

    public IEnumerator PlayTransitionBackToLobby()
    {
        transitionObj.SetActive(false);
        transitionObj.SetActive(true);

        yield return new WaitForSeconds(1f);

        HandleDisconnections.instance.GoBackToLobby();

        yield return new WaitForSeconds(1f);

        transitionObj.SetActive(false);
    }
}

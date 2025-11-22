using UnityEngine;

public class RankManager : MonoBehaviour
{
    public static RankManager instance;

    public Sprite[] ranks;

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

    public int GetCurrentRankSprite(string rankName)
    {
        int count = 0;

        foreach (Sprite rank in ranks)
        {
            if (rank.name == rankName) return count;

            count++;
        }

        return 0;
    }
}

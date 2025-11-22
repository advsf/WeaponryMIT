using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HandlePlayerStatsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject windowed;
    [SerializeField] private GameObject full;

    [Header("UI References")]
    [SerializeField] private Image rankImage;
    [SerializeField] private Sprite[] rankSprites;
    [SerializeField] private Slider expSlider;
    [SerializeField] private GameObject allRanksGameObj;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI currentRankNameText;
    [SerializeField] private TextMeshProUGUI expCounter;
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private TextMeshProUGUI gamesWonText;
    [SerializeField] private TextMeshProUGUI gamesLostText;
    [SerializeField] private TextMeshProUGUI totalKillsText;
    [SerializeField] private TextMeshProUGUI totalDeathsText;

    [Header("Settings")]
    [SerializeField] private float expSliderSmoothFactor = 2f;

    private bool isAnimationPlaying = false; // used to check if the de/rankup animation is playing

    private void Start()
    {
        TurnOnWindowed();

        // handle initializing data
        if (!PlayerPrefs.HasKey("winCount"))
            PlayerPrefs.SetInt("winCount", 0);

        if (!PlayerPrefs.HasKey("loseCount"))
            PlayerPrefs.SetInt("loseCount", 0);

        if (!PlayerPrefs.HasKey("killsCount"))
            PlayerPrefs.SetInt("killsCount", 0);

        if (!PlayerPrefs.HasKey("deathsCount"))
            PlayerPrefs.SetInt("deathsCount", 0);

        if (!PlayerPrefs.HasKey("rankedExp"))
            PlayerPrefs.SetInt("rankedExp", 10);

        if (!PlayerPrefs.HasKey("currentRank"))
            PlayerPrefs.SetInt("currentRank", 0);

        rankImage.sprite = rankSprites[PlayerPrefs.GetInt("currentRank")];
        HandleSettingCurrentRankNameAndRankImagePos();

        allRanksGameObj.SetActive(false);
    }

    private void Update()
    {
        // while authenticating username dont allow the player to turn on the player stats menu
        if (LobbyUIManager.instance.usernameMenu.activeInHierarchy)
            TurnOnWindowed();

        // for exp bar animations
        if (full.activeInHierarchy)
            HandleExpBarRelatedAnimations();

        // limit values
        PlayerPrefs.SetInt("rankedExp", Mathf.Clamp(PlayerPrefs.GetInt("rankedExp"), 0, 100));
        PlayerPrefs.SetInt("currentRank", Mathf.Clamp(PlayerPrefs.GetInt("currentRank"), 0, 18)); // 18 is the highest rank (legend)
    }

    public void ResetData()
    {
        PlayerPrefs.SetInt("currentRank", 0);
        PlayerPrefs.SetInt("rankedExp", 10);
        PlayerPrefs.SetInt("killsCount", 0);
        PlayerPrefs.SetInt("deathsCount", 0);
        PlayerPrefs.SetInt("winCount", 0);
        PlayerPrefs.SetInt("loseCount", 0);
    }

    public void TurnOnPlayerStats()
    {
        windowed.SetActive(false);
        full.SetActive(true);

        SetUpTexts();
    }

    public void TurnOnWindowed()
    {
        windowed.SetActive(true);
        full.SetActive(false);

        // reset the bar progress, so that when the player opens the full gameobject again the bar progression animation can play 
        expSlider.value = 0;
    }

    public void TurnOnAllRanksObject()
    {
        allRanksGameObj.SetActive(true);
        full.SetActive(false);
        windowed.SetActive(true);
    }

    public void TurnOffAllRanksObject() => allRanksGameObj.SetActive(false);

    private void SetUpTexts()
    {
        usernameText.text = $"Username: {PlayerPrefs.GetString("username")}";
        gamesWonText.text = $"Games Won: {PlayerPrefs.GetInt("winCount")}";
        gamesLostText.text = $"Games Lost: {PlayerPrefs.GetInt("loseCount")}";
        totalKillsText.text = $"Total Kills: {PlayerPrefs.GetInt("killsCount")}";
        totalDeathsText.text = $"Total Deaths: {PlayerPrefs.GetInt("deathsCount")}";
    }

    private void HandleExpBarRelatedAnimations()
    {
        if (!isAnimationPlaying)
        {
            // smoothly lerp data values
            expSlider.value = Mathf.MoveTowards(expSlider.value, PlayerPrefs.GetInt("rankedExp"), expSliderSmoothFactor * Time.deltaTime);

            // if max rank
            if (PlayerPrefs.GetInt("currentRank") == RankManager.instance.ranks.Length - 1)
                expCounter.text = "Max rank! Congratulations!";
            else
                expCounter.text = $"{100 - expSlider.value} EXP away from {RankManager.instance.ranks[PlayerPrefs.GetInt("currentRank") + 1].name}";
        }

        // if rank down (only when the bar reaches 0 and the exp is 0 and the player isnt already at the lowest rank)
        if (expSlider.value <= 0 && expSlider.value == PlayerPrefs.GetInt("rankedExp") && PlayerPrefs.GetInt("currentRank") != 0 && !isAnimationPlaying)
        {
            StartCoroutine(PlayDerankAnimation());

            // decrease rank
            PlayerPrefs.SetInt("currentRank", PlayerPrefs.GetInt("currentRank") - 1);
            PlayerPrefs.SetInt("rankedExp", 90);
        }

        // if rank up (only when the bar reaches 100 and the exp is 100 and the player isnt already at the highest rank)
        if (expSlider.value >= 100 && expSlider.value == PlayerPrefs.GetInt("rankedExp") && PlayerPrefs.GetInt("currentRank") != RankManager.instance.ranks.Length - 1 && !isAnimationPlaying)
        {
            StartCoroutine(PlayRankUpAnimation());

            // increase rank
            PlayerPrefs.SetInt("currentRank", PlayerPrefs.GetInt("currentRank") + 1);
            PlayerPrefs.SetInt("rankedExp", 10);
        }
    }

    private IEnumerator PlayDerankAnimation()
    {
        float time = Time.time;
        float alpha = 1;
        isAnimationPlaying = true;

        while (Time.time - time <= 1)
        {
            rankImage.color = new(rankImage.color.r, rankImage.color.g, rankImage.color.b, alpha -= 0.01f);
            yield return null;
        }

        // reset time
        time = Time.time;

        // change sprite
        rankImage.sprite = RankManager.instance.ranks[PlayerPrefs.GetInt("currentRank")];

        // change text
        HandleSettingCurrentRankNameAndRankImagePos();

        // reset exp bar
        expSlider.value = 100;

        isAnimationPlaying = false;

        while (Time.time - time <= 1)
        {
            rankImage.color = new(rankImage.color.r, rankImage.color.g, rankImage.color.b, alpha += 0.01f);
            yield return null;
        }
    }

    private IEnumerator PlayRankUpAnimation()
    {
        float time = Time.time;
        float alpha = 1;
        isAnimationPlaying = true;

        while (Time.time - time <= 1)
        {
            rankImage.color = new(rankImage.color.r, rankImage.color.g, rankImage.color.b, alpha -= 0.01f);
            yield return null;
        }

        // reset time
        time = Time.time;

        // change sprite
        rankImage.sprite = RankManager.instance.ranks[PlayerPrefs.GetInt("currentRank")];
        
        // change text
        HandleSettingCurrentRankNameAndRankImagePos();

        // reset exp bar
        expSlider.value = 0;

        isAnimationPlaying = false;

        while (Time.time - time <= 1)
        {
            rankImage.color = new(rankImage.color.r, rankImage.color.g, rankImage.color.b, alpha += 0.01f);
            yield return null;
        }
    }

    private void HandleSettingCurrentRankNameAndRankImagePos()
    {
        currentRankNameText.text = rankImage.sprite.name.ToUpper();

        // handle changing the colors and change the rank image position
        if (rankImage.sprite.name.Contains("Bronze"))
        {
            currentRankNameText.color = new(0.4245283f, 0.2018983f, 0.0740922f);
            rankImage.rectTransform.localPosition = new(rankImage.rectTransform.localPosition.x, 200f);
        }

        // silver
        else if (rankImage.sprite.name.Contains("Silver"))
        {
            currentRankNameText.color = new(0.8018868f, 0.8018868f, 0.8018868f);
            rankImage.rectTransform.localPosition = new(rankImage.rectTransform.localPosition.x, 200f);
        }

        // gold
        else if (rankImage.sprite.name.Contains("Gold"))
        {
            currentRankNameText.color = new(0.8962264f, 0.8698792f, 0.1733268f);
            rankImage.rectTransform.localPosition = new(rankImage.rectTransform.localPosition.x, 200f);
        }

        // platinum
        else if (rankImage.sprite.name.Contains("Platinum"))
        {
            currentRankNameText.color = new(0, 0.6792453f, 0.5797614f);
            rankImage.rectTransform.localPosition = new(rankImage.rectTransform.localPosition.x, 200f);
        }

        // diamond
        else if (rankImage.sprite.name.Contains("Diamond"))
        {
            currentRankNameText.color = new(0.5467769f, 0.003921578f, 0.8313726f);
            rankImage.rectTransform.localPosition = new(rankImage.rectTransform.localPosition.x, 200f);
        }

        // star
        else if (rankImage.sprite.name.Contains("Star"))
        {
            currentRankNameText.color = new(0.8313726f, 0.06173592f, 0.003921578f);
            rankImage.rectTransform.localPosition = new(rankImage.rectTransform.localPosition.x, 215f);
        }

        // legend
        else if (rankImage.sprite.name.Contains("Legend"))
        {
            currentRankNameText.color = new(0.9245283f, 0.9029948f, 0.3881274f);
            rankImage.rectTransform.localPosition = new(rankImage.rectTransform.localPosition.x, 173f);
        }
    }
}

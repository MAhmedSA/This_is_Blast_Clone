using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Collections.Generic;
using System.Media;
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("GamePlay UI References")]
    [SerializeField] Image _progressionBar;
    [SerializeField] GameObject _winnerPanel;
    [SerializeField] GameObject _losePanel;
    [SerializeField] TextMeshProUGUI currentLeveltextWinStat;
    [SerializeField] TextMeshProUGUI currentLevelGamePlay;

    [Header("Reward Images References")]
    [SerializeField] Image RewardImage;
    [SerializeField] Image RewardImageFake;
    [SerializeField][Range(0,1)] float _precentageCompleted;
    [SerializeField] TextMeshProUGUI rewardTextProgression;

    [Header("LoadingPanel Variables")]

    [SerializeField] GameObject _loadingPanel;
    [SerializeField] Image _loadingBar;
    [SerializeField] Button _ContinueButton;

    [Header("Coins Setup")]
    public List<Transform> coins;           // Coins in the scene
    public Transform target;                // Target (like coin icon in UI)
    public float scaleDuration = 0.3f;
    public float moveDuration = 0.6f;
    public float delayBetweenCoins = 0.1f;

    public TextMeshProUGUI _coinsText;
    public int _currentCoins;
    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();

    private void Awake()
    {
        _ContinueButton.interactable = false;
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        _currentCoins=PlayerPrefs.GetInt("_currentCoins", 0);
        // Store initial positions for Coins
        foreach (var coin in coins)
        {
            originalPositions[coin] = coin.position;
        }

        _progressionBar.fillAmount = 0f;
        currentLevelGamePlay.text = GameManager.Instance.CurrentLevelData.levelName.ToString();
        currentLeveltextWinStat.text = GameManager.Instance.CurrentLevelData.levelName.ToString();
    }
    public void UpdateLevelTextInBoth()
    {
        currentLevelGamePlay.text = GameManager.Instance.CurrentLevelData.levelName.ToString();
        currentLeveltextWinStat.text = GameManager.Instance.CurrentLevelData.levelName.ToString();
    }
    public void UpdateProgressionBar(float progress)
    {
        _progressionBar.fillAmount = progress;

        if (_progressionBar.fillAmount >= 1) {
            ShowWinnerPanel();
        }
    }
    public void ShowWinnerPanel() {
        _winnerPanel.SetActive(true);
        _winnerPanel.transform.DOScale(Vector3.one, 2f)
            .OnComplete(
        () =>
        {

            GameManager.Instance.LevelUP();
            DOTween.To(() => _precentageCompleted, x => _precentageCompleted = x, 0.25f, 2f)

              .OnUpdate(() =>
              {

                      RewardImageFake.fillAmount = _precentageCompleted;
                      rewardTextProgression.text = Mathf.RoundToInt(_precentageCompleted * 100).ToString() + "%";


              }
            


              ) 
            
              
              .OnComplete(() =>

              {
                  //here you can call level up function after Coins Animation completed

                  PlayCoinAnimation();
                  _progressionBar.fillAmount = 0;
                  _ContinueButton.interactable = true;


              }
              
              
              
              );

        }


        );
    }

    void CloseWinnerPanel()
    {
            _winnerPanel.SetActive(false);
            _winnerPanel.transform.localScale = Vector3.zero;
    }
    public void ShowLosePanel()
    {
        _losePanel.SetActive(true);
        _losePanel.transform.DOScale(Vector3.one, 3f);
    }

    public void UpdateCurrentLevelText(int level)
    {
        currentLeveltextWinStat.text = "Level " + level.ToString();
    }

    public void OpenLoadingPanel() {
        _loadingPanel.SetActive(true);
        CloseWinnerPanel();
        UpdateLevelTextInBoth();
        DOTween.To(() => _loadingBar.fillAmount, x => _loadingBar.fillAmount = x, 1f, 2f).OnComplete(() =>
        {
            _loadingPanel.SetActive(false);
            _loadingBar.fillAmount = 0f;
        });
    }

    [ContextMenu("Play Coin Animation")] // to test in editor and show what happens
    public void PlayCoinAnimation()
    {
        StartCoroutine(AnimateCoins());
    }

    private IEnumerator<WaitForSeconds> AnimateCoins()
    {
        for (int i = 0; i < coins.Count; i++)
        {
            Transform coin = coins[i];
            AnimateCoin(coin);
            yield return new WaitForSeconds(delayBetweenCoins);
        }
    }

    private void AnimateCoin(Transform coin)
    {
        Vector3 startPos = originalPositions[coin];
        coin.localScale = Vector3.zero;
        coin.gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();

        seq.Append(coin.DOScale(1f, scaleDuration).SetEase(Ease.OutBack)); // Pop in
        seq.Append(coin.DOMove(target.position, moveDuration).SetEase(Ease.InQuad)); // Move to target
        seq.AppendInterval(0.1f);
        seq.AppendCallback(() =>
        {
            coin.gameObject.SetActive(false); // Disappear at target
            _currentCoins += 1;
            _coinsText.text = _currentCoins.ToString();
            AudioManager.Instance.PlaySound("CoinCollect");

            coin.position = startPos;          // Reset to original pos
            coin.localScale = Vector3.zero;    // Hide for next use
        });

        seq.Play();
    }
}

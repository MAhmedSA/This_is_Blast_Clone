using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Tooltip("Variable For Tutorial SingleTone")]
    public static TutorialManager instance;
    
    [Tooltip("Variable For UI Tutorial")]
    [SerializeField] GameObject uiTutorial;

    [Tooltip("Variable For UI ImageFade to control fade")]
    [SerializeField] Image _imageFade;
    
    [Tooltip("Variable For Hand Animator object")]
    [SerializeField] GameObject _handAnimator;
    
    [Tooltip("Variable For Fade Precentage")]
    [SerializeField] float _fadePrecentage=0.5f;

    [Tooltip("Variable bool if is completed")]
    [SerializeField] bool _isTutorialCompleted ;

    [Tooltip("Properity to access is tutorial or not!")]
    public bool IsTutorialCompleted { get => _isTutorialCompleted;}

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (GameManager.Instance.CurrentLevelData.isTutorial)
        {
            ShowTutorial();
        }
    }

    public void ShowTutorial()
    {
        uiTutorial.SetActive(true);
        Color color = _imageFade.color;
        color.a = _fadePrecentage;
        _imageFade.color = color;
       
        _handAnimator.SetActive(true);   
        _handAnimator.GetComponent<Animator>().Play("HandAnimation");
    }

    public void DisapleTutorial() {
        _isTutorialCompleted=true;
        uiTutorial.SetActive(false);
        Color color = _imageFade.color;
        color.a =0f;
        _imageFade.color = color;
        _imageFade.gameObject.SetActive(false);
        _handAnimator.GetComponent<Animator>().StopPlayback();
        _handAnimator.SetActive(false);

    }
}

using TMPro; 
using UnityEngine;
using UnityEngine.SceneManagement;


public class LoadingManager : MonoBehaviour
{
    //variable to save data container for loading SceneDataBase
    public SceneDataBase sceneDataBase;
    //Variable to check if level is completed or not
    public bool  levelIsCompleted=false;
    // variable to track current level
    public int currentLevel;
    //variable to text of Current Level
    [SerializeField] TextMeshProUGUI currentLevelText;
   

    // Singleton Instance
    public static LoadingManager Instance;
    
    private void Awake()
    {
        Application.targetFrameRate = 60;
        if (Instance==null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        if (sceneDataBase == null)
        {
            sceneDataBase = Resources.Load<SceneDataBase>("SceneDataBase");
            Debug.LogError("SceneDataBase is not assigned in LoadingManager.");
        }
       
      
    }

    private void Start()
    {
        if (currentLevel == 0) { 
            currentLevel = 1;
            SetCurrentLevelText();
        }
        SetCurrentLevelText();
    }
    // function to load Scene By Name from Our Database
    public void LoadScene(string sceneName)
    {
        var sceneEntry = sceneDataBase.sceneList.Find(s => s.sceneName == sceneName);
        if (sceneEntry != null)
        {
            SceneManager.LoadScene(sceneEntry.sceneName);
        }
        else
        {
            Debug.LogWarning("Scene not found in database: " + sceneName);
        }
    }

    //  Function to check current level and load it
    public void LoadCurrentLevel()
    {
        // Get scene name from your database using the current level index
        if (currentLevel < sceneDataBase.sceneList.Count)
        {
            string sceneName = sceneDataBase.sceneList[currentLevel].sceneName;
            Debug.Log("Loading Current Level: " + sceneName);
            LoadScene(sceneName);
            if (currentLevelText == null)
            {
                currentLevelText = GameObject.FindWithTag("LevelText")?.GetComponent<TextMeshProUGUI>();
                SetCurrentLevelText();
            }
        }
        else
        {
            Debug.LogWarning("Current level index out of range!");
        }
    }

    // function to load Next Level in the database
    public void LoadNextScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        // find current scene in database
        int currentIndex = sceneDataBase.sceneList.FindIndex(s => s.sceneName == currentSceneName);

        if (currentIndex != -1 && currentIndex + 1 < sceneDataBase.sceneList.Count)
        {
            string nextScene = sceneDataBase.sceneList[currentIndex + 1].sceneName;
            SceneManager.LoadScene(nextScene);
        }
        else if (currentIndex != -1 && currentIndex + 1 < sceneDataBase.sceneList.Count && !levelIsCompleted) {
            SceneManager.LoadScene(currentIndex);
        }
        else
        {
            Debug.Log("No next scene available or reached the last scene.");
        }
    }

    public void SetCurrentLevelText()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu"|| SceneManager.GetActiveScene().buildIndex==0) { 
        
            currentLevelText.text = "Level " + (currentLevel).ToString();
        }
    }


    
}

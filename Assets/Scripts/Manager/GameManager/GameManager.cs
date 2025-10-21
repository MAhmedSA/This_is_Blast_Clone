using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Linq.Expressions;
using System.ComponentModel;


public class GameManager : MonoBehaviour
{
    [Header("Enemy Setup")]
    public GameObject enemyPrefab;
    public Material[] enemyMaterials;
    public int numberOfEnemies ;

    [Header("Level Data")]
    public LevelData  levelData;
    private LevelData.LevelEntry  _currentLevelData;
    public LevelData.LevelEntry CurrentLevelData { get { return _currentLevelData; } }
    public int currentLevelIndex ;

    [Header("Grid Settings")]
    public Vector3 startPosition = new Vector3(-5, 0, -7.59f);
    public int rows = 3;
    public int columns = 4;
    public float spawnOffsetY = 3f;

    [Header("Level Progression")]
    [SerializeField][Range(0, 1)] float levelPrecentage;

    [SerializeField] int numberOfEnemiesDefeated=0;
    public int NumberOfEnemiesDefeated
    {
        get { return numberOfEnemiesDefeated; }
        set
        {
            numberOfEnemiesDefeated = value;
            levelPrecentage = (float)numberOfEnemiesDefeated / numberOfEnemies;
             UIManager.Instance.UpdateProgressionBar(levelPrecentage);
        }
    }

    public Transform minmumYPos;

    private EnemyMovement[,] enemyGrid;

    // Color-based enemy lists
    public List<Transform> redEnemies = new List<Transform>();
    public List<Transform> blueEnemies = new List<Transform>();
    public List<Transform> greenEnemies = new List<Transform>();
    public List<Transform> yellowEnemies = new List<Transform>();

    public static GameManager Instance;
    
    void Awake()
    {
        currentLevelIndex = PlayerPrefs.GetInt("currentLevelIndex",1);
        _currentLevelData = levelData.levelEntries[currentLevelIndex-1];
        
        Instance = this;

        if (_currentLevelData.isRandomized)
        {
            rows = _currentLevelData.rows;
            columns = _currentLevelData.columns;
        }
        else {
            rows = _currentLevelData.levelLayout.Count;
            columns = _currentLevelData.levelLayout[0].rowList.Count;

        }
        numberOfEnemies = rows * columns;

        enemyGrid = new EnemyMovement[rows, columns];
        
    }
    IEnumerator InitializeLevel()
    {
        NumberOfEnemiesDefeated = 0;
        levelPrecentage = 0;

        //PlayerManager.Instance.CreateNewPlayers();

        currentLevelIndex = PlayerPrefs.GetInt("currentLevelIndex", 1);
        _currentLevelData = levelData.levelEntries[currentLevelIndex - 1];

        if (_currentLevelData.isRandomized)
        {
            rows = _currentLevelData.rows;
            columns = _currentLevelData.columns;
        }
        else
        {
            rows = _currentLevelData.levelLayout.Count;
            columns = _currentLevelData.levelLayout[0].rowList.Count;
        }

        numberOfEnemies = rows * columns;
        enemyGrid = new EnemyMovement[rows, columns];

        PlayerManager.Instance.CreateNewPlayers();
        SpawnEnemiesGrid();
        yield return new WaitForSeconds(3f);
        AssignAttackCountsByColor();
    }
    IEnumerator Start()
    {
        SpawnEnemiesGrid();

        // wait one frame to ensure all Awake/Start have finished
        yield return null;

        AssignAttackCountsByColor();
        //PlayerManager.Instance.StartAllPlayerAttacks();



    }
   

    void SpawnEnemiesGrid()
    {
        if (_currentLevelData.isRandomized) {
            Renderer prefabRenderer = enemyPrefab.GetComponent<Renderer>();
            float enemyWidth = prefabRenderer != null ? prefabRenderer.bounds.size.x : 1f;
            float enemyHeight = prefabRenderer != null ? prefabRenderer.bounds.size.y : 1f;
            numberOfEnemies = _currentLevelData.rows * _currentLevelData.columns;
            int enemyCount = 0;

            for (int row = _currentLevelData.rows - 1; row >= 0; row--)
            {
                for (int col = 0; col < _currentLevelData.columns; col++)
                {
                    if (enemyCount >= numberOfEnemies) return;

                    Vector3 targetPos = startPosition + new Vector3(col * enemyWidth, row * enemyHeight, 0);
                    Vector3 spawnPos = targetPos + new Vector3(0, spawnOffsetY, 0);

                    GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                    newEnemy.tag = "Enemy";

                    // Random material
                    Material randomMat = enemyMaterials[Random.Range(0, _currentLevelData.colors.Length)];
                    
                    Renderer rend = newEnemy.GetComponent<Renderer>();
                    if (rend != null)
                        rend.material = randomMat;

                    // Add to color list
                    AddEnemyToColorList(newEnemy.transform, randomMat.name);

                    // Movement
                    EnemyMovement move = newEnemy.GetComponent<EnemyMovement>();
                    if (move == null)
                        move = newEnemy.AddComponent<EnemyMovement>();

                    move.InitializeEnemy(row, col, targetPos);
                    enemyGrid[row, col] = move;

                    enemyCount++;
                }
            }
        }
        else
        {
           
            Renderer prefabRenderer = enemyPrefab.GetComponent<Renderer>();
            float enemyWidth = prefabRenderer != null ? prefabRenderer.bounds.size.x : 1f;
            float enemyHeight = prefabRenderer != null ? prefabRenderer.bounds.size.y : 1f;
           
            numberOfEnemies = _currentLevelData.levelLayout.Count * _currentLevelData.levelLayout[0].rowList.Count;
           
            int enemyCount = 0;

            for (int row = _currentLevelData.levelLayout.Count - 1; row >= 0; row--)
            {
                for (int col = 0; col < _currentLevelData.levelLayout[0].rowList.Count; col++)
                {
                    if (enemyCount >= numberOfEnemies) return;

                    Vector3 targetPos = startPosition + new Vector3(col * enemyWidth, row * enemyHeight, 0);
                    Vector3 spawnPos = targetPos + new Vector3(0, spawnOffsetY, 0);

                    GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                    newEnemy.tag = "Enemy";

               
                     
                    Material randomMat = _currentLevelData.colors[_currentLevelData.levelLayout[row].rowList[col]];

                    Renderer rend = newEnemy.GetComponent<Renderer>();
                    if (rend != null)
                        rend.material = randomMat;

                    // Add to color list
                    AddEnemyToColorList(newEnemy.transform, randomMat.name);

                    // Movement
                    EnemyMovement move = newEnemy.GetComponent<EnemyMovement>();
                    if (move == null)
                        move = newEnemy.AddComponent<EnemyMovement>();

                    move.InitializeEnemy(row, col, targetPos);
                    enemyGrid[row, col] = move;

                    enemyCount++;
                }
            }
          
        }

    }

    void AddEnemyToColorList(Transform enemy, string matName)
    {
        string lower = matName.ToLower();
        if (lower.Contains("red")) redEnemies.Add(enemy);
        else if (lower.Contains("blue")) blueEnemies.Add(enemy);
        else if (lower.Contains("green")) greenEnemies.Add(enemy);
        else if (lower.Contains("yellow")) yellowEnemies.Add(enemy);

      
            
    }

    public Transform GetFrontlineEnemy(string colorName)
    {
        List<Transform> list = null;
        if (colorName.ToLower().Contains("red")) list = redEnemies;
        else if (colorName.ToLower().Contains("blue")) list = blueEnemies;
        else if (colorName.ToLower().Contains("green")) list = greenEnemies;
        else if (colorName.ToLower().Contains("yellow")) list = yellowEnemies;

        if (list == null || list.Count == 0) return null;

        Transform lowest = list[0];
        foreach (var e in list)
        {
            if (e == null) continue;
            if (e.position.y < lowest.position.y)
                lowest = e;
        }
        return lowest;
    }

    public (int row, int col) GetGridPosition(Vector3 worldPos)
    {
        Renderer prefabRenderer = enemyPrefab.GetComponent<Renderer>();
        float enemyWidth = prefabRenderer != null ? prefabRenderer.bounds.size.x : 1f;
        float enemyHeight = prefabRenderer != null ? prefabRenderer.bounds.size.y : 1f;

        int col = Mathf.RoundToInt((worldPos.x - startPosition.x) / enemyWidth);
        int row = Mathf.RoundToInt((worldPos.y - startPosition.y) / enemyHeight);

        return (row, col);
    }

    public Vector3 GetWorldPosition(int row, int col)
    {
        Renderer prefabRenderer = enemyPrefab.GetComponent<Renderer>();
        float enemyWidth = prefabRenderer != null ? prefabRenderer.bounds.size.x : 1f;
        float enemyHeight = prefabRenderer != null ? prefabRenderer.bounds.size.y : 1f;

        return startPosition + new Vector3(col * enemyWidth, row * enemyHeight, 0);
    }

    public bool IsCellFree(int row, int col)
    {
        return enemyGrid[row, col] == null;
    }

    public void OccupyCell(int row, int col, EnemyMovement enemy)
    {
        enemyGrid[row, col] = enemy;
    }

    public void FreeCell(int row, int col)
    {
        if (enemyGrid[row, col] != null)
            enemyGrid[row, col] = null;
    }

    public List<Transform> GetFirstRowEnemiesByColor(string color)
    {
        string lower = color.ToLower();
        List<Transform> colorList = null;

        if (lower.Contains("red")) colorList = redEnemies;
        else if (lower.Contains("blue")) colorList = blueEnemies;
        else if (lower.Contains("green")) colorList = greenEnemies;
        else if (lower.Contains("yellow")) colorList = yellowEnemies;

        if (colorList == null|| colorList.Count<=0)
        {
            
            return new List<Transform>();
        }

        if (colorList.Count == 0) return new List<Transform>();

        float minY = minmumYPos.position.y;
        

        List<Transform> firstRow = new List<Transform>();
        foreach (var e in colorList)
            if (e != null && Mathf.Abs(e.position.y - minY) < 0.1f)
                firstRow.Add(e);
      
        return firstRow;
    }

    void AssignAttackCountsByColor()
    {
        GameObject[] playerObjs = GameObject.FindGameObjectsWithTag("Player");
        List<PlayerAttack> players = new List<PlayerAttack>();

        foreach (var p in playerObjs)
        {
            var atk = p.GetComponent<PlayerAttack>();
            if (atk != null)
                players.Add(atk);
        }

        if (players.Count == 0)
        {
        
            return;
        }

        // Group players by color
        Dictionary<string, List<PlayerAttack>> playersByColor = new Dictionary<string, List<PlayerAttack>>();
        foreach (var player in players)
        {
            string color = player.playerColor.ToLower();
            if (!playersByColor.ContainsKey(color))
                playersByColor[color] = new List<PlayerAttack>();
            playersByColor[color].Add(player);
        }

        // Assign attack numbers per color
        AssignColor("red", redEnemies, playersByColor);
        AssignColor("blue", blueEnemies, playersByColor);
        AssignColor("green", greenEnemies, playersByColor);
        AssignColor("yellow", yellowEnemies, playersByColor);
    }

    void AssignColor(string color, List<Transform> colorEnemies, Dictionary<string, List<PlayerAttack>> playersByColor)
    {
        if (!playersByColor.ContainsKey(color)) return;

        List<PlayerAttack> colorPlayers = playersByColor[color];
        int totalEnemies = colorEnemies.Count;

        if (totalEnemies == 0) return;

        // Split enemies fairly across players
        int[] split = GenerateEvenSplit(totalEnemies, colorPlayers.Count);

        for (int i = 0; i < colorPlayers.Count; i++)
        {
            colorPlayers[i].SetAttackCount(split[i]);
        }
    }


    // Generates a fair even split that always sums exactly to total
    int[] GenerateEvenSplit(int total, int parts)
    {
        int[] result = new int[parts];
        if (parts == 0 || total == 0) return result;

        int baseShare = total / parts;
        int remainder = total % parts;

        for (int i = 0; i < parts; i++)
        {
            result[i] = baseShare;
            if (i < remainder)
                result[i]++;
        }

        // Optional shuffle for randomness
        for (int i = 0; i < result.Length; i++)
        {
            int j = Random.Range(0, result.Length);
            (result[i], result[j]) = (result[j], result[i]);
        }

        return result;
    }

    
    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("currentLevelIndex", currentLevelIndex);
    }
    public void LevelUP() {
      
        if (currentLevelIndex < levelData.levelEntries.Length ) { 
           
           
            currentLevelIndex++;
            PlayerPrefs.SetInt("currentLevelIndex", currentLevelIndex);
            StartCoroutine(InitializeLevel());
          
           
        }
    
    }
    
}

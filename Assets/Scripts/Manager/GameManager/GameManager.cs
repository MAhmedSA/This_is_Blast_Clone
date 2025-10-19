using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;


public class GameManager : MonoBehaviour
{
    [Header("Enemy Setup")]
    public GameObject enemyPrefab;
    public Material[] enemyMaterials;
    public int numberOfEnemies = 20;
    [Header("Level Data")]
    public List<LevelData>  levelData;
    public int currentLevelIndex ;
    [Header("Grid Settings")]
    public Vector3 startPosition = new Vector3(-5, 0, -7.59f);
    public int rows = 3;
    public int columns = 4;
    public float spawnOffsetY = 3f;

    public Transform minmumYPos;

    private EnemyMovement[,] enemyGrid;

    // Color-based enemy lists
    public List<Transform> redEnemies = new List<Transform>();
    public List<Transform> blueEnemies = new List<Transform>();
    public List<Transform> greenEnemies = new List<Transform>();

    public static GameManager Instance;

    void Awake()
    {
        currentLevelIndex = PlayerPrefs.GetInt("currentLevelIndex", 0);
        Instance = this;
        enemyGrid = new EnemyMovement[rows, columns];
    }
    IEnumerator Start()
    {
        SpawnEnemiesGrid();

        // wait one frame to ensure all Awake/Start have finished
        yield return null;

        AssignAttackCountsByColor();

        Debug.Log($"RED enemies: {redEnemies.Count}");
        Debug.Log($"BLUE enemies: {blueEnemies.Count}");
        Debug.Log($"GREEN enemies: {greenEnemies.Count}");
    }
   

    void SpawnEnemiesGrid()
    {
        Renderer prefabRenderer = enemyPrefab.GetComponent<Renderer>();
        float enemyWidth = prefabRenderer != null ? prefabRenderer.bounds.size.x : 1f;
        float enemyHeight = prefabRenderer != null ? prefabRenderer.bounds.size.y : 1f;

        int enemyCount = 0;

        for (int row = rows - 1; row >= 0; row--)
        {
            for (int col = 0; col < columns; col++)
            {
                if (enemyCount >= numberOfEnemies) return;

                Vector3 targetPos = startPosition + new Vector3(col * enemyWidth, row * enemyHeight, 0);
                Vector3 spawnPos = targetPos + new Vector3(0, spawnOffsetY, 0);

                GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                newEnemy.tag = "Enemy";

                // Random material
                Material randomMat = enemyMaterials[Random.Range(0, enemyMaterials.Length)];
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

    void AddEnemyToColorList(Transform enemy, string matName)
    {
        string lower = matName.ToLower();
        if (lower.Contains("red")) redEnemies.Add(enemy);
        else if (lower.Contains("blue")) blueEnemies.Add(enemy);
        else if (lower.Contains("green")) greenEnemies.Add(enemy);
    }

    public Transform GetFrontlineEnemy(string colorName)
    {
        List<Transform> list = null;
        if (colorName.ToLower().Contains("red")) list = redEnemies;
        else if (colorName.ToLower().Contains("blue")) list = blueEnemies;
        else if (colorName.ToLower().Contains("green")) list = greenEnemies;

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

        if (colorList == null|| colorList.Count<=0)
        {
            Debug.LogWarning($"[GameManager] Color list not found for '{color}'");
            return new List<Transform>();
        }

        if (colorList.Count == 0) return new List<Transform>();

        float minY = minmumYPos.position.y;
        

        List<Transform> firstRow = new List<Transform>();
        foreach (var e in colorList)
            if (e != null && Mathf.Abs(e.position.y - minY) < 0.1f)
                firstRow.Add(e);
        Debug.Log($"[GameManager] Found {firstRow.Count} '{color}' enemies in the first row (Y={minY})");
        return firstRow;
    }

    // ================================
    //  NEW SECTION: Attack Count Distribution
    // ================================
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
            Debug.LogWarning("[GameManager] No players found for attack assignment!");
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
    }

    void AssignColor(string color, List<Transform> colorEnemies, Dictionary<string, List<PlayerAttack>> playersByColor)
    {
        if (!playersByColor.ContainsKey(color))
        {
            Debug.Log($"[AssignAttackCounts] No {color} players found.");
            return;
        }

        List<PlayerAttack> colorPlayers = playersByColor[color];

        int totalEnemies = colorEnemies.Count; //  use all enemies of this color

        if (totalEnemies == 0)
        {
            Debug.Log($"[AssignAttackCounts] No {color} enemies found.");
            return;
        }

        int[] split = GenerateRandomSplit(totalEnemies, colorPlayers.Count);

        for (int i = 0; i < colorPlayers.Count; i++)
        {
            // Explicitly cast the argument to ensure the correct method is called
            colorPlayers[i].SetAttackCount(split[i]); // Removed redundant cast to (int)
            Debug.Log($"[{color.ToUpper()}] {colorPlayers[i].name} → {split[i]} attacks");
        }

        int totalAssigned = split.Sum();
        Debug.Log($"✅ [{color.ToUpper()}] Total attacks = {totalAssigned}/{totalEnemies}");
    }

    int[] GenerateRandomSplit(int total, int parts)
    {
        int[] result = new int[parts];
        if (parts == 0) return result;

        // Generate random cut points between 0 and total
        List<int> cuts = new List<int>();
        for (int i = 0; i < parts - 1; i++)
            cuts.Add(Random.Range(0, total + 1));

        cuts.Sort();

        int prev = 0;
        for (int i = 0; i < parts - 1; i++)
        {
            result[i] = cuts[i] - prev;
            prev = cuts[i];
        }

        result[parts - 1] = total - prev;

        // Ensure nobody gets 0 (optional)
        for (int i = 0; i < result.Length; i++)
        {
            if (result[i] == 0) result[i] = 1;
        }

        // Adjust total back if we overshoot after forcing ≥1
        int currentSum = result.Sum();
        while (currentSum > total)
        {
            int idx = Random.Range(0, result.Length);
            if (result[idx] > 1)
            {
                result[idx]--;
                currentSum--;
            }
        }

        // Shuffle for randomness
        for (int i = 0; i < result.Length; i++)
        {
            int rand = Random.Range(0, result.Length);
            (result[i], result[rand]) = (result[rand], result[i]);
        }

        return result;
    }
    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("currentLevelIndex", currentLevelIndex);
    }

}

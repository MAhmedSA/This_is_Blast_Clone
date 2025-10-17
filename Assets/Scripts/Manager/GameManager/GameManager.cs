using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Enemy Setup")]
    public GameObject enemyPrefab;
    public Material[] enemyMaterials;
    public int numberOfEnemies = 20;

    [Header("Grid Settings")]
    public Vector3 startPosition = new Vector3(-5, 0, -7.59f);
    public int rows = 3;
    public int columns = 4;
    public float spawnOffsetY = 3f;

    private EnemyMovement[,] enemyGrid;

    //  New color-based enemy lists
    public List<Transform> redEnemies = new List<Transform>();
    public List<Transform> blueEnemies = new List<Transform>();
    public List<Transform> greenEnemies = new List<Transform>();

    public static GameManager Instance;

    void Awake()
    {
        Instance = this;
        enemyGrid = new EnemyMovement[rows, columns];
    }

    void Start()
    {
        SpawnEnemiesGrid();
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

                // Add to the appropriate color list
                AddEnemyToColorList(newEnemy.transform, randomMat.name);

                // Add movement and initialize
                EnemyMovement move = newEnemy.GetComponent<EnemyMovement>();
                if (move == null)
                    move = newEnemy.AddComponent<EnemyMovement>();

                move.InitializeEnemy(row, col, targetPos);
                enemyGrid[row, col] = move;

                enemyCount++;
            }
        }
    }

    // Add enemy to the color list based on its material
    void AddEnemyToColorList(Transform enemy, string matName)
    {
        string lower = matName.ToLower();
        if (lower.Contains("red"))
            redEnemies.Add(enemy);
        else if (lower.Contains("blue"))
            blueEnemies.Add(enemy);
        else if (lower.Contains("green"))
            greenEnemies.Add(enemy);
    }

    //Return the frontline (lowest Y) enemy of that color
    public Transform GetFrontlineEnemy(string colorName)
    {
        List<Transform> list = null;
        if (colorName.ToLower().Contains("red")) list = redEnemies;
        else if (colorName.ToLower().Contains("blue")) list = blueEnemies;
        else if (colorName.ToLower().Contains("green")) list = greenEnemies;

        if (list == null || list.Count == 0) return null;

        // Pick the lowest Y enemy (frontline)
        Transform lowest = list[0];
        foreach (var e in list)
        {
            if (e == null) continue;
            if (e.position.y < lowest.position.y)
                lowest = e;
        }

        return lowest;
    }

    // ---------------- GRID HELPERS (unchanged) ----------------
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



}

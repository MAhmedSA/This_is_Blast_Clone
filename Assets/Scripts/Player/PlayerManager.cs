using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class PlayerManager : MonoBehaviour
{
    [Header("Spawn Setup")]
    public GameObject playerPrefab;
    public List<GameObject> touchedPlayers;
    public Transform[] spawnPositions;
    public Material[] playerMaterials;
    public int numberOfPlayers = 5;

    [Header("Boundary Positions")]
    public Vector3 topFirstRow = new Vector3(0f, 0.22f, -7.75f);
    public Vector3 rightPosition = new Vector3(0.47f, 0.22f, -7.75f);
    public Vector3 downPosition = new Vector3(0.01f, 0f, -7.75f);
    public Vector3 leftPosition = new Vector3(-0.52f, 0f, -7.75f);

    [Header("Grid Setup")]
    public int gridRows = 3;
    public int gridColumns = 3;
    public float horizontalSpacing = 0.5f;
    public float verticalSpacing = 0.5f;

    [Header("Available Locations")]
    public List<Transform> moveLocations;

    private Dictionary<Transform, bool> locationOccupied = new Dictionary<Transform, bool>();
    private Dictionary<Vector2Int, GameObject> gridPositions = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<GameObject, Vector2Int> playerGridPositions = new Dictionary<GameObject, Vector2Int>();

    private Dictionary<string, bool> colorAttackLock = new Dictionary<string, bool>
    {
        { "red", false },
        { "blue", false },
        { "green", false },
        { "yellow", false }
    };

    public static PlayerManager Instance;

    // Public properties to access boundaries
    public Vector3 FirstRowPosition => topFirstRow;
    public Vector3 RightBoundary => rightPosition;
    public Vector3 LeftBoundary => leftPosition;
    public Vector3 BottomBoundary => downPosition;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ResetPlayerState();
        SpawnPlayers();
    }

    public void ClearAllPlayers()
    {
        foreach (var player in touchedPlayers)
        {
            if (player != null)
            {
                Vector2Int gridPos;
                if (playerGridPositions.TryGetValue(player, out gridPos))
                {
                    gridPositions.Remove(gridPos);
                }
            }
        }

        touchedPlayers.Clear();
        playerGridPositions.Clear();
        gridPositions.Clear();
        touchedPlayers = new List<GameObject>();
    }

    public void ResetPlayerState()
    {
        ClearAllPlayers();

        locationOccupied.Clear();
        foreach (var loc in moveLocations)
            locationOccupied[loc] = false;

        colorAttackLock.Clear();
        foreach (var mat in GameManager.Instance.CurrentLevelData.colors)
        {
            string colorName = mat.name.Replace(" (Instance)", "");
            colorAttackLock[colorName] = false;
        }
    }

    public void CreateNewPlayers()
    {
        GameObject[] oldPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in oldPlayers)
        {
            Destroy(player);
        }

        ResetPlayerState();
        SpawnPlayers();
    }

    void SpawnPlayers()
    {
        InitializeGrid();

        int playersSpawned = 0;

        // Calculate grid boundaries based on your positions
        float gridWidth = rightPosition.x - leftPosition.x;
        float gridHeight = topFirstRow.z - downPosition.z;

        horizontalSpacing = gridWidth / (gridColumns - 1);
        verticalSpacing = gridHeight / (gridRows - 1);

        // Spawn players in grid positions
        for (int row = 0; row < gridRows && playersSpawned < GameManager.Instance.CurrentLevelData.playerCount; row++)
        {
            for (int col = 0; col < gridColumns && playersSpawned < GameManager.Instance.CurrentLevelData.playerCount; col++)
            {
                Vector2Int gridPos = new Vector2Int(col, row);
                Vector3 worldPos = GridToWorldPosition(gridPos);

                GameObject newPlayer = Instantiate(playerPrefab, worldPos, Quaternion.identity);

                if (newPlayer.GetComponent<PlayerAttack>() == null)
                    newPlayer.AddComponent<PlayerAttack>();

                // Set player color and properties
                if (GameManager.Instance.CurrentLevelData.colors.Length > 0)
                {
                    string colorName = "";
                    Material randomMat = GameManager.Instance.CurrentLevelData.colors[playersSpawned % GameManager.Instance.CurrentLevelData.colors.Length];

                    Renderer rend = newPlayer.GetComponent<Renderer>();
                    if (randomMat.name == "Red_Mat")
                        colorName = "red";
                    else if (randomMat.name == "Blue_Mat")
                        colorName = "blue";
                    else if (randomMat.name == "Green_Mat")
                        colorName = "green";
                    else if (randomMat.name == "Yellow_Mat")
                        colorName = "yellow";

                    if (rend != null)
                    {
                        rend.material = randomMat;
                    }

                    SetLayerByMaterial(newPlayer, randomMat);

                    PlayerAttack attack = newPlayer.GetComponent<PlayerAttack>();
                    if (attack != null)
                    {
                        attack.playerColor = colorName;
                        // Set first row players for immediate attack
                        attack.isFirstRow = (row == 0);
                    }
                }

                // Register player in grid system
                RegisterPlayerInGrid(newPlayer, gridPos);
                //touchedPlayers.Add(newPlayer);
                newPlayer.tag = "Player";

                playersSpawned++;
            }
        }
    }

    void InitializeGrid()
    {
        gridPositions.Clear();
        playerGridPositions.Clear();

        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < gridColumns; col++)
            {
                gridPositions[new Vector2Int(col, row)] = null;
            }
        }
    }

    Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        // Calculate position within boundaries
        float x = Mathf.Lerp(leftPosition.x, rightPosition.x, (float)gridPos.x / (gridColumns - 1));
        float z = Mathf.Lerp(topFirstRow.z, downPosition.z, (float)gridPos.y / (gridRows - 1));
        float y = Mathf.Lerp(topFirstRow.y, downPosition.y, (float)gridPos.y / (gridRows - 1));

        return new Vector3(x, y, z);
    }

    public void RegisterPlayerInGrid(GameObject player, Vector2Int gridPos)
    {
        // Remove from old position if exists
        if (playerGridPositions.ContainsKey(player))
        {
            Vector2Int oldPos = playerGridPositions[player];
            gridPositions[oldPos] = null;
        }

        // Add to new position
        gridPositions[gridPos] = player;
        playerGridPositions[player] = gridPos;

        // Update first row status
        PlayerAttack attack = player.GetComponent<PlayerAttack>();
        if (attack != null)
        {
            attack.isFirstRow = (gridPos.y == 0); // First row is y=0 (top row)
        }
    }

    // Call this when a player leaves their position
    public void PlayerLeftPosition(GameObject player)
    {
        if (playerGridPositions.ContainsKey(player))
        {
            Vector2Int emptyPos = playerGridPositions[player];

            // Remove the player
            gridPositions[emptyPos] = null;
            playerGridPositions.Remove(player);
            touchedPlayers.Remove(player);

            // Move players to fill the gap
            FillGridGap(emptyPos);
        }
    }

    void FillGridGap(Vector2Int emptyPos)
    {
        // For 2.5D, we fill gaps by moving players from back to front
        // Players in higher row numbers (further back) move forward

        List<Vector2Int> positionsToCheck = new List<Vector2Int>();

        // Check all positions behind the empty spot in the same column
        for (int row = emptyPos.y + 1; row < gridRows; row++)
        {
            positionsToCheck.Add(new Vector2Int(emptyPos.x, row));
        }

        foreach (Vector2Int checkPos in positionsToCheck)
        {
            if (gridPositions.ContainsKey(checkPos) && gridPositions[checkPos] != null)
            {
                GameObject playerToMove = gridPositions[checkPos];
                MovePlayerToGridPosition(playerToMove, emptyPos);
                emptyPos = checkPos; // The moved player's old position is now empty
            }
        }
    }

    void MovePlayerToGridPosition(GameObject player, Vector2Int gridPos)
    {
        Vector2Int oldPos = playerGridPositions[player];

        // Update grid
        gridPositions[oldPos] = null;
        gridPositions[gridPos] = player;
        playerGridPositions[player] = gridPos;

        // Move player visually
        Vector3 targetWorldPos = GridToWorldPosition(gridPos);
        player.transform.position = targetWorldPos;

        // Update first row status
        PlayerAttack attack = player.GetComponent<PlayerAttack>();
        if (attack != null)
        {
            attack.isFirstRow = (gridPos.y == 0);
        }

       
    }

    // Get all players in the first row
    public List<GameObject> GetFirstRowPlayers()
    {
        List<GameObject> firstRowPlayers = new List<GameObject>();

        for (int col = 0; col < gridColumns; col++)
        {
            Vector2Int gridPos = new Vector2Int(col, 0); // First row is y=0
            if (gridPositions.ContainsKey(gridPos) && gridPositions[gridPos] != null)
            {
                firstRowPlayers.Add(gridPositions[gridPos]);
            }
        }

        return firstRowPlayers;
    }

    // Get players by specific color in first row
    public List<GameObject> GetFirstRowPlayersByColor(string color)
    {
        List<GameObject> coloredPlayers = new List<GameObject>();
        var firstRowPlayers = GetFirstRowPlayers();

        foreach (GameObject player in firstRowPlayers)
        {
            PlayerAttack attack = player.GetComponent<PlayerAttack>();
            if (attack != null && attack.playerColor.ToLower() == color.ToLower())
            {
                coloredPlayers.Add(player);
            }
        }

        return coloredPlayers;
    }

    // Check if a position is within game boundaries
    public bool IsWithinBoundaries(Vector3 position)
    {
        return position.x >= leftPosition.x && position.x <= rightPosition.x &&
               position.z <= topFirstRow.z && position.z >= downPosition.z;
    }

    // Get all empty grid positions
    public List<Vector2Int> GetEmptyGridPositions()
    {
        List<Vector2Int> emptyPositions = new List<Vector2Int>();

        foreach (var kvp in gridPositions)
        {
            if (kvp.Value == null)
            {
                emptyPositions.Add(kvp.Key);
            }
        }

        return emptyPositions;
    }

    // Get player at specific grid position
    public GameObject GetPlayerAtGridPosition(Vector2Int gridPos)
    {
        if (gridPositions.ContainsKey(gridPos))
        {
            return gridPositions[gridPos];
        }
        return null;
    }

    // Get grid position of a specific player
    public Vector2Int GetPlayerGridPosition(GameObject player)
    {
        if (playerGridPositions.ContainsKey(player))
        {
            return playerGridPositions[player];
        }
        return new Vector2Int(-1, -1);
    }

    // Visualize grid and boundaries in editor
    void OnDrawGizmosSelected()
    {
        // Draw boundaries
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(topFirstRow, 0.1f);
        Gizmos.DrawSphere(rightPosition, 0.1f);
        Gizmos.DrawSphere(downPosition, 0.1f);
        Gizmos.DrawSphere(leftPosition, 0.1f);

        // Draw boundary lines
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(topFirstRow, rightPosition);
        Gizmos.DrawLine(rightPosition, downPosition);
        Gizmos.DrawLine(downPosition, leftPosition);
        Gizmos.DrawLine(leftPosition, topFirstRow);

        // Draw grid positions
        Gizmos.color = Color.cyan;
        if (Application.isPlaying)
        {
            foreach (var kvp in gridPositions)
            {
                Vector3 pos = GridToWorldPosition(kvp.Key);
                Gizmos.DrawWireCube(pos, new Vector3(0.3f, 0.1f, 0.3f));
            }
        }
    }

    // Rest of your existing methods...
    void SetLayerByMaterial(GameObject player, Material mat)
    {
        string matName = mat.name.ToLower();

        if (matName.Contains("red"))
        {
            player.layer = LayerMask.NameToLayer("RedPlayers");
        }
        else if (matName.Contains("blue"))
        {
            player.layer = LayerMask.NameToLayer("BluePlayers");
        }
        else if (matName.Contains("green"))
        {
            player.layer = LayerMask.NameToLayer("GreenPlayers");
        }
        else if (matName.Contains("yellow"))
        {
            player.layer = LayerMask.NameToLayer("YellowPlayers");
        }
        else
        {
            player.layer = LayerMask.NameToLayer("Default");
        }
    }

    public Transform GetFreeLocation()
    {
        foreach (var loc in moveLocations)
        {
            if (!locationOccupied[loc])
            {
                locationOccupied[loc] = true;
                return loc;
            }
        }
        return null;
    }

    public void FreeLocation(Transform location)
    {
        if (location != null && locationOccupied.ContainsKey(location))
        {
            locationOccupied[location] = false;
        }
    }

    public void CheckPlayersForAttack()
    {
      
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            if (player.GetComponent<PlayerAttack>() == null)
                player.AddComponent<PlayerAttack>();

            PlayerAttack attack = player.GetComponent<PlayerAttack>();
            if (attack != null)
            {
                
            }
        }
    }

    public bool CanColorAttack(string colorName)
    {
        if (!colorAttackLock.ContainsKey(colorName))
        {
            colorAttackLock[colorName] = false;
        }
        return !colorAttackLock[colorName];
    }

    public void LockColorAttack(string colorName)
    {
        string key = colorName.ToLower();
        if (colorAttackLock.ContainsKey(key))
            colorAttackLock[key] = true;
    }

    public void UnlockColorAttack(string colorName)
    {
        string key = colorName.ToLower();
        if (colorAttackLock.ContainsKey(key))
            colorAttackLock[key] = false;
    }

    public void StartAllPlayerAttacks()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        Dictionary<string, List<PlayerAttack>> playersByColor = new Dictionary<string, List<PlayerAttack>>();
        foreach (var playerObj in players)
        {
            var atk = playerObj.GetComponent<PlayerAttack>();
            if (atk == null) continue;

            string color = atk.playerColor.ToLower();
            if (!playersByColor.ContainsKey(color))
                playersByColor[color] = new List<PlayerAttack>();

            playersByColor[color].Add(atk);
        }

        foreach (var kvp in playersByColor)
        {
            string color = kvp.Key;
            List<PlayerAttack> colorPlayers = kvp.Value;
            List<Transform> colorEnemies = GameManager.Instance.GetFirstRowEnemiesByColor(color);

            if (colorEnemies == null || colorEnemies.Count == 0) continue;

            int enemyIndex = 0;


            foreach (var player in colorPlayers)
            {
                List<Transform> assigned = new List<Transform>();

                for (int i = 0; i < player.attackCount && enemyIndex < colorEnemies.Count; i++)
                {
                    assigned.Add(colorEnemies[enemyIndex]);
                    enemyIndex++;
                }

                player.AssignTargets(assigned);
            }
        }
    }
    public void AssignEnemiesToPlayer(GameObject player)
    {
        PlayerAttack attack = player.GetComponent<PlayerAttack>();
        if (attack == null) return;

        string color = attack.playerColor;

        // Get available enemies for this color that aren't already assigned
        List<Transform> allColorEnemies = GameManager.Instance.GetFirstRowEnemiesByColor(color);
        if (allColorEnemies == null || allColorEnemies.Count == 0) return;

        // Filter out enemies that are already being attacked
        List<Transform> availableEnemies = new List<Transform>();
        foreach (Transform enemy in allColorEnemies)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy && !IsEnemyBeingAttacked(enemy))
            {
                availableEnemies.Add(enemy);
            }
        }

        if (availableEnemies.Count > 0)
        {
            int count = Mathf.Min(attack.attackCount, availableEnemies.Count);
            List<Transform> assignedEnemies = availableEnemies.GetRange(0, count);
            attack.AssignTargets(assignedEnemies);

            // Mark these enemies as being attacked
            foreach (Transform enemy in assignedEnemies)
            {
                MarkEnemyAsAttacked(enemy, true);
            }
        }
    }

    private bool IsEnemyBeingAttacked(Transform enemy)
    {
        // Check if any player is currently attacking this enemy
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            PlayerAttack attack = player.GetComponent<PlayerAttack>();
            if (attack != null && attack.isAttacking && attack.targetEnemies.Contains(enemy))
            {
                return true;
            }
        }
        return false;
    }

    private void MarkEnemyAsAttacked(Transform enemy, bool isAttacked)
    {
        // You can implement enemy state tracking here if needed
    }
    public void AssignTargetsToPlayer(GameObject player, List<Transform> enemies)
    {
        PlayerAttack attack = player.GetComponent<PlayerAttack>();
        if (attack != null && enemies != null && enemies.Count > 0)
        {
            int count = Mathf.Min(attack.attackCount, enemies.Count);
            List<Transform> assigned = enemies.GetRange(0, count);
            attack.AssignTargets(assigned);
        }
    }
}
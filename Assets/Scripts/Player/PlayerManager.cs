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


    [Header("Available Locations")]
    public List<Transform> moveLocations; // Assign in Inspector

    private Dictionary<Transform, bool> locationOccupied = new Dictionary<Transform, bool>();

    private Dictionary<string, bool> colorAttackLock = new Dictionary<string, bool>
{
    { "red", false },
    { "blue", false },
    { "green", false }
};

    public static PlayerManager Instance;

    void Awake()
    {
        Instance = this; // Singleton for easy access
    }

    void Start()
    {
        foreach (var loc in moveLocations)
        {
            locationOccupied[loc] = false;
        }

        foreach (var mat in playerMaterials) {
            string colorName = mat.name.Replace(" (Instance)", "");
            if (!colorAttackLock.ContainsKey(colorName))
                colorAttackLock[colorName] = false;
        }

        SpawnPlayers();
    }


    void SpawnPlayers()
    {
        for (int i = 0; i < numberOfPlayers; i++)
        {
            Transform spawnPoint = spawnPositions[i];
            GameObject newPlayer = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);

            if (newPlayer.GetComponent<PlayerAttack>() == null)
                newPlayer.AddComponent<PlayerAttack>();

           

            if (playerMaterials.Length > 0)
            {
                string colorName;
                colorName = "";
                Material randomMat = playerMaterials[i];
                Renderer rend = newPlayer.GetComponent<Renderer>();
                if (randomMat.name == "Red_Mat")
                    colorName = "red";
                if (randomMat.name == "Blue_Mat")
                    colorName = "blue";
                if (randomMat.name == "Green_Mat")
                    colorName = "green";
                // Material mat = playerMaterials[i % playerMaterials.Length];
                if (rend != null)
                {
                    rend.material = randomMat;
                }

                // Set layer based on color name
                SetLayerByMaterial(newPlayer, randomMat);
              
                newPlayer.GetComponent<PlayerAttack>().playerColor = colorName;
            }


            newPlayer.tag = "Player";
        }
    }

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
        else
        {
            player.layer = LayerMask.NameToLayer("Default"); // fallback
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
        return null; // none available
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
        Debug.Log("inside Player Attack Check Function");
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            if (player.GetComponent<PlayerAttack>()== null)
                player.AddComponent<PlayerAttack>();

            PlayerAttack attack = player.GetComponent<PlayerAttack>();
            Debug.Log("PlayerAttack :  " + attack.name);
            if (attack != null)
            {
                Debug.Log("Attack Script not equle null value ");
              //  attack.EnableAttack(); // ? re-check matching enemy
            }
        }
    }




    //Check if a color can attack
    public bool CanColorAttack(string colorName)
    {
        if (!colorAttackLock.ContainsKey(colorName))
        {
            Debug.LogWarning($"[PlayerManager] Color '{colorName}' not found in colorLock — adding it now.");
            colorAttackLock[colorName] = false;
        }
        return !colorAttackLock[colorName];
    }
    //Lock a color attack (start attacking)
    public void LockColorAttack(string colorName)
    {
        string key = colorName.ToLower();
        if (colorAttackLock.ContainsKey(key))
            colorAttackLock[key] = true;
    }

    // Unlock a color attack (attack finished)
    public void UnlockColorAttack(string colorName)
    {
        string key = colorName.ToLower();
        if (colorAttackLock.ContainsKey(key))
            colorAttackLock[key] = false;
    }

    public void StartAllPlayerAttacks()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        // Group players by color
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

        // For each color group, assign unique enemies based on attack count
        foreach (var kvp in playersByColor)
        {
            string color = kvp.Key;
            List<PlayerAttack> colorPlayers = kvp.Value;
            List<Transform> colorEnemies = GameManager.Instance.GetFirstRowEnemiesByColor(color);

            if (colorEnemies == null || colorEnemies.Count == 0)
            {
                Debug.LogWarning($"[PlayerManager] No enemies found for color {color}");
                continue;
            }

            int totalEnemies = colorEnemies.Count;
            int totalAttackSlots = colorPlayers.Sum(p => p.attackCount);
            int enemyIndex = 0;

            // Assign targets sequentially based on attack count
            foreach (var player in colorPlayers)
            {
                List<Transform> assigned = new List<Transform>();

                for (int i = 0; i < player.attackCount && enemyIndex < totalEnemies; i++)
                {
                    assigned.Add(colorEnemies[enemyIndex]);
                    enemyIndex++;
                }

                //if (assigned.Count > 0)
                //   // player.EnableAttack(assigned);
                //else
                //    Debug.Log($"[PlayerManager] {player.name} has no targets for {color}");
            }
        }
    }



}
using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerManager : MonoBehaviour
{
    [Header("Spawn Setup")]
    public GameObject playerPrefab;
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
            Transform spawnPoint = spawnPositions[UnityEngine.Random.Range(0, spawnPositions.Length)];
            GameObject newPlayer = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);

            if (newPlayer.GetComponent<PlayerAttack>() == null)
                newPlayer.AddComponent<PlayerAttack>();



            if (playerMaterials.Length > 0)
            {
                Material randomMat = playerMaterials[UnityEngine.Random.Range(0, playerMaterials.Length)];
                Renderer rend = newPlayer.GetComponent<Renderer>();
                // Material mat = playerMaterials[i % playerMaterials.Length];
                if (rend != null)
                {
                    rend.material = randomMat;
                }

                // Set layer based on color name
                SetLayerByMaterial(newPlayer, randomMat);
                string colorName = randomMat.name.Replace(" (Instance)", "");
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

}
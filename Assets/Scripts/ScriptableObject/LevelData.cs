
using NUnit.Framework;
using System.Collections.Generic;
using ToonyColorsPro;
using UnityEngine;
[CreateAssetMenu(fileName = "LevelData", menuName = "Level/Level Data")]
public class LevelData : ScriptableObject
{
    [System.Serializable]
    public class IntListWrapper
    {
        public List<int> rowList;

        public IntListWrapper()
        {
            rowList = new List<int>();
        }
    }

    [System.Serializable]
    public class LevelEntry
    {
        [Tooltip("Name of the level such as Level 1")]
        public string levelName;

        [Tooltip("Number of players in this level")]
        public int playerCount;
        
        [Tooltip("Number of rows & columns for enemies in this level ")]
        public int rows;
        public int columns;
        
        [Tooltip("Layout Of Level is random or not to arrange enemies")]
        public bool isRandomized;

        [Tooltip("Layout Of Level is not random to arrange enemies such as cubs")]
        public List<IntListWrapper> levelLayout = new List<IntListWrapper>();

        [Tooltip("bool to check there is tutorial or not in this level")]
        public bool isTutorial;

        [Tooltip("Colors for players and enemies in this level ")]
        public Material[] colors;

        [Tooltip("Colors for environment in this level ")]
        public Material[] environmentMat; 
        
    }

    public LevelEntry[] levelEntries;
}

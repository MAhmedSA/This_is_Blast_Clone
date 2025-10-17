using UnityEngine;
using System.Collections.Generic;




[CreateAssetMenu(fileName = "SceneDatabase", menuName = "Scene/Scene Database")]
public class SceneDataBase : ScriptableObject
{
    [System.Serializable]
    public class SceneEntry
    {
        public string sceneName;
        public int sceneIndex;
    }
    public List<SceneEntry> sceneList = new List<SceneEntry>();
}

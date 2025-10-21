using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AudioDatabase", menuName = "Audio/Audio Database")]
public class AudioDatabase : ScriptableObject
{
    [System.Serializable]
    public class AudioEntry
    {
        public string id;      
        public AudioClip clip; 
        public bool isLooping; 
    }

    public List<AudioEntry> audioList = new List<AudioEntry>();

   
    public AudioClip GetClipById(string id)
    {
        foreach (var entry in audioList)
        {
            if (entry.id == id)
                return entry.clip;
        }
       
        return null;
    }
}

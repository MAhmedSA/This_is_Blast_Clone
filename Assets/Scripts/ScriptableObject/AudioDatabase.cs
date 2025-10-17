using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AudioDatabase", menuName = "Audio/Audio Database")]
public class AudioDatabase : ScriptableObject
{
    [System.Serializable]
    public class AudioEntry
    {
        public string id;       // Unique ID for this sound
        public AudioClip clip;  // Audio clip reference
        public bool isLooping; // Is the audio looping
    }

    public List<AudioEntry> audioList = new List<AudioEntry>();

    /// <summary>
    /// Get AudioClip by id
    /// </summary>
    public AudioClip GetClipById(string id)
    {
        foreach (var entry in audioList)
        {
            if (entry.id == id)
                return entry.clip;
        }
        Debug.LogWarning("AudioClip with ID " + id + " not found!");
        return null;
    }
}

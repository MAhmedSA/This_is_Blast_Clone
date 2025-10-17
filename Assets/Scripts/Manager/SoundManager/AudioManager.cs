using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioDatabase audioDatabase;
    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(string id)
    {
        AudioClip clip = audioDatabase.GetClipById(id);
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}

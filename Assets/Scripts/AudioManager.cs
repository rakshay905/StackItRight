using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioClip placeClip;
    public AudioClip perfectClip;
    public AudioClip failClip;
    public AudioClip buttonClip;

    AudioSource source;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            source = GetComponent<AudioSource>();
        }
        else Destroy(gameObject);
    }

    public void Play(AudioClip clip)
    {
        if (clip == null) return;
        source.PlayOneShot(clip);
    }
}

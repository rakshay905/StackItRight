// using UnityEngine;

// public class AudioManager : MonoBehaviour
// {
//     public static AudioManager Instance;

//     public bool soundOn = true;
//     public bool vibrationOn = true;

//     public AudioClip placeClip;
//     public AudioClip perfectClip;
//     public AudioClip failClip;
//     public AudioClip buttonClip;

//     AudioSource source;

//     void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//             source = GetComponent<AudioSource>();
//         }
//         else Destroy(gameObject);
//     }

//     public void Play(AudioClip clip)
//     {
//         if (clip == null) return;
//         source.PlayOneShot(clip);
//     }

//     public void Vibrate()
//     {
//         if (!vibrationOn) return;

//     #if UNITY_ANDROID || UNITY_IOS
//         Handheld.Vibrate();
//     #endif
//     }

// }

using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public bool soundOn = true;
    public bool vibrationOn = true;

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

            // 🔑 LOAD SAVED SETTINGS
            soundOn = PlayerPrefs.GetInt("SOUND_ON", 1) == 1;
            vibrationOn = PlayerPrefs.GetInt("VIBRATION_ON", 1) == 1;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Play(AudioClip clip)
    {
        // 🔑 RESPECT SOUND SETTING
        if (!soundOn || clip == null) return;

        source.PlayOneShot(clip);
    }

    public void Vibrate()
    {
        if (!vibrationOn) return;

    #if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
    #endif
    }
}

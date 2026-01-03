using UnityEngine;
using GoogleMobileAds.Api;

public class AdMobManager : MonoBehaviour
{
    public static AdMobManager Instance;

    // Google TEST rewarded ad unit ID (Android)
    private string rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";

    private RewardedAd rewardedAd;

    // ---------------- INTERSTITIAL (NEW) ----------------
    private string interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
    private InterstitialAd interstitialAd;

    private int gameOverCount = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        MobileAds.Initialize(_ =>
        {
            Debug.Log("AdMob initialized");
            LoadRewardedAd();
            LoadInterstitialAd();
        });
    }

    // ✅ LOAD (NEW STATIC API, NO Builder)
    public void LoadRewardedAd()
    {
        // Destroy old ad if exists
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        AdRequest request = new AdRequest();

        RewardedAd.Load(rewardedAdUnitId, request,
            (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load: " + error);
                    return;
                }

                rewardedAd = ad;
                Debug.Log("Rewarded ad loaded");
            });
    }

    // ✅ SHOW (NEW API)
    public void ShowRewardedAd()
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show(reward =>
            {
                Debug.Log("Reward earned");
                StackGameManager.Instance.OnRewardedAdSuccess();
            });

            // Preload next ad
            LoadRewardedAd();
        }
        else
        {
            Debug.LogWarning("Rewarded ad not ready");
            StackGameManager.Instance.FinalGameOver();
        }
    }

    public void LoadInterstitialAd()
    {
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        InterstitialAd.Load(interstitialAdUnitId, new AdRequest(),
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("Interstitial failed to load: " + error);
                    return;
                }

                interstitialAd = ad;
                Debug.Log("Interstitial ad loaded");
            });
    }

    // public void TryShowInterstitial()
    // {
    //     gameOverCount++;
    //     // Show every 2 game overs
    //     if (gameOverCount < 2)
    //         return;
    //     Debug.Log("Interstitial ad loaded check " + interstitialAd);
    //     if (interstitialAd != null && interstitialAd.CanShowAd())
    //     {
    //         interstitialAd.Show();
    //         gameOverCount = 0;
    //         LoadInterstitialAd(); // preload next
    //     }
    // }

    public void TryShowInterstitial()
    {
        gameOverCount++;
       // Show every 2 game overs
        if (gameOverCount < 2)
        return;

        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            Debug.Log("Showing interstitial");
            interstitialAd.Show();

            interstitialAd = null;   // consume the ad
            LoadInterstitialAd();    // preload next
        }
        else
        {
            Debug.Log("Interstitial not ready");
        }
    }


}

using UnityEngine;
using System.Collections;
using GoogleMobileAds.Api;
using Cysharp.Threading.Tasks;

public class AdManager : MonoBehaviour
{
    public string admobBannerID = "ca-app-pub-3940256099942544/6300978111";
    public string admobInterstitialID = "ca-app-pub-3940256099942544/1033173712";
    public string admobRewardID = "ca-app-pub-3940256099942544/5224354917";

    public static BannerView bannerAd;
    public static InterstitialAd interstitialAd;
    public static RewardedAd rewardedAd;

    public static bool isShowingAds = true;
    public static bool isInitialized = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        MobileAds.Initialize(async initStatus =>
        {
            rewardedAd = new RewardedAd(admobRewardID);
            bannerAd = new BannerView(admobBannerID, AdSize.Banner, AdPosition.Bottom);
            interstitialAd = new InterstitialAd(admobInterstitialID);
            AdRequest request = new AdRequest.Builder().Build();
            interstitialAd.LoadAd(request);
            rewardedAd.LoadAd(request);
            rewardedAd.OnUserEarnedReward += GetReward;
            await UniTask.WaitUntil(() => interstitialAd.IsLoaded() == true);
            await UniTask.WaitUntil(() => rewardedAd.IsLoaded() == true);
            isInitialized = true;
        });
    }

    public void GetReward(object sender, Reward args)
    {
        bannerAd.Destroy();
        interstitialAd.Destroy();
        isShowingAds = false;
        string type = args.Type;
        double amount = args.Amount;
        GameManager.Instance.scoreText.text = args.Amount.ToString();
    }


    public static void ShowRewardAD()
    {
        if (rewardedAd != null) rewardedAd.Show();
    }

    public static void ShowBannerAD()
    {
        if (isShowingAds)
        {
            AdRequest request = new AdRequest.Builder().Build();
            bannerAd.LoadAd(request);
        }
    }

    public static void ShowInterstitialAD()
    {
        if (isShowingAds)
        {
            interstitialAd.Show();
        }
    }
}

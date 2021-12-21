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

    public static int gameCount = 0;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        MobileAds.Initialize(async initStatus =>
        {
            await PreloadAds();
            isInitialized = true;
        });
    }

    async UniTask PreloadAds()
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
    }

    public async void GetReward(object sender, Reward args)
    {
        bannerAd.Destroy();
        interstitialAd.Destroy();
        //string type = args.Type;
        double gamesWithoutAds = args.Amount;
        isShowingAds = false;
        var againAds = gameCount + gamesWithoutAds;
        await UniTask.WaitUntil(() => gameCount == againAds);
        await PreloadAds();
        isShowingAds = true;
    }


    public static void ShowRewardAD()
    {
        if (isShowingAds) rewardedAd.Show();
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

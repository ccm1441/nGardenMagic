using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdsManager : MonoBehaviour
{
    private InterstitialAd interstitial;
    private RewardedAd rewardedAd;

    private bool _isAdUnitLoad = false;
    private bool _isRewardUnitLoad = false;

    // 배너광고
    //string adUnitId = "ca-app-pub-3940256099942544/1033173712";
    string adUnitId = "ca-app-pub-1676040129310540/2876969224";// 진짜 광고

    // 보상 광고
    //string rewardUnitId = "ca-app-pub-3940256099942544/5224354917";
    string rewardUnitId = "ca-app-pub-1676040129310540/4536152364";// 진짜 광고


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Initialize the Mobile Ads SDK.
        MobileAds.Initialize((initStatus) =>
        {
            InitAdUnit();
            InitRewardAdUnit();

            //this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
            //Dictionary<string, AdapterStatus> map = initStatus.getAdapterStatusMap();
            //foreach (KeyValuePair<string, AdapterStatus> keyValuePair in map)
            //{
            //    string className = keyValuePair.Key;
            //    AdapterStatus status = keyValuePair.Value;
            //    switch (status.InitializationState)
            //    {
            //        case AdapterState.NotReady:
            //            // The adapter initialization did not complete.
            //            MonoBehaviour.print("Adapter: " + className + " not ready.");
            //            break;
            //        case AdapterState.Ready:
            //            // The adapter was successfully initialized.
            //            MonoBehaviour.print("Adapter: " + className + " is initialized.");
            //            break;
            //    }
            //}
        });


    }


    // 전면광고 초기화
    private void InitAdUnit()
    {
        // 전면
        this.interstitial = new InterstitialAd(adUnitId);
        this.interstitial.OnAdClosed += HandleOnAdClosed;
        this.interstitial.OnAdFailedToLoad += HandleOnAdFailedToLoad;
        this.interstitial.OnAdLoaded += HandleOnAdLoaded;

        AdRequest request = new AdRequest.Builder().Build();
        this.interstitial.LoadAd(request);
    }

    // 보상광고 초기화
    private void InitRewardAdUnit()
    {
        // 보상
        this.rewardedAd = new RewardedAd(rewardUnitId);
        this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        this.rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        this.rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
        this.rewardedAd.OnAdClosed += HandleRewardedAdClosed;

        AdRequest requestReward = new AdRequest.Builder().Build();
        this.rewardedAd.LoadAd(requestReward);
    }


    // 전면 광고 - 게임에서 죽은 뒤 
    public void GameOverAds()
    {
        print("전면 광고 콜");
        // Create an empty ad request.       

        if (this.interstitial.IsLoaded()) this.interstitial.Show();
      //  else IngameUI.GetInstance().FailLoadADs(1);

    }
    public void HandleOnAdLoaded(object sender, EventArgs args)
    {
        _isAdUnitLoad = true;
        MonoBehaviour.print("광고 로드 완료,HandleAdLoaded event received");
    }

    public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        _isAdUnitLoad = false;
        MonoBehaviour.print("광고 로드 실패,HandleFailedToReceiveAd event received with message: "
                            + args.Message);
    }

    // 광고 닫았을때
    public void HandleOnAdClosed(object sender, EventArgs args)
    {
        print("광고 닫음");
        Time.timeScale = 1;
        interstitial.Destroy();
        IngameUI.GetInstance().ActiveAdGuard(false);
        GameManager.GetInstance().player.EndGameoberAds();
    }


    // 보상형 광고 - 광고 부활
    public void RebornAds()
    {
        if (this.rewardedAd.IsLoaded()) this.rewardedAd.Show();
      //  else IngameUI.GetInstance().FailLoadADs(2);

    }

    // 광고 리워드 받은거
    public void HandleUserEarnedReward(object sender, Reward args)
    {
        IngameUI.GetInstance().ActiveAdGuard(false);
        IngameUI.GetInstance().Reborn();
    }

    public void HandleRewardedAdLoaded(object sender, EventArgs args)
    {
        _isRewardUnitLoad = true;
        MonoBehaviour.print("광고 로드 완료 HandleRewardedAdLoaded event received");
    }

    public void HandleRewardedAdFailedToLoad(object sender, AdErrorEventArgs args)
    {
        _isRewardUnitLoad = false;
        MonoBehaviour.print(
            "광고 로드 실패HandleRewardedAdFailedToLoad event received with message: "
                             + args.Message);
    }

    public void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        IngameUI.GetInstance().RewardAdClosed();
        IngameUI.GetInstance().ActiveAdGuard(false);
    }
}

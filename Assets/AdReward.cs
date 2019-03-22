using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdReward : MonoBehaviour {

    private RewardBasedVideoAd rewardBasedVideo;

    bool IsRewarded = false;
    bool IsClosed = false;

    void Update()
    {
        if (IsRewarded && IsClosed)
        {
            IsRewarded = false;
            IsClosed = false;

            //ここで報酬を与える処理を実行
            GameObject.Find("GameRoot").GetComponent<PuzzleMain>().StatusData.Hand += 10;
            GameObject.Find("GameRoot").GetComponent<PuzzleMain>().StatusData.StatusUpdate();

        }
    }

    public void Start()
    {

#if UNITY_ANDROID
        string appId = "ca-app-pub-4228179100830730~2084688814"; //NekotanAndroid Admob AppID
#elif UNITY_IPHONE
        string appId = "ca-app-pub-4228179100830730~9684855854";//NekotaniPhone Admob AppID
#else
            string appId = "unexpected_platform";
#endif

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(appId);

        // Get singleton reward based video ad reference.
        this.rewardBasedVideo = RewardBasedVideoAd.Instance;

        
        // Called when an ad request has successfully loaded.
        rewardBasedVideo.OnAdLoaded += HandleRewardBasedVideoLoaded;
        // Called when an ad request failed to load.
        rewardBasedVideo.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;
        // Called when an ad is shown.
        rewardBasedVideo.OnAdOpening += HandleRewardBasedVideoOpened;
        // Called when the ad starts to play.
        rewardBasedVideo.OnAdStarted += HandleRewardBasedVideoStarted;
        // Called when the user should be rewarded for watching a video.
        rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
        // Called when the ad is closed.
        rewardBasedVideo.OnAdClosed += HandleRewardBasedVideoClosed;
        // Called when the ad click caused the user to leave the application.
        rewardBasedVideo.OnAdLeavingApplication += HandleRewardBasedVideoLeftApplication;
        
        this.RequestRewardBasedVideo();
        
    }


    private void RequestRewardBasedVideo()
    {
#if UNITY_ANDROID
        //string adUnitId = "ca-app-pub-4228179100830730/6265931815";//正しい
        string adUnitId = "ca-app-pub-3940256099942544/5224354917"; //サンプル
#elif UNITY_IPHONE
        //string adUnitId = "ca-app-pub-4228179100830730/8343854021";//正しい
        string adUnitId = "ca-app-pub-3940256099942544/1712485313";//サンプル
#else
            string adUnitId = "unexpected_platform";
#endif

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded video ad with the request.
        this.rewardBasedVideo.LoadAd(request, adUnitId);
    }

    public void HandleRewardBasedVideoLoaded(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoLoaded event received");
    }

    public void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        MonoBehaviour.print(
            "HandleRewardBasedVideoFailedToLoad event received with message: "
                             + args.Message);
    }

    public void HandleRewardBasedVideoOpened(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoOpened event received");
    }

    public void HandleRewardBasedVideoStarted(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoStarted event received");
    }

    public void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    {
        this.RequestRewardBasedVideo();
        MonoBehaviour.print("HandleRewardBasedVideoClosed event received");
        IsClosed = true;
    }

    public void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
        MonoBehaviour.print(
            "HandleRewardBasedVideoRewarded event received for "
                        + amount.ToString() + " " + type);

        IsRewarded = true;


    }

    public void HandleRewardBasedVideoLeftApplication(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoLeftApplication event received");
    }
    
    public void UserOptToWatchAd()
    {
        if (rewardBasedVideo.IsLoaded())
        {
#if UNITY_EDITOR
            IsRewarded = true;
            IsClosed = true;
            rewardBasedVideo.Show();
# else
            rewardBasedVideo.Show();
#endif

        }
    }
   

}

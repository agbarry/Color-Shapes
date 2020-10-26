using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; set; }

    private string appID = "ca-app-pub-7689563293462257~4387629542";
                            
    private BannerView bannerView;
    private string bannerID = "ca-app-pub-7689563293462257/3896674958";

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        MobileAds.Initialize(appID);
    }

    public void ShowBanner() {
        bannerView = new BannerView(bannerID, AdSize.Banner, AdPosition.Bottom);

        AdRequest request = new AdRequest.Builder().Build();

        bannerView.LoadAd(request);
        
        bannerView.Show();
    }
    public void HideBanner() {
        bannerView.Hide();
    }

    public void HandleRewardedAdLoaded(object sender, EventArgs args) {
        MonoBehaviour.print("HandleRewardBasedVideoLoaded event received");
    }

    public void HandleRewardedAdFailedToLoad(object sender, AdFailedToLoadEventArgs args) {
        MonoBehaviour.print(
            "HandleRewardedAdFailedToLoad event received with message: "
                             + args.Message);
    }

    public void HandleUserEarnedReward(object sender, Reward args) {
        
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;

public class GoogleMobileAdsDemoScript : MonoBehaviour
{
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
    }
}
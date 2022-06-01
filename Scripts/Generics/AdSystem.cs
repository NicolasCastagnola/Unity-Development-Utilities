using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Events;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

public class AdSystem : MonoBehaviour, IUnityAdsListener
{
	private static AdSystem m_Instance;

	#if UNITY_IOS
	private string gameId = "";
	#elif UNITY_ANDROID
	private string gameId = "";
	#elif UNITY_EDITOR
	private string gameId = "";
	#endif

	public bool testMode = false;

	public bool shouldShowFull = false;
	public bool shouldShowBanner = false;
	public bool PermaHideBanner = false;

	public UnityAction onAdFinished;

	public static AdSystem Instance {
		get {
			if (m_Instance != null) return m_Instance;

			GameObject inst = new GameObject("AdSystem");
			m_Instance = inst.AddComponent<AdSystem>();
			DontDestroyOnLoad(inst);

			return m_Instance;
		}
	}
	void Awake() {
		SceneManager.activeSceneChanged += ChangedActiveScene;
		m_Instance = this;
	}

	void OnDestroy() {
		m_Instance = null;
	}

	private void ChangedActiveScene(Scene current, Scene next) {
		this.shouldShowBanner = false;
		this.shouldShowFull = false;

		this.HideAds();
	}

	public void Initialize() {
		if (Advertisement.isInitialized) return;

		Advertisement.AddListener(AdSystem.Instance);
		Advertisement.Initialize(gameId, false);
	}

	void Update() {
		if (Advertisement.isShowing) return;
		
		if (Advertisement.GetPlacementState("video") == PlacementState.Ready && shouldShowFull) {
			Advertisement.Show("video");
			shouldShowFull = false;
		}

		// if (Advertisement.GetPlacementState("banner") == PlacementState.Ready && shouldShowBanner) {
		// 	Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
		// 	Advertisement.Banner.Show("banner");
		// 	shouldShowBanner = false;
		// }

		if (!shouldShowBanner) {
			Advertisement.Banner.Hide();
		}
	}

	public void ShowFullAd() {
		shouldShowFull = true;
		AnalyticsEvent.AdStart(false);
	}

	public void ShowBannerAd() {
		PermaHideBanner = false;
		shouldShowBanner = true;
		StartCoroutine(ShowBannerWhenReady());
	}

	IEnumerator ShowBannerWhenReady() {
		// while (Advertisement.GetPlacementState("banner") != PlacementState.Ready) {
		// 	yield return new WaitForSeconds(0.25f);
		// }
		while (!Advertisement.IsReady ("banner")) {
			yield return new WaitForSeconds (0.5f);
		}

		if (shouldShowBanner) {
			Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
			Advertisement.Banner.Show("banner");
		}
	}

	public void HideAds() {
		PermaHideBanner = true;
		shouldShowBanner = false;
		Advertisement.Banner.Hide();
	}

	// Implement IUnityAdsListener interface methods:
	public void OnUnityAdsDidFinish (string placementId, ShowResult showResult) {
		// Define conditional logic for each ad completion status:
		if (showResult == ShowResult.Finished) {
			// Reward the user for watching the ad to completion.
			AnalyticsEvent.AdComplete(false);
		} else if (showResult == ShowResult.Skipped) {
			// Do not reward the user for skipping the ad.
			AnalyticsEvent.AdSkip(false);
		} else if (showResult == ShowResult.Failed) {
			Debug.LogWarning("The ad did not finish due to an error.");
		}

		Debug.Log("AD Finished!");
		Debug.Log(onAdFinished);

		if (onAdFinished != null) onAdFinished.Invoke();
	}

	public void OnUnityAdsReady (string placementId) {
		// If the ready Placement is rewarded, show the ad:
		// if (placementId == myPlacementId) {
		// 	Advertisement.Show (myPlacementId);
		// }

		if (placementId == "banner" && (PermaHideBanner || !shouldShowBanner)) Advertisement.Banner.Hide();

		Debug.Log("AD Ready! " + placementId);
	}

	public void OnUnityAdsDidError (string message) {
		// Log the error.
		Debug.Log("AD Errored! " + message);
		if (onAdFinished != null) onAdFinished.Invoke();
	}

	public void OnUnityAdsDidStart (string placementId) {
		Debug.Log("AD Start! " + placementId);
		// Optional actions to take when the end-users triggers an ad.
	} 
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayFeeds : MonoBehaviour {

	/// <summary>
	/// TODO: Include blob detetction initialization parameters
	/// </summary>
	/// 
	/// principle fucniton should be here

	PeopleTracking peopleTrackingScript;

	// Display feeds
	public RawImage rgbFrameDisplay;
	public RawImage rawDepthFrameDisplay;
	public RawImage rangeLimitedDepthFrameDisplay;
	public RawImage blobsBasedDepthFrameDisplay;
	public RawImage visualizationDepthFrameDisplay;

	// Staging area (textures) for displaying feeds
	private Texture2D rgbFrameTexture;
	private Texture2D rawDepthFrameTexture;
	private Texture2D rangeLimitedDepthFrameTexture;
	private Texture2D blobsBasedDepthFrameTexture;
	private Texture2D visualizationDepthFrameTexture;

	// Feed dimensions
	private int rgbFrameHeight = 0;
	private int rgbFrameWidth = 0;
	private long rgbDataSize = 0;
	private int depthFrameHeight = 0;
	private int depthFrameWidth = 0;
	private long depthDataSize = 0;

	private bool initializationComplete = false;

	void InitializeFeeds() {
		initializationComplete = false;
		rgbFrameHeight = peopleTrackingScript.rgbFrameHeight;
		rgbFrameWidth = peopleTrackingScript.rgbFrameWidth;
		depthFrameHeight = peopleTrackingScript.depthFrameHeight;
		depthFrameWidth = peopleTrackingScript.depthFrameWidth;
		rgbDataSize = peopleTrackingScript.rgbDataSize;
		depthDataSize = peopleTrackingScript.depthDataSize;

		if (!(rgbFrameWidth > 0 && rgbFrameHeight > 0 && rgbDataSize > 0 && depthFrameWidth > 0 && depthFrameHeight > 0 && depthDataSize > 0)) {
			initializationComplete = false;
			return;
		} 

		rgbFrameTexture = new Texture2D (rgbFrameWidth, rgbFrameHeight, TextureFormat.BGRA32, false);
		rawDepthFrameTexture = new Texture2D (depthFrameWidth, depthFrameHeight, TextureFormat.Alpha8, false);
		rangeLimitedDepthFrameTexture = new Texture2D (depthFrameWidth, depthFrameHeight, TextureFormat.Alpha8 , false);
		blobsBasedDepthFrameTexture = new Texture2D (depthFrameWidth, depthFrameHeight, TextureFormat.Alpha8, false);
		visualizationDepthFrameTexture = new Texture2D (depthFrameWidth, depthFrameHeight, TextureFormat.BGRA32, false);

		initializationComplete = true;
	}

	void Start () {
		peopleTrackingScript = (PeopleTracking)FindObjectOfType<PeopleTracking>();

		if (!peopleTrackingScript.copyFeedsData)
			Destroy(this);

		InitializeFeeds();
	}
	
	void Update () {

		if (!initializationComplete) {
			InitializeFeeds();
			return;
		}

		// Show RGB Frame
		rgbFrameTexture.LoadRawTextureData(peopleTrackingScript.returnedRGBData);
		rgbFrameTexture.Apply();
		rgbFrameDisplay.texture = rgbFrameTexture;

		// Show Raw Depth Frame
		rawDepthFrameTexture.LoadRawTextureData(peopleTrackingScript.returnedRawDepthData);
		rawDepthFrameTexture.Apply();
		rawDepthFrameDisplay.texture = rawDepthFrameTexture;

		// Show Range-limited Depth Frame
		rangeLimitedDepthFrameTexture.LoadRawTextureData(peopleTrackingScript.returnedRangeLimitedDepthData);
		rangeLimitedDepthFrameTexture.Apply();
		rangeLimitedDepthFrameDisplay.texture = rangeLimitedDepthFrameTexture;

		// Show Blobs-based Depth Frame
		blobsBasedDepthFrameTexture.LoadRawTextureData(peopleTrackingScript.returnedBlobsBasedDepthData);
		blobsBasedDepthFrameTexture.Apply();
		blobsBasedDepthFrameDisplay.texture = blobsBasedDepthFrameTexture;

		// Show Visualization Depth Frame
		visualizationDepthFrameTexture.LoadRawTextureData(peopleTrackingScript.returnedVisualizationDepthData);
		visualizationDepthFrameTexture.Apply();
		visualizationDepthFrameDisplay.texture = visualizationDepthFrameTexture;
	}
}

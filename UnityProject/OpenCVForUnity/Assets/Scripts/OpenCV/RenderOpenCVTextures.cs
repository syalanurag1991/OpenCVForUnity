using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RenderOpenCVTextures : MonoBehaviour {

	/// <summary>
	/// TODO: Include blob detetction initialization parameters
	/// </summary>

	// People tracking object
	private OpenCVInterface opencvInterfaceInstance;

	// Display feeds
	public RawImage opencvRGBFrameDisplay;
	public RawImage opencvDepthFrameDisplay;
	public RawImage opencvGradedDepthFrameDisplay;
	public RawImage opencvBlobsBasedDepthFrameDisplay;
	public RawImage opencvVisualizationDepthFrameDisplay;

	// Depth frame cleaning parameters
	public int numberOfFrames = 30;
	public float currentFrameWeight = 0.4f;
	public float resizeFactor = 0.5f;
	public byte binarizingCutoff = 240;

	// Storage (array) for holding Graded depth field
	private byte[] copyOfopencvDepthDataArray;
	private byte[] opencvGradedDepthDataArray;

	// Staging area (textures) for displaying feeds
	private Texture2D opencvRGBFrameTexture;
	private Texture2D opencvDepthFrameTexture;
	private Texture2D opencvGradedDepthFrameTexture;
	private Texture2D opencvBlobsBasedDepthFrameTexture;
	private Texture2D opencvVisualizationDepthFrameTexture;

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

		rgbFrameWidth = KinectWrapper.Constants.ColorImageWidth;
		rgbFrameHeight = KinectWrapper.Constants.ColorImageHeight;
		depthFrameWidth = KinectWrapper.Constants.DepthImageWidth;
		depthFrameHeight = KinectWrapper.Constants.DepthImageHeight;
		rgbDataSize = rgbFrameWidth * rgbFrameHeight * 3;
		depthDataSize = depthFrameWidth * depthFrameHeight;

		if (!(rgbFrameWidth > 0 && rgbFrameHeight > 0 && rgbDataSize > 0 && depthFrameWidth > 0 && depthFrameHeight > 0 && depthDataSize > 0)) {
			initializationComplete = false;
			return;
		} 

		copyOfopencvDepthDataArray = new byte[depthDataSize];
		opencvGradedDepthDataArray = new byte[depthDataSize * 4];

		opencvRGBFrameTexture = new Texture2D(rgbFrameWidth, rgbFrameHeight, TextureFormat.BGRA32, false);
		opencvDepthFrameTexture = new Texture2D(depthFrameWidth, depthFrameHeight, TextureFormat.Alpha8, false);
		opencvGradedDepthFrameTexture = new Texture2D(depthFrameWidth, depthFrameHeight, TextureFormat.BGRA32, false);
		opencvBlobsBasedDepthFrameTexture = new Texture2D(depthFrameWidth, depthFrameHeight, TextureFormat.Alpha8, false);
		opencvVisualizationDepthFrameTexture = new Texture2D(depthFrameWidth, depthFrameHeight, TextureFormat.RGBA32, false);

		initializationComplete = true;
	}

	void Start() {
		opencvInterfaceInstance = OpenCVInterface.Instance;
		if (!opencvInterfaceInstance.copyFeedsData)
			Destroy(this);
		InitializeFeeds();
	}

	void Update() {

		if (!initializationComplete) {
			InitializeFeeds ();
			return;
		}

		if (Input.GetKeyDown (KeyCode.Escape))
			Application.Quit ();

		//OpenCVInterface.Instance.SetDepthCleaningParameters(numberOfFrames, currentFrameWeight, resizeFactor, binarizingCutoff);
		PrepareGradedDepthFeed();

		// Show RGB Frame received from to OpenCV DLL
		if ((opencvRGBFrameTexture != null) && (opencvRGBFrameDisplay != null)) {
			opencvRGBFrameTexture.LoadRawTextureData (opencvInterfaceInstance.returnedRGBData);
			opencvRGBFrameTexture.Apply ();
			opencvRGBFrameDisplay.texture = opencvRGBFrameTexture;
		}

		// Show Depth Frame received from OpenCV DLL (This can be range limited)
		if ((opencvDepthFrameTexture != null) && (opencvDepthFrameDisplay != null)) {
			opencvDepthFrameTexture.LoadRawTextureData (opencvInterfaceInstance.returnedDepthData);
			opencvDepthFrameTexture.Apply ();
			opencvDepthFrameDisplay.texture = opencvDepthFrameTexture;
		}

		// Show Graded Depth Frame
		if ((opencvGradedDepthFrameTexture != null) && (opencvGradedDepthFrameDisplay != null)) {
			opencvGradedDepthFrameTexture.LoadRawTextureData (opencvGradedDepthDataArray);
			opencvGradedDepthFrameTexture.Apply ();
			opencvGradedDepthFrameDisplay.texture = opencvGradedDepthFrameTexture;
		}

		// Show Blobs-based Depth Frame received from OpenCV DLL
		if ((opencvBlobsBasedDepthFrameTexture != null) && (opencvBlobsBasedDepthFrameDisplay != null)) {
			opencvBlobsBasedDepthFrameTexture.LoadRawTextureData (opencvInterfaceInstance.returnedBlobsBasedDepthData);
			opencvBlobsBasedDepthFrameTexture.Apply ();
			opencvBlobsBasedDepthFrameDisplay.texture = opencvBlobsBasedDepthFrameTexture;
		}

		// Show Visualization Depth Frame received from OpenCV DLL
		if ((opencvVisualizationDepthFrameTexture != null) && (opencvVisualizationDepthFrameDisplay != null)) {
			opencvVisualizationDepthFrameTexture.LoadRawTextureData (opencvInterfaceInstance.returnedVisualizationDepthData);
			opencvVisualizationDepthFrameTexture.Apply ();
			opencvVisualizationDepthFrameDisplay.texture = opencvVisualizationDepthFrameTexture;
		}
	}

	void PrepareGradedDepthFeed() {
        if (opencvInterfaceInstance.returnedDepthData == null) {
            Debug.Log("OpenCV module is null");
            return;
        }

        if (KinectManager.Instance == null) {
            Debug.Log("Kinect module is null");
            return;
        }
		
        copyOfopencvDepthDataArray = opencvInterfaceInstance.returnedDepthData;
		for (int i = 0; i < copyOfopencvDepthDataArray.Length; i++) {
			float tempPixel = (float)copyOfopencvDepthDataArray[i];
            if (tempPixel < KinectManager.Instance.level_min_byte) {																	// black
				opencvGradedDepthDataArray [4 * i] = 0;
				opencvGradedDepthDataArray [4 * i + 1] = 0;
				opencvGradedDepthDataArray [4 * i + 2] = 0;
				opencvGradedDepthDataArray [4 * i + 3] = 255;
			} else if (tempPixel >= KinectManager.Instance.level_min_byte && tempPixel < KinectManager.Instance.level_1_byte) {		// red
				opencvGradedDepthDataArray [4 * i] = 204;
				opencvGradedDepthDataArray [4 * i + 1] = 55;
				opencvGradedDepthDataArray [4 * i + 2] = 105;
				opencvGradedDepthDataArray [4 * i + 3] = 255;
			} else if (tempPixel >= KinectManager.Instance.level_1_byte && tempPixel < KinectManager.Instance.level_2_byte) {			// yellow
				opencvGradedDepthDataArray [4 * i] = 214;
				opencvGradedDepthDataArray [4 * i + 1] = 192;
				opencvGradedDepthDataArray [4 * i + 2] = 29;
				opencvGradedDepthDataArray [4 * i + 3] = 255;
			} else if (tempPixel >= KinectManager.Instance.level_2_byte && tempPixel < KinectManager.Instance.level_3_byte) {			// cyan
				opencvGradedDepthDataArray [4 * i] = 28;
				opencvGradedDepthDataArray [4 * i + 1] = 214;
				opencvGradedDepthDataArray [4 * i + 2] = 208;
				opencvGradedDepthDataArray [4 * i + 3] = 255;
			} else if (tempPixel >= KinectManager.Instance.level_3_byte && tempPixel < KinectManager.Instance.level_max_byte) {		// blue
				opencvGradedDepthDataArray [4 * i] = 83;
				opencvGradedDepthDataArray [4 * i + 1] = 109;
				opencvGradedDepthDataArray [4 * i + 2] = 254;
				opencvGradedDepthDataArray [4 * i + 3] = 255;
			} else {																												// pure red
				opencvGradedDepthDataArray [4 * i] = 0;
				opencvGradedDepthDataArray [4 * i + 1] = 0;
				opencvGradedDepthDataArray [4 * i + 2] = 0;
				opencvGradedDepthDataArray [4 * i + 3] = 255;
			}
		}
	}

}

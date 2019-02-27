using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;


public class KinectManager : MonoBehaviour
{
    OpenCVInterface opencvInterfaceScript;

    // UI Text to show messages.
    public Text UILog;
	
	// How high off the ground is the sensor (in meters).
	public float kinectPlacementHeight = 1.0f;

	// Kinect elevation angle (in degrees)
	public int kinectMotorAngle = 0;
	
	// Bool to keep track of whether Kinect has been initialized
	private bool kinectInitialized = false;

    // Image stream handles for the kinect
    private IntPtr colorStreamHandle;
    private IntPtr depthStreamHandle;
	private bool rawColorStreamUpdated = false;
	private bool rawDepthDataUpdated = false;

    private ushort[] previousCorrectedDepthData;

    // Kinect data storage objects
    private KinectData lowResolutionKinectData;
	private KinectData fullResolutionKinectData;
	float scaleX, scaleY;

    // Variables for producing graded depth view
    [HideInInspector]
    public ushort maxDistanceShort = ushort.MinValue;
    [HideInInspector]
    public int maxValuePixel = int.MinValue;
    [HideInInspector]
    public ushort minDistanceShort = ushort.MaxValue;
    [HideInInspector]
    public int minValuePixel = int.MaxValue;

    [HideInInspector]
    public float level_min_byte;
    [HideInInspector]
    public float level_max_byte;
    [HideInInspector]
    public float level_1_byte;
    [HideInInspector]
    public float level_2_byte;
    [HideInInspector]
    public float level_3_byte;

    public bool changeLevelsManually = false;
    public ushort level_min_short = ushort.MinValue;
    public ushort level_max_short = (ushort.MaxValue) >> 4;
    public ushort level_1_short;
    public ushort level_2_short;
    public ushort level_3_short;
    private ushort level_step_short;
    private float totalLevelDifference;
    public Color32 color_min, color_1, color_2, color_3, color_4;

	// UI options
    public bool showStatsInLog = false;
    public bool processNormalizedDepth = false;

    // Kinect to world mapping
	private Matrix4x4 kinectToWorld, flipMatrix;
	
    // returns the single KinectManager instance
    private static KinectManager instance;
    public static KinectManager Instance {
        get
        {
            return instance;
        }
    }
	
	// checks if Kinect is initialized and ready to use. If not, there was an error during Kinect-sensor initialization
	public static bool IsKinectInitialized() {
		return instance != null ? instance.kinectInitialized : false;
	}
	
	// checks if Kinect is initialized and ready to use. If not, there was an error during Kinect-sensor initialization
	public bool IsInitialized() {
		return kinectInitialized;
	}
	
	// returns raw depth data
	public ushort[] GetRawDepthData() {
		return fullResolutionKinectData.RawDepths;
	}

	// returns interpolated low resolution corrected depth data
	public ushort[] GetLowResolutionDepthData() {
		return lowResolutionKinectData.CorrectedDepths;
	}

    // returns the corrected depth data
    public ushort[] GetCorrectedDepthData() {
        return fullResolutionKinectData.CorrectedDepths;
    }
	
	// returns the depth data for a specific pixel
	public ushort GetDepthForPixel(int x, int y) {
		int index = y * KinectWrapper.Constants.DepthImageWidth + x;
		
		if(index >= 0 && index < fullResolutionKinectData.CorrectedDepths.Length)
			return fullResolutionKinectData.CorrectedDepths[index];
		else
			return 0;
	}
	
	public Vector2 GetColorCorrespondingToDepthPixelPosition(Vector2 posDepth) {
		int cx, cy;

		KinectWrapper.NuiImageViewArea pcViewArea = new KinectWrapper.NuiImageViewArea {
            eDigitalZoom = 0,
            lCenterX = 0,
            lCenterY = 0
        };
		
		KinectWrapper.NuiImageGetColorPixelCoordinatesFromDepthPixelAtResolution(
			KinectWrapper.Constants.ColorImageResolution,
			KinectWrapper.Constants.DepthImageResolution,
			ref pcViewArea,
			(int)posDepth.x, (int)posDepth.y, GetDepthForPixel((int)posDepth.x, (int)posDepth.y),
			out cx, out cy);
		
		return new Vector2(cx, cy);
	}

    // Returns raw color image data
	public Color32[] GetRGBFrameData() {
		return fullResolutionKinectData.RegisteredColorStreamColors;
	}
	
    //---------------------------------- END OF PUBLIC FUNCTIONS -----------------------------------------------------------//

    void Awake() {
        
        KinectCoordinatesAdjustment = new KinectWrapper.NuiImageViewArea
        {
            eDigitalZoom = 0,
            lCenterX = 0,
            lCenterY = 0
        };

		int hr = 0;
		
		try
		{
			hr = KinectWrapper.NuiInitialize(KinectWrapper.NuiInitializeFlags.UsesSkeleton |
				KinectWrapper.NuiInitializeFlags.UsesDepthAndPlayerIndex |
				KinectWrapper.NuiInitializeFlags.UsesColor);
            if (hr != 0)
			{
            	throw new Exception("NuiInitialize Failed");
			}
			
			depthStreamHandle = IntPtr.Zero;
			hr = KinectWrapper.NuiImageStreamOpen(KinectWrapper.NuiImageType.DepthAndPlayerIndex, 
				KinectWrapper.Constants.DepthImageResolution, 0, 2, IntPtr.Zero, ref depthStreamHandle);
			if (hr != 0)
			{
				throw new Exception("Cannot open depth stream");
			}
			
			colorStreamHandle = IntPtr.Zero;
			hr = KinectWrapper.NuiImageStreamOpen(KinectWrapper.NuiImageType.Color, 
				KinectWrapper.Constants.ColorImageResolution, 0, 2, IntPtr.Zero, ref colorStreamHandle);
			if (hr != 0)
			{
				throw new Exception("Cannot open color stream");
			}

			// set kinect elevation angle
			KinectWrapper.NuiCameraElevationSetAngle(kinectMotorAngle);
			
			//create the transform matrix that converts from kinect-space to world-space
			Quaternion quatTiltAngle = new Quaternion();
			quatTiltAngle.eulerAngles = new Vector3(-kinectMotorAngle, 0.0f, 0.0f);
			
			// transform matrix - kinect to world
			kinectToWorld.SetTRS(new Vector3(0.0f, kinectPlacementHeight, 0.0f), quatTiltAngle, Vector3.one);
			flipMatrix = Matrix4x4.identity;
			flipMatrix[2, 2] = -1;
			DontDestroyOnLoad(gameObject);
		}
		catch(DllNotFoundException e)
		{
			string message = "Please check the Kinect SDK installation.";
			Debug.LogError(message);
			Debug.LogError(e.ToString());
			if(UILog != null)
				UILog.text = message;
				
			return;
		}
		catch (Exception e)
		{
			string message = e.Message + " - " + KinectWrapper.GetNuiErrorString(hr);
			Debug.LogError(message);
			Debug.LogError(e.ToString());
			if(UILog != null)
				UILog.text = message;
			return;
		}
		
        InitializeFeeds();
        InitializeGradedDepthStreamColors();

        instance = this;
		
		if(UILog != null)
		{
			UILog.text = "Kinect is initialized";
		}

        Debug.Log("Kinect is initialized");
		kinectInitialized = true;
	}

    void Update()
    {
        if (kinectInitialized)
        {
            AdjustGradedDepthLevels();
			rawColorStreamUpdated = KinectWrapper.PollColor(colorStreamHandle, ref fullResolutionKinectData.RawColorStreamColors);
			rawDepthDataUpdated = KinectWrapper.PollDepth(depthStreamHandle, KinectWrapper.Constants.IsNearMode, ref fullResolutionKinectData.RawDepths);
			if (depthStreamHandle != IntPtr.Zero && colorStreamHandle != IntPtr.Zero && rawColorStreamUpdated && rawDepthDataUpdated)
            {
                ProcessRawDepthData();
				ProduceLowResolutionData();
            }
			if (Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();
        }
    }

    void InitializeGradedDepthStreamColors()
    {
        color_min = new Color32((byte)0, (byte)0, (byte)0, (byte)255);
		color_1 = new Color32((byte)105, (byte)55, (byte)204, (byte)255);
		color_2 = new Color32((byte)29, (byte)192, (byte)214, (byte)255);
        color_3 = new Color32((byte)208, (byte)214, (byte)29, (byte)255);
		color_4 = new Color32((byte)254, (byte)109, (byte)83, (byte)255);
    }

    void InitializeFeeds()
    {
		lowResolutionKinectData = new KinectData(KinectWrapper.Constants.TargetWidth, KinectWrapper.Constants.TargetHeight);
		fullResolutionKinectData = new KinectData(KinectWrapper.Constants.ColorImageWidth, KinectWrapper.Constants.ColorImageHeight);

		previousCorrectedDepthData = new ushort[fullResolutionKinectData.Size];
		scaleX = (float)lowResolutionKinectData.Width / (float)fullResolutionKinectData.Width;
		scaleY = (float)lowResolutionKinectData.Height / (float)fullResolutionKinectData.Height;
    }

    public KinectData GetLowResolutionKinectDataInstance()
	{
		return lowResolutionKinectData;
	}

	public KinectData GetFullResolutionKinectDataInstance()
	{
		return fullResolutionKinectData;
	}

    void AdjustGradedDepthLevels()
    {
        level_min_byte = 0;
        level_max_byte = 255;
        totalLevelDifference = (float)(level_max_short - level_min_short);
        level_1_byte = (int)(255f * (((float)(level_1_short - level_min_short)) / totalLevelDifference));
        level_2_byte = (int)(255f * (((float)(level_2_short - level_min_short)) / totalLevelDifference));
        level_3_byte = (int)(255f * (((float)(level_3_short - level_min_short)) / totalLevelDifference));

        if (!changeLevelsManually)
        {
            level_step_short = (ushort)(totalLevelDifference / 4);
            level_1_short = (ushort)(level_min_short + 1 * level_step_short);
            level_2_short = (ushort)(level_min_short + 2 * level_step_short);
            level_3_short = (ushort)(level_min_short + 3 * level_step_short);
        }
    }
	
	// Creating low resolution image and other data using Bilinear Interpolation
	void ProduceLowResolutionData ()
	{
		int i = 0, j = 0;
		float deltaY = 0, deltaX = 0;
		int[] indexesOfNeighborPixels = new int[4];
		KinectData fourNeighborPixels = new KinectData (2, 2);
		for (int index = 0; index < lowResolutionKinectData.Size; index++) {
			BilinearInterpolation.GetBilinearInterpolationParameters (ref fullResolutionKinectData.Width, ref fullResolutionKinectData.Height, ref lowResolutionKinectData.Width, ref lowResolutionKinectData.Height,
				ref scaleX, ref scaleY, ref index, ref deltaX, ref deltaY, ref indexesOfNeighborPixels);
			if (lowResolutionKinectData != null)
			{
				GetFourNeighborPixels (ref indexesOfNeighborPixels, ref fullResolutionKinectData, ref fourNeighborPixels);
				BilinearInterpolation.GetBilinearInterpolatedValue(ref deltaX, ref deltaY, ref fourNeighborPixels.CorrectedDepths, ref lowResolutionKinectData.CorrectedDepths[index]);
				BilinearInterpolation.GetBilinearInterpolatedValue(ref deltaX, ref deltaY, ref fourNeighborPixels.NormalizedDepths, ref lowResolutionKinectData.NormalizedDepths[index]);
				BilinearInterpolation.GetBilinearInterpolatedValue(ref deltaX, ref deltaY, ref fourNeighborPixels.DepthStreamColors, ref lowResolutionKinectData.DepthStreamColors[index]);
				BilinearInterpolation.GetBilinearInterpolatedValue(ref deltaX, ref deltaY, ref fourNeighborPixels.RawColorStreamColors, ref lowResolutionKinectData.RawColorStreamColors[index]);
				BilinearInterpolation.GetBilinearInterpolatedValue(ref deltaX, ref deltaY, ref fourNeighborPixels.GradedDepthStreamColors, ref lowResolutionKinectData.GradedDepthStreamColors[index]);
				BilinearInterpolation.GetBilinearInterpolatedValue(ref deltaX, ref deltaY, ref fourNeighborPixels.RegisteredColorStreamColors, ref lowResolutionKinectData.RegisteredColorStreamColors[index]);
			}
		}
	}

	void GetFourNeighborPixels (ref int[] inputIndexesOfNeighborPixels, ref KinectData sourceKinectData, ref KinectData resultNeighborPixels)
	{
		for (int i = 0; i < 4; i++)
		{
			resultNeighborPixels.RawDepths[i] = sourceKinectData.RawDepths[inputIndexesOfNeighborPixels[i]];
			resultNeighborPixels.CorrectedDepths[i] = sourceKinectData.CorrectedDepths[inputIndexesOfNeighborPixels[i]];
			resultNeighborPixels.NormalizedDepths[i] = GetNormalizedDepthForPixel(resultNeighborPixels.CorrectedDepths[i]);
			resultNeighborPixels.RawColorStreamColors[i] = sourceKinectData.RawColorStreamColors[inputIndexesOfNeighborPixels[i]];
			GetColorForDepthStream (ref resultNeighborPixels.NormalizedDepths[i], ref resultNeighborPixels.DepthStreamColors[i]);
			GetColorForGradedDepthStreamPixel(ref sourceKinectData.CorrectedDepths[inputIndexesOfNeighborPixels[i]], ref resultNeighborPixels.GradedDepthStreamColors[i]);
			GetColorForRegsiteredColorStreamPixel(ref inputIndexesOfNeighborPixels[i], ref sourceKinectData, ref resultNeighborPixels.RegisteredColorStreamColors[i]);
		}
		return;
	}

	float GetNormalizedDepthForPixel (ushort correctedDepth)
	{
		if (correctedDepth < level_max_short)
        {
            if (processNormalizedDepth)
            {
				if (correctedDepth > level_min_short)
					return (255f * (((float)(correctedDepth - level_min_short)) / totalLevelDifference));
            }
            else
				return (255f * ((float)correctedDepth / (float)level_max_short));
        }

        return 0f;
	}

	// Structure needed by the coordinate mapper
    private KinectWrapper.NuiImageViewArea KinectCoordinatesAdjustment;
    void ProcessRawDepthData () {
		if(fullResolutionKinectData.RawDepths.Length != fullResolutionKinectData.Size)
			Debug.Log ("Current depth data is lesser than alloted size");
		ushort correctedDistance = 0;
		for (int i = 0; i < fullResolutionKinectData.Size; i++)
		{
			correctedDistance = (ushort)(fullResolutionKinectData.RawDepths [i] >> 3);
			previousCorrectedDepthData [i] = fullResolutionKinectData.CorrectedDepths[i];
			fullResolutionKinectData.CorrectedDepths[i] = correctedDistance;
		}

		if (showStatsInLog)
		{
			// Find Min and Max distances
			FindMinMaxDistancePerceived (correctedDistance);
			Debug.Log("Depth stream updated: " + rawColorStreamUpdated);
			Debug.Log("Color stream updated: " + rawDepthDataUpdated);
            Debug.Log("Max pixel value observed (byte): " + maxValuePixel);
            Debug.Log("Min pixel value observed (byte): " + minValuePixel);
            Debug.Log("Max distance observed  (ushort): " + maxDistanceShort);
            Debug.Log("Min distance observed  (ushort): " + minDistanceShort);
			Debug.Log("Source color and depth streams @" + fullResolutionKinectData.Width + "x" + fullResolutionKinectData.Height + " (" + fullResolutionKinectData.Size + " bytes/frame)");
			Debug.Log("Target color and depth streams @" + lowResolutionKinectData.Width + "x" + lowResolutionKinectData.Height + " (" + lowResolutionKinectData.Size + " bytes/frame)");
        }

        if (OpenCVInterface.Instance != null)
        {
            //OpenCVInterface.Instance.ConvertRGBDataToOpenCVFormat(rgbDataArray);
            //OpenCVInterface.Instance.ConvertDepthDataToOpenCVFormat(depthDataArray);
        }
    }

    // Find minimum and maximum shortvalues received
    void FindMinMaxDistancePerceived (ushort correctedDistance) {
		if (correctedDistance > maxDistanceShort)
			maxDistanceShort = correctedDistance;
		if (correctedDistance < minDistanceShort)
			minDistanceShort = correctedDistance;
			return;
	}

	void GetColorForDepthStream (ref float normalizedDepth, ref Color32 result)
	{
		result.r = (byte)normalizedDepth;
		result.g = (byte)normalizedDepth;
		result.b = (byte)normalizedDepth;
		result.a = 255;
		return;
	}

	// Process for graded depth stream
	void GetColorForGradedDepthStreamPixel (ref ushort correctedDistance, ref Color32 result) {
		if (correctedDistance < level_min_short || correctedDistance > level_max_short)
			result = color_min;											// black
		else if (correctedDistance < level_1_short)
			result = color_1;											// red
		else if (correctedDistance < level_2_short)
			result = color_2;											// yellow
		else if (correctedDistance < level_3_short)
			result = color_3;											// cyan
		else
			result = color_4;											// blue
		return;
	}

	void GetColorForRegsiteredColorStreamPixel (ref int index, ref KinectData sourceKinectData, ref Color32 result)
	{
		result = new Color32 (0, 0, 0, 255);
		if (fullResolutionKinectData.RawColorStreamColors != null) {
			int x = index % sourceKinectData.Width;
			int y = index / sourceKinectData.Width;

			int cx, cy;
			int hr = KinectWrapper.NuiImageGetColorPixelCoordinatesFromDepthPixelAtResolution (
				KinectWrapper.Constants.ColorImageResolution,
				KinectWrapper.Constants.DepthImageResolution,
				ref KinectCoordinatesAdjustment,
				x, y, sourceKinectData.RawDepths[index],
				out cx, out cy);

			if (hr == 0) {
				int colorIndex = cy * sourceKinectData.Width + cx;
				if (colorIndex >= 0 && colorIndex < sourceKinectData.Size)
					result = sourceKinectData.RawColorStreamColors [colorIndex];
			}
		}
		return;
	}

    public float[] GetLowResolutionNormalizedDepthData () {
    	return lowResolutionKinectData.NormalizedDepths;
	}

	public float[] GetNormalizedDepthData () {
		return fullResolutionKinectData.NormalizedDepths;
	}

	public float[] GetSandBoxTopography () {
		return fullResolutionKinectData.NormalizedDepths;
	}

    // Make sure to kill the Kinect on quitting
    void OnApplicationQuit()
    {
        if (kinectInitialized)
        {
            // Shutdown OpenNI
            KinectWrapper.NuiShutdown();
            instance = null;
        }
    }
}
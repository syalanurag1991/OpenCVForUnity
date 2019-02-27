using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;

public class OpenCVInterface : MonoBehaviour {

	[DllImport("OpenCVForUnity", EntryPoint = "?Add@Functions@OpenCV@@SAMMM@Z")]
	public static extern float Add(float a, float b);

	[DllImport("OpenCVForUnity", EntryPoint = "?Multiply@Functions@OpenCV@@SAMMM@Z")]
	public static extern float Multiply(float a, float b);

	[DllImport("OpenCVForUnity", EntryPoint = "?Foopluginmethod@Functions@OpenCV@@SAMXZ")]
	public static extern float Foopluginmethod();

	[DllImport("OpenCVForUnity", EntryPoint = "?InitializeTracking@Functions@OpenCV@@SA_N_N@Z")]
	public static extern bool InitializeTracking(int minDistance, int maxDistance, bool activateVisualization);

	[DllImport("OpenCVForUnity", EntryPoint = "?GetRGBData@Functions@OpenCV@@SAPEAEXZ")]
	public static extern IntPtr GetRGBData();

	[DllImport("OpenCVForUnity", EntryPoint = "?GetDepthData@Functions@OpenCV@@SAPEAEXZ")]
	public static extern IntPtr GetDepthData();

	[DllImport("OpenCVForUnity", EntryPoint = "?GetBufferSize@Functions@OpenCV@@SAHXZ")]
	public static extern int GetBufferSize();

	[DllImport("OpenCVForUnity", EntryPoint = "?SetDepthCleaningParameters@Functions@OpenCV@@SAXHMME@Z")]
	public static extern void SetDepthCleaningParameters(int numberOfFrames, float currentFrameWeight, float resizeFactor, byte binarizingCutoff);

	[DllImport("OpenCVForUnity", EntryPoint = "?GetBlobsBasedDepthData@Functions@OpenCV@@SAPEAEXZ")]
	public static extern IntPtr GetBlobsBasedDepthData();

	[DllImport("OpenCVForUnity", EntryPoint = "?GetDepthDataForVisualization@Functions@OpenCV@@SAPEAEXZ")]
	public static extern IntPtr GetDepthDataForVisualization();

	[DllImport("OpenCVForUnity", EntryPoint = "?ConvertRGBDataToOpenCVFormat@Functions@OpenCV@@SAHPEAE@Z")]
	public static extern int ConvertRGBDataToOpenCVFormat(IntPtr rgbDataFromSensor);

	[DllImport("OpenCVForUnity", EntryPoint = "?ConvertDepthDataToOpenCVFormat@Functions@OpenCV@@SAHPEAE@Z")]
	public static extern int ConvertDepthDataToOpenCVFormat(IntPtr depthDataFromSensor);

	[DllImport("OpenCVForUnity", EntryPoint = "?ConvertVisualizationDepthDataToOpenCVFormat@Functions@OpenCV@@SAHPEAE@Z")]
	public static extern int ConvertVisualizationDepthDataToOpenCVFormat(IntPtr visualizationDepthDataFromSensor);

	[DllImport("OpenCVForUnity", EntryPoint = "?IsBlobTrackingThreadRunning@Functions@OpenCV@@SA_NXZ")]
	public static extern bool IsBlobTrackingThreadRunning();

	[DllImport("OpenCVForUnity", EntryPoint = "?TrackInFrame@Functions@OpenCV@@SA_NXZ")]
	public static extern bool TrackInFrame();

	[DllImport("OpenCVForUnity", EntryPoint = "?GetNumberOfBlobs@Functions@OpenCV@@SAHXZ")]
	public static extern int GetNumberOfBlobs();

	[DllImport("OpenCVForUnity", EntryPoint = "?GetBlobsData@Functions@OpenCV@@SAPEAMXZ")]
	public static extern IntPtr GetBlobsData();

	[DllImport("OpenCVForUnity", EntryPoint = "?DeleteBlobsData@Functions@OpenCV@@SAHPEAM@Z")]
	public static extern int DeleteBlobsData(IntPtr blobDataAddress);

	[DllImport("OpenCVForUnity", EntryPoint = "?Close@Functions@OpenCV@@SA_NXZ")]
	public static extern bool Close();

	public unsafe int ConvertDepthDataToOpenCVFormat(byte[] depthDataFromSensor) {
		int size = Marshal.SizeOf(depthDataFromSensor[0]) * depthDataFromSensor.Length;
		IntPtr pointerToDepthFrameData = Marshal.AllocHGlobal(size);												//Pin Memory
		try {																										// Copy the array to unmanaged memory
			Marshal.Copy(depthDataFromSensor, 0, pointerToDepthFrameData, depthDataFromSensor.Length);			
		} catch(Exception e) {
			status = e.Message;
			Marshal.FreeHGlobal(pointerToDepthFrameData);															// Free the unmanaged memory in case of error
		}
		int result = ConvertDepthDataToOpenCVFormat(pointerToDepthFrameData);
		Marshal.FreeHGlobal(pointerToDepthFrameData);
        if(startBlobTracking)
            StartTracking();
		UpdateLatencyMeasurements();
		return result;
	}

	public unsafe int ConvertVisualizationDepthDataToOpenCVFormat(byte[] visualizationDepthDataFromSensor) {
		int size = Marshal.SizeOf(visualizationDepthDataFromSensor[0]) * visualizationDepthDataFromSensor.Length;
		IntPtr pointerToVisualizationDepthFrameData = Marshal.AllocHGlobal(size);									//Pin Memory
		try {																										// Copy the array to unmanaged memory
			Marshal.Copy(visualizationDepthDataFromSensor, 0, pointerToVisualizationDepthFrameData, visualizationDepthDataFromSensor.Length);
		} catch(Exception e) {
			status = e.Message;
			Marshal.FreeHGlobal(pointerToVisualizationDepthFrameData);												// Free the unmanaged memory in case of error
		}
		int result = ConvertVisualizationDepthDataToOpenCVFormat(pointerToVisualizationDepthFrameData);
		Marshal.FreeHGlobal(pointerToVisualizationDepthFrameData);
		return result;
	}

	public unsafe int ConvertRGBDataToOpenCVFormat(byte[] rgbDataFromSensor) {
		int size = Marshal.SizeOf(rgbDataFromSensor[0]) * rgbDataFromSensor.Length;
		IntPtr pointerToRGBFrameData = Marshal.AllocHGlobal(size);													//Pin Memory
		try {																										// Copy the array to unmanaged memory
			Marshal.Copy(rgbDataFromSensor, 0, pointerToRGBFrameData, rgbDataFromSensor.Length);
		} catch(Exception e) {
			status = e.Message;
			Marshal.FreeHGlobal(pointerToRGBFrameData);																// Free the unmanaged memory in case of error
		}
		int result = ConvertRGBDataToOpenCVFormat(pointerToRGBFrameData);
		Marshal.FreeHGlobal(pointerToRGBFrameData);
		return result;
	}

	// Tracking data variables
    public bool startBlobTracking = false;
	[HideInInspector]
	public int numberOfBlobs;
	[HideInInspector]
	public bool IsTrackingInitialized = false;
	private float[] returnedBlobsData;
	private IntPtr returnedBlobsDataAddress;
	public List<Blob> collectionOfBlobs;

	[HideInInspector]
	public string IPAddressOfPeopleTrackingServer;
	[HideInInspector]
	public string status = "Hi";
	[HideInInspector]
	public string sendStatus = "";
	[HideInInspector]
	public int meanNumberOfBlobsDetected = 0;
	public Text viewStatus;
	private bool isTrackingRunningStatus = false;

	// Latency measurement variables
	private float previousTime = 0;
	private float currentTime = 0;
	private int responsesReceived = 0;
	[HideInInspector]
	public float measurementPeriod = 5f;
	[HideInInspector]
	public float currentLatencyInMilliSeconds = -1;
	[HideInInspector]
	public float fps = 0;

	// Feed dimensions
	public bool copyFeedsData = false;
	[HideInInspector]
	public int rgbFrameHeight;
	[HideInInspector]
	public int rgbFrameWidth;
	[HideInInspector]
	public long rgbDataSize;
	[HideInInspector]
	public int depthFrameHeight;
	[HideInInspector]
	public int depthFrameWidth;
	[HideInInspector]
	public long depthDataSize;

	// Store RGB and various depth feeds data in to array
	[HideInInspector]
	public byte[] returnedRGBData;
	[HideInInspector]
	public byte[] returnedDepthData;
	[HideInInspector]
	public byte[] returnedBlobsBasedDepthData;
	[HideInInspector]
	public byte[] returnedVisualizationDepthData;

	// Addresses returned by People-tracking DLL
	private IntPtr returnedRGBDataAddress;
	private IntPtr returnedDepthDataAddress;
	private IntPtr returnedBlobsBasedDepthDataAddress;
	private IntPtr returnedVisualizationDepthDataAddress;

	// returns the single KinectManager instance
	private static OpenCVInterface instance;
    public static OpenCVInterface Instance {
        get {
            return instance;
        }
    }

	void InitializeStorageForFeeds() {
		rgbFrameWidth = KinectWrapper.Constants.ColorImageWidth;
		rgbFrameHeight = KinectWrapper.Constants.ColorImageHeight;
		depthFrameWidth = KinectWrapper.Constants.DepthImageWidth;
		depthFrameHeight = KinectWrapper.Constants.DepthImageHeight;
		rgbDataSize = rgbFrameWidth * rgbFrameHeight * 4;
		depthDataSize = depthFrameWidth * depthFrameHeight;
		returnedRGBData = new byte[rgbDataSize];
		returnedDepthData = new byte[depthDataSize];
		returnedBlobsBasedDepthData = new byte[depthDataSize];
		returnedVisualizationDepthData = new byte[4*depthDataSize];
	}

	void CopyFeedsData() {
		// Show RGB Frame
		returnedRGBDataAddress = GetRGBData();
		Marshal.Copy(returnedRGBDataAddress, returnedRGBData, 0, (int)rgbDataSize);

		// Show Raw Depth Frame
		returnedDepthDataAddress = GetDepthData();
		Marshal.Copy(returnedDepthDataAddress, returnedDepthData, 0, (int)depthDataSize);

		// Show Blobs-based Depth Frame
		returnedBlobsBasedDepthDataAddress = GetBlobsBasedDepthData();
		Marshal.Copy(returnedBlobsBasedDepthDataAddress, returnedBlobsBasedDepthData, 0, (int)depthDataSize);

		// Show Visualization Depth Frame
		returnedVisualizationDepthDataAddress = GetDepthDataForVisualization();
		Marshal.Copy(returnedVisualizationDepthDataAddress, returnedVisualizationDepthData, 0, (int)depthDataSize*4);
	}

	void Start () {

		var x = InitializeTracking(0, 600, true);
		Debug.Log("OpenCV initialized: " + x.ToString());

		if(copyFeedsData)
			InitializeStorageForFeeds();

		instance = this;
	}

	void Update () {

		//Debug.Log("RGB Frame size  : " + rgbDataSize.ToString() + " bytes");
		//Debug.Log("Depth Frame size: " + depthDataSize.ToString() + " bytes");

		//Debug.Log("Buffer size:" + GetBufferSize());

		if (copyFeedsData)
			CopyFeedsData();

		if (TrackInFrame()) {
//			int numberOfBlobs = GetNumberOfBlobs();
//			if (numberOfBlobs > 0) {
//				returnedBlobsDataAddress = GetBlobsData();
//				returnedBlobsData = new double[numberOfBlobs*3];
//				Marshal.Copy(returnedBlobsDataAddress, returnedBlobsData, 0, numberOfBlobs*3);
//				DeleteBlobsData(returnedBlobsDataAddress);
//			}
		}
	}

	void StartTracking() {
		isTrackingRunningStatus = true;
		if(copyFeedsData)
			CopyFeedsData();

		Debug.Log("RGB Frame size  : " + rgbDataSize.ToString() + " bytes");
		Debug.Log("Depth Frame size: " + depthDataSize.ToString() + " bytes");

		numberOfBlobs = GetNumberOfBlobs();
		if(TrackInFrame()) {
			if(collectionOfBlobs != null)
				collectionOfBlobs.Clear();
			collectionOfBlobs = new List<Blob>();
			if(numberOfBlobs > 0) {
				int returnedBlobsDataSize = numberOfBlobs * 4;
				returnedBlobsDataAddress = GetBlobsData();
				returnedBlobsData = new float[returnedBlobsDataSize];
				Marshal.Copy(returnedBlobsDataAddress, returnedBlobsData, 0, returnedBlobsDataSize);
				for (int i = 0; i<numberOfBlobs; i++) {
					Blob tempBlob = new Blob (
						(int)returnedBlobsData[4*i],
						returnedBlobsData[4*i + 1],
						returnedBlobsData[4*i + 2],
						returnedBlobsData[4*i + 3]
					);
					collectionOfBlobs.Add(tempBlob);
				}
				DeleteBlobsData(returnedBlobsDataAddress);
			}
		}
		isTrackingRunningStatus = false;
	}

    public bool IsTrackingRunning() {
        return isTrackingRunningStatus; 
    }

	void UpdateLatencyMeasurements() {
		currentTime = Time.time;
		if(currentTime - previousTime < measurementPeriod) {
			responsesReceived++;
		} else {
			previousTime = currentTime;
			float tempLatency =(1000.0f*measurementPeriod)/(float)responsesReceived;
			currentLatencyInMilliSeconds = Mathf.Round(tempLatency * 100.0f) / 100.0f;
			if(currentLatencyInMilliSeconds > 0)
				fps = 1000f / currentLatencyInMilliSeconds;
			responsesReceived = 0;
		}
	}

	void OnApplicationQuit(){
		Close();
		instance = null;
	}
}

using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class OpenCVInterface2 : MonoBehaviour
{    
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// DLL Functions
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	[DllImport("opencv_android_vs")]
	private static extern float Foopluginmethod();

	[DllImport("opencv_android_vs")]
	private static extern double Add(double a, double b);

	[DllImport("opencv_android_vs")]
	public static extern bool InitializeTracking(bool activateVisualization);

	[DllImport("opencv_android_vs")]
	public static extern int ConvertDepthDataToOpenCVFormat(IntPtr depthDataFromSensor);

	[DllImport("opencv_android_vs")]
	public static extern int ConvertVisualizationDepthDataToOpenCVFormat(IntPtr visualizationDepthDataFromSensor);

	[DllImport("opencv_android_vs")]
	public static extern int ConvertRGBDataToOpenCVFormat(IntPtr rgbDataFromSensor);

	[DllImport("opencv_android_vs")]
	public static extern IntPtr GetRGBData();

	[DllImport("opencv_android_vs")]
	public static extern IntPtr GetDepthData();

	[DllImport("opencv_android_vs")]
	public static extern IntPtr GetRangeLimitedDepthData();

	[DllImport("opencv_android_vs")]
	public static extern IntPtr GetBlobsBasedDepthData();

	[DllImport("opencv_android_vs")]
	public static extern IntPtr GetDepthDataForVisualization();

	[DllImport("opencv_android_vs")]
	public static extern long GetRGBDataSize();

	[DllImport("opencv_android_vs")]
	public static extern int GetRGBFrameHeight();

	[DllImport("opencv_android_vs")]
	public static extern int GetRGBFrameWidth();

	[DllImport("opencv_android_vs")]
	public static extern int GetDepthDataSize();

	[DllImport("opencv_android_vs")]
	public static extern int GetDepthFrameHeight();

	[DllImport("opencv_android_vs")]
	public static extern int GetDepthFrameWidth();

	[DllImport("opencv_android_vs")]
	public static extern bool IsBlobTrackingThreadRunning();

	[DllImport("opencv_android_vs")]
	public static extern bool TrackInFrame();

	[DllImport("opencv_android_vs")]
	public static extern int GetNumberOfBlobs();

	[DllImport("opencv_android_vs")]
	public static extern IntPtr GetBlobsData();

	[DllImport("opencv_android_vs")]
	public static extern int DeleteBlobsData(IntPtr blobDataAddress);

	[DllImport("opencv_android_vs")]
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

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// Helper Functions
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Depth sensor object
	public bool copyFeedsData = false;
	public int minObervableDistance = 0;
	public int maxObervableDistance = 10000;

	// Feed dimensions
	[HideInInspector]
	public int rgbFrameWidth = 320;
	[HideInInspector]
	public int rgbFrameHeight = 240;
	[HideInInspector]
	public int depthFrameWidth = 160;
	[HideInInspector]
	public int depthFrameHeight = 120;
	[HideInInspector]
	public long rgbDataSize = 320 * 240;
	[HideInInspector]
	public long depthDataSize = 160 * 120;

	// Addresses returned by People-tracking DLL
	private IntPtr returnedRGBDataAddress;
	private IntPtr returnedDepthDataAddress;
	private IntPtr returnedRangeLimitedDepthDataAddress;
	private IntPtr returnedBlobsBasedDepthDataAddress;
	private IntPtr returnedVisualizationDepthDataAddress;

	// Store RGB and various depth feeds data in to array
	[HideInInspector]
	public byte[] returnedRGBData;
	[HideInInspector]
	public byte[] returnedDepthData;
	[HideInInspector]
	public byte[] returnedRangeLimitedDepthData;
	[HideInInspector]
	public byte[] returnedBlobsBasedDepthData;
	[HideInInspector]
	public byte[] returnedVisualizationDepthData;

	// Tracking data variables
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

	void OnGUI()
	{
		string result =
			"People Tracking Server: " + IPAddressOfPeopleTrackingServer
			+ "\n" + "FPS: " + fps.ToString ()
			+ "\n" + "Average number of blobs: " + meanNumberOfBlobsDetected.ToString()
			+ "\n" + "Current number of blobs: " + numberOfBlobs.ToString ()
			+ "\n" + "Blob positions:";

		foreach (var tempBlob in collectionOfBlobs) {
			string blobsData =
				"\nIndex = " + tempBlob.index.ToString()
				+ " | X = " + tempBlob.x.ToString()
				+ " | Y = " + tempBlob.y.ToString()
				+ " | Depth = " + tempBlob.depth.ToString();
			result += blobsData;
		}

		//GUI.Label(new Rect(200, 800, 450, 400), result);
		viewStatus.text = result;
	}

	void InitializeStorageForFeeds() {
		returnedRGBData = new byte[rgbDataSize];
		returnedDepthData = new byte[depthDataSize];
		returnedRangeLimitedDepthData = new byte[depthDataSize];
		returnedBlobsBasedDepthData = new byte[depthDataSize];
		returnedVisualizationDepthData = new byte[4*depthDataSize];
	}

	void CopyFeedsData() {
		returnedRGBDataAddress = GetRGBData();																			// Show RGB Frame
		Marshal.Copy(returnedRGBDataAddress, returnedRGBData, 0,(int)rgbDataSize);

		returnedDepthDataAddress = GetDepthData();																		// Show Depth Frame
		Marshal.Copy(returnedDepthDataAddress, returnedDepthData, 0,(int)depthDataSize);

		returnedBlobsBasedDepthDataAddress = GetBlobsBasedDepthData();													// Show Blobs-based Depth Frame
		Marshal.Copy(returnedBlobsBasedDepthDataAddress, returnedBlobsBasedDepthData, 0,(int)depthDataSize);

		returnedVisualizationDepthDataAddress = GetDepthDataForVisualization();											// Show Visualization Depth Frame
		Marshal.Copy(returnedVisualizationDepthDataAddress, returnedVisualizationDepthData, 0,(int)(4*depthDataSize));
	}

	void Start() {
//		rgbFrameWidth = astraContollerScript.rgbFrameWidth;
//		rgbFrameHeight = astraContollerScript.rgbFrameHeight;
//		depthFrameWidth = astraContollerScript.depthFrameWidth;
//		depthFrameHeight = astraContollerScript.depthFrameHeight;
		rgbDataSize = rgbFrameWidth * rgbFrameHeight * 3;
		depthDataSize = depthFrameWidth * depthFrameHeight;
		if(copyFeedsData)
			InitializeStorageForFeeds();
		IsTrackingInitialized = InitializeTracking(copyFeedsData);
	}

	public bool IsTrackingRunning() {
		return isTrackingRunningStatus;
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
	}
}

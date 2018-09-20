using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;

public class PeopleTracking : MonoBehaviour {

	[DllImport("PeopleTracking", EntryPoint = "?InitializeTracking@Functions@PeopleTracking@@SA_NHH_N@Z")]
	public static extern bool InitializeTracking(int minDistance, int maxDistance, bool activateVisualization);

	[DllImport("PeopleTracking", EntryPoint = "?GetRGBData@Functions@PeopleTracking@@SAPEAEXZ")]
	public static extern IntPtr GetRGBData();

	[DllImport("PeopleTracking", EntryPoint = "?GetRawDepthData@Functions@PeopleTracking@@SAPEAEXZ")]
	public static extern IntPtr GetRawDepthData();

	[DllImport("PeopleTracking", EntryPoint = "?GetRangeLimitedDepthData@Functions@PeopleTracking@@SAPEAEXZ")]
	public static extern IntPtr GetRangeLimitedDepthData();

	[DllImport("PeopleTracking", EntryPoint = "?GetBlobsBasedDepthData@Functions@PeopleTracking@@SAPEAEXZ")]
	public static extern IntPtr GetBlobsBasedDepthData();

	[DllImport("PeopleTracking", EntryPoint = "?GetDepthDataForVisualization@Functions@PeopleTracking@@SAPEAEXZ")]
	public static extern IntPtr GetDepthDataForVisualization();

	[DllImport("PeopleTracking", EntryPoint = "?GetRGBDataSize@Functions@PeopleTracking@@SAJXZ")]
	public static extern long GetRGBDataSize();

	[DllImport("PeopleTracking", EntryPoint = "?GetRGBFrameHeight@Functions@PeopleTracking@@SAHXZ")]
	public static extern int GetRGBFrameHeight();

	[DllImport("PeopleTracking", EntryPoint = "?GetRGBFrameWidth@Functions@PeopleTracking@@SAHXZ")]
	public static extern int GetRGBFrameWidth();

	[DllImport("PeopleTracking", EntryPoint = "?GetDepthDataSize@Functions@PeopleTracking@@SAJXZ")]
	public static extern int GetDepthDataSize();

	[DllImport("PeopleTracking", EntryPoint = "?GetDepthFrameHeight@Functions@PeopleTracking@@SAHXZ")]
	public static extern int GetDepthFrameHeight();

	[DllImport("PeopleTracking", EntryPoint = "?GetDepthFrameWidth@Functions@PeopleTracking@@SAHXZ")]
	public static extern int GetDepthFrameWidth();

	[DllImport("PeopleTracking", EntryPoint = "?IsBlobTrackingThreadRunning@Functions@PeopleTracking@@SA_NXZ")]
	public static extern bool IsBlobTrackingThreadRunning();

	[DllImport("PeopleTracking", EntryPoint = "?TrackInFrame@Functions@PeopleTracking@@SA_NXZ")]
	public static extern bool TrackInFrame();

	[DllImport("PeopleTracking", EntryPoint = "?GetNumberOfBlobs@Functions@PeopleTracking@@SAHXZ")]
	public static extern int GetNumberOfBlobs();

	[DllImport("PeopleTracking", EntryPoint = "?GetBlobsData@Functions@PeopleTracking@@SAPEANXZ")]
	public static extern IntPtr GetBlobsData();

	[DllImport("PeopleTracking", EntryPoint = "?DeleteBlobsData@Functions@PeopleTracking@@SAHPEAN@Z")]
	public static extern int DeleteBlobsData(IntPtr blobDataAddress);

	[DllImport("PeopleTracking", EntryPoint = "?Close@Functions@PeopleTracking@@SA_NXZ")]
	public static extern bool Close();

	// Tracking data variables
	[HideInInspector]
	public int numberOfBlobs;
	public struct Blob {
		public double[] normalizedBlobCoordinates;
		public byte depthValue;
	};
	public List<Blob> collectionOfBlobs;
	private double[] returnedBlobsData;
	private IntPtr returnedBlobsDataAddress;

	// Feed dimensions
	public bool copyFeedsData = false;
	[HideInInspector]
	public int rgbFrameHeight = 0;
	[HideInInspector]
	public int rgbFrameWidth = 0;
	[HideInInspector]
	public long rgbDataSize;
	[HideInInspector]
	public int depthFrameHeight = 0;
	[HideInInspector]
	public int depthFrameWidth = 0;
	[HideInInspector]
	public long depthDataSize;

	// Store RGB and various depth feeds data in to array
	[HideInInspector]
	public byte[] returnedRGBData;
	[HideInInspector]
	public byte[] returnedRawDepthData;
	[HideInInspector]
	public byte[] returnedRangeLimitedDepthData;
	[HideInInspector]
	public byte[] returnedBlobsBasedDepthData;
	[HideInInspector]
	public byte[] returnedVisualizationDepthData;

	// Addresses returned by People-tracking DLL
	private IntPtr returnedRGBDataAddress;
	private IntPtr returnedRawDepthDataAddress;
	private IntPtr returnedRangeLimitedDepthDataAddress;
	private IntPtr returnedBlobsBasedDepthDataAddress;
	private IntPtr returnedVisualizationDepthDataAddress;

	void GetDimensionsAndDataSize(){
		rgbDataSize = GetRGBDataSize();
		rgbFrameHeight = GetRGBFrameHeight();
		rgbFrameWidth = GetRGBFrameWidth();
		depthDataSize = GetDepthDataSize();
		depthFrameHeight = GetDepthFrameHeight();
		depthFrameWidth = GetDepthFrameWidth();
	}

	void InitializeStorageForFeeds() {
		returnedRGBData = new byte[rgbDataSize];
		returnedRawDepthData = new byte[depthDataSize];
		returnedRangeLimitedDepthData = new byte[depthDataSize];
		returnedBlobsBasedDepthData = new byte[depthDataSize];
		returnedVisualizationDepthData = new byte[4*depthDataSize];
	}

	void CopyFeedsData() {
		// Show RGB Frame
		returnedRGBDataAddress = GetRGBData();
		Marshal.Copy(returnedRGBDataAddress, returnedRGBData, 0, (int)rgbDataSize);

		// Show Raw Depth Frame
		returnedRawDepthDataAddress = GetRawDepthData();
		Marshal.Copy(returnedRawDepthDataAddress, returnedRawDepthData, 0, (int)depthDataSize);

		// Show Range-limited Depth Frame
		returnedRangeLimitedDepthDataAddress = GetRangeLimitedDepthData();
		Marshal.Copy(returnedRangeLimitedDepthDataAddress, returnedRangeLimitedDepthData, 0, (int)depthDataSize);

		// Show Blobs-based Depth Frame
		returnedBlobsBasedDepthDataAddress = GetBlobsBasedDepthData();
		Marshal.Copy(returnedBlobsBasedDepthDataAddress, returnedBlobsBasedDepthData, 0, (int)depthDataSize);

		// Show Visualization Depth Frame
		returnedVisualizationDepthDataAddress = GetDepthDataForVisualization();
		Marshal.Copy(returnedVisualizationDepthDataAddress, returnedVisualizationDepthData, 0, (int)depthDataSize*4);
	}

	void Start () {
		var x = InitializeTracking(0, 600, true);
		Debug.Log("Initializing successful: " + x.ToString());

		GetDimensionsAndDataSize();
		if(copyFeedsData)
			InitializeStorageForFeeds();
	}

	void Update () {

		Debug.Log("RGB Frame size  : " + rgbDataSize.ToString() + " bytes");
		Debug.Log("Depth Frame size: " + depthDataSize.ToString() + " bytes");

		if (copyFeedsData)
			CopyFeedsData();

		if (TrackInFrame()) {
			int numberOfBlobs = GetNumberOfBlobs();
			if (numberOfBlobs > 0) {
				returnedBlobsDataAddress = GetBlobsData();
				returnedBlobsData = new double[numberOfBlobs*3];
				Marshal.Copy(returnedBlobsDataAddress, returnedBlobsData, 0, numberOfBlobs*3);
				DeleteBlobsData(returnedBlobsDataAddress);
			}
		}
	}

	void OnApplicationQuit(){
		Close();
	}
}
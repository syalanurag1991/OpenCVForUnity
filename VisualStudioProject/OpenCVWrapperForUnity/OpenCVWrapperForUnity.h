//BlobTracking.h - Contains declaration of Functions Class
#pragma once

#ifdef BLOBTRACKING_EXPORTS
#define BLOBTRACKING_API __declspec(dllexport)
#else
#define BLOBTRACKING_API __declspec(dllexport)
#endif

#include <Kinect.h>
#include <opencv2/opencv.hpp>
#include <opencv2/bgsegm.hpp>

using namespace std;
using namespace cv;
using namespace bgsegm;

#define absoluteMaxRange 8000
#define rgbFrameWidth 1920
#define rgbFrameHeight 1080
#define depthFrameWidth 512
#define depthFrameHeight 424
#define rgbFrameHalfWidth 960
#define rgbFrameHalfHeight 540
#define depthFrameHalfWidth 256
#define depthFrameHalfHeight 212

typedef struct Blob {
	double normalizedBlobCoordinates[2];
	byte depthValue;
} MyBlob;

namespace PeopleTracking {

	//This class is exported from MathLibrary.dll
	class Functions {
	public:
		static BLOBTRACKING_API double Add(double a, double b);
		static BLOBTRACKING_API double Multiply(double a, double b);
		static BLOBTRACKING_API char* getByteArray();
		static BLOBTRACKING_API int freeMem(char* arrayPtr);
		
		// Function initializes depth sensor as well as OpenCV modules.
		// Initiates receiving of RGB and Depth streams from sensor as well.
		// Returns 'TRUE'/'FALSE'. Accepts min and max observable distances.
		// Function activates processing for visualizing tracking feed.
		// Visualization slows down tracking as new feed is generated.
		// Hence, default value for activateVisualization is FALSE.
		static BLOBTRACKING_API bool InitializeTracking(int minDistance, int maxDistance, bool activateVisualization);

		// Function will return a byte-array containing data from latest RGB frame
		static BLOBTRACKING_API byte* GetRGBData();

		// Function will return a byte-array containing raw depth data from latest Depth frame
		static BLOBTRACKING_API byte* GetRawDepthData();

		// Function will return a byte-array containing range-limited depth data from latest Depth frame
		static BLOBTRACKING_API byte* GetRangeLimitedDepthData();

		// Function will return a byte-array containing depth data for only blob regions
		static BLOBTRACKING_API byte* GetBlobsBasedDepthData();

		// Function will return a byte-array containing depth data for visualization
		static BLOBTRACKING_API byte* GetDepthDataForVisualization();

		// Function will return total size of RGB frame
		static BLOBTRACKING_API long GetRGBDataSize();

		// Function will return height of RGB frame
		static BLOBTRACKING_API int GetRGBFrameHeight();

		// Function will return width of RGB frame
		static BLOBTRACKING_API int GetRGBFrameWidth();

		// Function will return total size of Depth frame
		static BLOBTRACKING_API long GetDepthDataSize();

		// Function will return height of Depth frame
		static BLOBTRACKING_API int GetDepthFrameHeight();

		// Function will return width of Depth frame
		static BLOBTRACKING_API int GetDepthFrameWidth();

		// Function returns status of blob-tracking thread
		// Recommended to check status before calling 'TrackInFrame' function
		static BLOBTRACKING_API bool IsBlobTrackingThreadRunning();

		// Function will track blobs for only current frame
		static BLOBTRACKING_API bool TrackInFrame();

		// Function returns number of blobs detected
		static BLOBTRACKING_API int GetNumberOfBlobs();
		
		// Get number of blobs detected before using this
		// Each blob has (x, y) coordinates pair and depth value of its center
		// [0] -> x-Coordinate, [1] -> y-Coordinate, [2] -> depth value at center
		//static BLOBTRACKING_API double** GetBlobsData();
		static BLOBTRACKING_API double* GetBlobsData();

		// Use this function to delete blobs data and frees memory
		static BLOBTRACKING_API int DeleteBlobsData(double* blobsData);

		// Function closes sensor and frees memory
		static BLOBTRACKING_API bool Close();

	private:
		// Use this to initialize Kinect.
		// Initiates receiving RGB and Depth streams.
		// Returns 'TRUE'/'FALSE'.
		static BLOBTRACKING_API bool InitializeKinect(int minDistance, int maxDistance);

		// Use this to fetch data from Kinect.
		// Depth is considered for further processing only if:
		// -> it is GREATER than min limit, else it is 0
		// -> it is LESSER THAN OR EQUAL max limit, else it is 0
		// -> in a valid range it ranges in 1-255
		static BLOBTRACKING_API void GetKinectData();

		// Use this to fetch frame from Kinect and Convert data from Kinect to OpenCV format asynchronously.
		// Returns TRUE/FALSE. Function waits for a maximum number of attempts before returning a FALSE.
		static BLOBTRACKING_API bool ContinuousFetchFrameAsync();

		// Initializes background subtraction object using MOG2 method (an OpenCV function)
		static BLOBTRACKING_API void InitializeForegroundMasking();

		// Updates data input for background-subtraction.
		// Waits for certain number of frames to pass before updating.
		// A '0'-wait will mean that data input is updated every frame.
		// First few frames in background-subtraction are used for determining the background.
		// In general, you don't want to update input for background-subtraction every frame.
		// Ideally you would want background to be updated only a few times before it is fixed. 
		static BLOBTRACKING_API void UpdateForegroundMask();

		// Performs background-subtraction and produces a masked-foreground
		static BLOBTRACKING_API void ApplyForegroundMasking();

		// Function performs denoising of background-subtracted depth frame
		// 'Hole-filling' is performed before return to fill gaps in blobs
		static BLOBTRACKING_API void CleanForegroundMask();

		// Use this to Convert data from Kinect to OpenCV format.
		static BLOBTRACKING_API bool ConvertKinectDataToOpenCVFormat();

		// Use this function to initialize blob-tracker
		// Parameters are fixed
		static BLOBTRACKING_API void InitializeBlobTracker();

		// Use function to create a circular visualizer for blob-tracking points
		static BLOBTRACKING_API void MyFilledCircle(Mat* img, Point center);
	};
}
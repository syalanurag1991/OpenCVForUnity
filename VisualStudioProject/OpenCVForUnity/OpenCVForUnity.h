#pragma once

#ifdef OPENCVFORUNITY_EXPORTS
#define OPENCVFORUNITY_API __declspec(dllexport)
#else
#define OPENCVFORUNITY_API __declspec(dllimport)
#endif

#include <cstdlib>
#include <list>
#include <iostream>
#include <cstring>
#include <deque>
#include <opencv2/core.hpp>
#include <opencv2/photo.hpp>
#include <opencv2/imgcodecs.hpp>
#include <opencv2/imgproc.hpp>
#include <opencv2/videoio.hpp>
#include <opencv2/highgui.hpp>
#include <opencv2/video.hpp>
#include <opencv2/core/ocl.hpp>
#include <opencv2/features2d/features2d.hpp>

using namespace cv;
using namespace std;

#define absoluteMaxRange 10000
#define rgbFrameWidth 640
#define rgbFrameHeight 480
#define depthFrameWidth 320
#define depthFrameHeight 240
#define rgbFrameHalfWidth 320
#define rgbFrameHalfHeight 240
#define depthFrameHalfWidth 160
#define depthFrameHalfHeight 120

// (xNormalized, yNormalized) are normalized values between -1 to +1
typedef struct Blob {
	int index;
	float xActual;
	float yActual;
	float xNormalized;
	float yNormalized;
	float depthValue;
} MyBlob;

namespace OpenCV {
	class Functions {
	public:
		static OPENCVFORUNITY_API float Foopluginmethod();
		static OPENCVFORUNITY_API float Add(float a, float b);
		static OPENCVFORUNITY_API float Multiply(float a, float b);
		static OPENCVFORUNITY_API char* getByteArray();
		static OPENCVFORUNITY_API int freeMem(char* arrayPtr);

		// Function initializes depth sensor as well as OpenCV modules.
		// Initiates receiving of RGB and Depth streams from sensor as well.
		// Returns 'TRUE'/'FALSE'. Accepts min and max observable distances.
		// Function activates processing for visualizing tracking feed.
		// Visualization slows down tracking as new feed is generated.
		// Hence, default value for activateVisualization is FALSE.
		static OPENCVFORUNITY_API  bool InitializeTracking(bool activateVisualization);

		// Function will return a byte-array containing data from latest RGB frame
		static OPENCVFORUNITY_API uchar* GetRGBData();

		// Function will return a byte-array containing raw depth data from latest Depth frame
		static OPENCVFORUNITY_API uchar* GetDepthData();

		// Function will return a byte-array containing depth data for only blob regions
		static OPENCVFORUNITY_API uchar* GetBlobsBasedDepthData();

		// Function will return a byte-array containing depth data for visualization
		static OPENCVFORUNITY_API uchar* GetDepthDataForVisualization();

		// Function returns status of blob-tracking thread
		// Recommended to check status before calling 'TrackInFrame' function
		static OPENCVFORUNITY_API bool IsBlobTrackingThreadRunning();

		// Function will track blobs for only current frame
		static OPENCVFORUNITY_API bool TrackInFrame();

		// Function returns number of blobs detected
		static OPENCVFORUNITY_API int GetNumberOfBlobs();

		// Get number of blobs detected before using this
		// Each blob has (x, y) coordinates pair and depth value of its center
		// [0] -> x-Coordinate, [1] -> y-Coordinate, [2] -> depth value at center
		static OPENCVFORUNITY_API float* GetBlobsData();

		// Use this function to delete blobs data and frees memory
		static OPENCVFORUNITY_API int DeleteBlobsData(float* blobsData);

		// Function closes sensor and frees memory
		static OPENCVFORUNITY_API bool Close();

		// Initializes background subtraction object using MOG2 method (an OpenCV function)
		static OPENCVFORUNITY_API void InitializeForegroundMasking();

		// Updates data input for background-subtraction.
		// Waits for certain number of frames to pass before updating.
		// A '0'-wait will mean that data input is updated every frame.
		// First few frames in background-subtraction are used for determining the background.
		// In general, you don't want to update input for background-subtraction every frame.
		// Ideally you would want background to be updated only a few times before it is fixed. 
		static OPENCVFORUNITY_API void UpdateForegroundMask();

		// Performs background-subtraction and produces a masked-foreground
		static OPENCVFORUNITY_API void ApplyForegroundMasking();

		// Function performs denoising of background-subtracted depth frame
		// 'Hole-filling' is performed before return to fill gaps in blobs
		static OPENCVFORUNITY_API void CleanForegroundMask();

		// Function performs denoising of depth data 
		// 'Hole-filling' is performed before return to fill gaps in blobs
		static OPENCVFORUNITY_API void CleanDepthData();

		// Utility function for cleaninng noisy depth data, keeps history of past frames
		// Lot of frame manipulations are invloved, lower resize factor speeds up process
		// Current frame weight determines contribution of current frame to the cleaned frame
		// Method includes binary image thresholding, 0 <= cutoff <=255
		static OPENCVFORUNITY_API int GetBufferSize();
		static OPENCVFORUNITY_API void SetDepthCleaningParameters(int numberOfFrames, float currentFrameWeight, float resizeFactor, uchar binarizingCutoff);

		// Use this to Convert RGB data from Depth Sensor to OpenCV format
		static OPENCVFORUNITY_API int ConvertRGBDataToOpenCVFormat(uchar * rgbDataFromSensor);

		// Use this to Convert depth data from Depth Sensor to OpenCV format
		static OPENCVFORUNITY_API int ConvertDepthDataToOpenCVFormat(uchar * depthDataFromSensor);

		// Use this to Convert Depth data for visualization from Depth Sensor to OpenCV format
		static OPENCVFORUNITY_API int ConvertVisualizationDepthDataToOpenCVFormat(uchar * visualizationDepthDataFromSensor);

		// Use this function to initialize blob-tracker
		// Parameters are fixed
		static OPENCVFORUNITY_API void InitializeBlobTracker();

		// Use function to create a circular visualizer for blob-tracking points
		static OPENCVFORUNITY_API void MyFilledCircle(Mat* img, Point2f center);

		// Function to calculate distance between two blobs
		static OPENCVFORUNITY_API float GetDistance(float x1, float y1, float x2, float y2);

		// Function to create new blob. specifically for CollectionOfBlobs list
		static OPENCVFORUNITY_API MyBlob CreateNewBlobForCollection(int index, float blobCoordinateX, float blobCoordinateY);

		// Function to update a blob in CollectionOfBlobs list
		static OPENCVFORUNITY_API void UpdateExistingBlobInCollection(MyBlob * addressOfBlob, float blobCoordinateX, float blobCoordinateY);

		// Functions to normalize (x, y) coordinates and depth
		static OPENCVFORUNITY_API float NormalizeX(float blobCoordinateX);
		static OPENCVFORUNITY_API float NormalizeY(float blobCoordinateY);
		static OPENCVFORUNITY_API float NormalizeDepth(int blobCoordinateX, int blobCoordinateY);

		// Perform persistent blob-tracking
		static OPENCVFORUNITY_API void TrackBlobsPersistently();
	};
}



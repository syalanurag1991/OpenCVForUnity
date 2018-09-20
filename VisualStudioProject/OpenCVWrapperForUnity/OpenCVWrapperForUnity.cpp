#include "stdafx.h"
#include "OpenCVWrapperForUnity.h"

#include <Windows.h>
#include <Ole2.h>
#include <iostream>
#include <thread>

#include <Kinect.h>
#include <opencv2/opencv.hpp>
#include <opencv2/bgsegm.hpp>

using namespace std;
using namespace cv;
using namespace bgsegm;

// C++ variables
byte rgbData[rgbFrameWidth * rgbFrameHeight * 4];						// BGRA array containing the texture data
byte rawDepthData[depthFrameWidth * depthFrameHeight];						// Array containing depth texture data
byte rangeLimitedDepthData[depthFrameWidth * depthFrameHeight];			// Array containing range limited depth texture data
byte blobsBasedDepthData[depthFrameWidth * depthFrameHeight];			// Array containing binary depth texture data (0 or 255)
byte depthDataForVisualization[depthFrameWidth * depthFrameHeight * 4];	// Array containing depth texture data for visualization

// Kinect variables
IKinectSensor* sensor;					// Kinect sensor
IColorFrameReader* rgbStreamReader;		// Kinect color data source
IDepthFrameReader* depthStreamReader;	// Depth data source
int minObservableDistance = 0;
int maxObservableDistance = 600;
int rangeObservableDistance = maxObservableDistance - minObservableDistance;

// OpenCV variables
Mat ocvRGBFrameData;					// RGB data from Kinect (BGRA, 32 bit)
Mat ocvDepthFrameData;					// Depth data from Kinect (GRAY, 8 bit)
Mat ocvDepthFrameFGMaskCleaned_MOG2;	// Noise-free data after background-subtraction (MOG2)
Mat ocvDepthFrameDataForVisualization;	// Process visualization feed

// Denoising and Blob-formation Algorithm variables
bool dataAvailable = false;
bool showVisualization = false;
bool foregroundMaskIsAvailable = false;

int totalFramesPassed = 0;
int frameFetchAttempt = 0;
int maxNumberOfFrameFetchAttempts = 10;
int waitForFramesToPass = 1000;
int framesPassedAfterLastUpdate = 0;
int stopUpdatingBackgroundAfterFrames = 50;

// Blob-tracking variables
bool processingNow = false;
Ptr<SimpleBlobDetector> blobDetector;
vector<KeyPoint> keypoints;
list<MyBlob> collectionOfBlobs;

Ptr< BackgroundSubtractor> pointerToSubtractorObject_MOG2;
Mat foregroundMask_MOG2;
Mat updatedForegroundMask;

Mat morphingKernel_size1 = getStructuringElement(MORPH_RECT, Size(1, 1));
Mat morphingKernel_size3 = getStructuringElement(MORPH_RECT, Size(3, 3));
Mat morphingKernel_size5 = getStructuringElement(MORPH_RECT, Size(5, 5));
Mat morphingKernel_size7 = getStructuringElement(MORPH_RECT, Size(7, 7));

namespace PeopleTracking {
	
	double Functions::Add(double a, double b) {
		return a + b;
	}
	double Functions::Multiply(double a, double b) {
		return a * b;
	}

	char* Functions::getByteArray() {
		//Create your array(Allocate memory)
		char * arrayTest = new char[2];

		//Do something to the Array
		arrayTest[0] = 3;
		arrayTest[1] = 5;

		//Return it
		return arrayTest;
	}

	int Functions::freeMem(char* arrayPtr) {
		delete[] arrayPtr;
		return 0;
	}

	// Use this to initialize Kinect.
	// Initiates receiving RGB and Depth streams.
	// Returns 'TRUE'/'FALSE'.
	bool Functions::InitializeKinect(int minDistance, int maxDistance) {
		
		if ((maxDistance > minDistance) && (maxDistance > 500) && (minDistance >= 0)) {
			minObservableDistance = minDistance;
			maxObservableDistance = maxDistance;
		}

		if (FAILED(GetDefaultKinectSensor(&sensor))) {
			return false;
		}

		if (sensor) {
			sensor->Open();
			IColorFrameSource* rgbFrameSource = NULL;
			IDepthFrameSource* depthFrameSource = NULL;
			sensor->get_ColorFrameSource(&rgbFrameSource);
			sensor->get_DepthFrameSource(&depthFrameSource);
			rgbFrameSource->OpenReader(&rgbStreamReader);
			depthFrameSource->OpenReader(&depthStreamReader);
			if (rgbFrameSource) {
				rgbFrameSource->Release();
				rgbFrameSource = NULL;
			}
			if (depthFrameSource) {
				depthFrameSource->Release();
				depthFrameSource = NULL;
			}
			return true;
		}
		else {
			return false;
		}
	}

	// Use this to fetch data from Kinect.
	// Depth is considered for further processing only if:
	// -> it is GREATER than min limit, else it is 0
	// -> it is LESSER THAN OR EQUAL max limit, else it is 0
	// -> in a valid range it ranges in 1-255
	void Functions::GetKinectData() {
		byte* rawDepthDataCurrentAddress = rawDepthData;
		byte* rangeLimitedDepthDataCurrentAddress = rangeLimitedDepthData;
		byte* depthDataForVisualizationCurrentAddress = depthDataForVisualization;

		IColorFrame* rgbFrame = NULL;
		IDepthFrame* depthFrame = NULL;

		if (SUCCEEDED(rgbStreamReader->AcquireLatestFrame(&rgbFrame))) {
			rgbFrame->CopyConvertedFrameDataToArray(rgbFrameWidth*rgbFrameHeight * 4, rgbData, ColorImageFormat_Bgra);
		}
		if (rgbFrame) rgbFrame->Release();

		if (SUCCEEDED(depthStreamReader->AcquireLatestFrame(&depthFrame))) {
			unsigned int size;
			unsigned short* depthBuffer;
			depthFrame->AccessUnderlyingBuffer(&size, &depthBuffer);
			const unsigned short* currentAddress = (const unsigned short*)depthBuffer;
			const unsigned short* lastAddress = currentAddress + (depthFrameWidth*depthFrameHeight);
			while (currentAddress < lastAddress) {
				// Get depth in millimeters
				unsigned short depth = (*currentAddress++);
				
				// Draw a grayscale raw depth image normalized between 0-255 
				float rawDepthValue = (((float)depth)*255.0) / absoluteMaxRange;
				if (rawDepthValue > 255)
					rawDepthValue = 255;
				if (rawDepthValue < 0)
					rawDepthValue = 0;
				*rawDepthDataCurrentAddress++ = (byte) rawDepthValue;
				
				// Draw a grayscale depth image normalized between 0-255
				// For visualization: Draw a green-grayscale depth image normalized between 0-255
				// If depth > max limit -> blue, for depth < min limit -> red
				if (depth > maxObservableDistance) {
					*rangeLimitedDepthDataCurrentAddress++ = 0;
					if (showVisualization) {
						*depthDataForVisualizationCurrentAddress++ = 255;	// Show blue
						*depthDataForVisualizationCurrentAddress++ = 0;		// Don't show green
						*depthDataForVisualizationCurrentAddress++ = 0;		// Don't show red
						*depthDataForVisualizationCurrentAddress++ = 255;	// Alpha channel
					}
				} else if (depth <= minObservableDistance) {
					*rangeLimitedDepthDataCurrentAddress++ = 0;
					if (showVisualization) {
						*depthDataForVisualizationCurrentAddress++ = 0;		// Don't show blue
						*depthDataForVisualizationCurrentAddress++ = 0;		// Don't show green
						*depthDataForVisualizationCurrentAddress++ = 255;	// Show red
						*depthDataForVisualizationCurrentAddress++ = 255;	// Alpha channel
					}
				} else {
					float normalizedDepthPixelValue = (((float)(depth - minObservableDistance)*255.0) / (float)rangeObservableDistance);
					if (normalizedDepthPixelValue > 255.0)
						normalizedDepthPixelValue = 255.0;
					if (normalizedDepthPixelValue < 0)
						normalizedDepthPixelValue = 0;
					*rangeLimitedDepthDataCurrentAddress++ = (byte)((int)normalizedDepthPixelValue);
					if (showVisualization) {
						*depthDataForVisualizationCurrentAddress++ = 0;											// Don't show blue
						*depthDataForVisualizationCurrentAddress++ = (byte)((int)normalizedDepthPixelValue);	// Show green
						*depthDataForVisualizationCurrentAddress++ = 0;											// Don't show red
						*depthDataForVisualizationCurrentAddress++ = 255;	// Alpha channel
					}
				}
			}
		}
		if (depthFrame) depthFrame->Release();

		return;
	}

	// Use this to Convert data from Kinect to OpenCV format.
	bool Functions::ConvertKinectDataToOpenCVFormat() {
		int frameWidth = 0;
		int frameHeight = 0;
		void* frameData;
		GetKinectData();
		ocvRGBFrameData = Mat(rgbFrameHeight, rgbFrameWidth, CV_8UC4, rgbData);
		ocvDepthFrameData = Mat(depthFrameHeight, depthFrameWidth, CV_8UC1, rangeLimitedDepthData);
		if (showVisualization)
			ocvDepthFrameDataForVisualization = Mat(depthFrameHeight, depthFrameWidth, CV_8UC4, depthDataForVisualization);
		totalFramesPassed++;
		if (ocvRGBFrameData.rows > 0 && ocvDepthFrameData.rows) return true;
		else return false;
	}

	// Use this to fetch frame from Kinect and Convert data from Kinect to OpenCV format asynchronously.
	// Returns TRUE/FALSE. Function waits for a maximum number of attempts before returning a FALSE.
	bool Functions::ContinuousFetchFrameAsync() {
		if (frameFetchAttempt < maxNumberOfFrameFetchAttempts) {
			bool dataAvailable = ConvertKinectDataToOpenCVFormat();
			//Functions blobTrackingFunctions;
			//bool dataAvailable = blobTrackingFunctions.ConvertKinectDataToOpenCVFormat();
			if (dataAvailable) {
				frameFetchAttempt = 0;
			}
			else {
				frameFetchAttempt++;
			}
			return true;
		}
		else {
			frameFetchAttempt = 0;
			destroyAllWindows();
			return false;
		}
	}

	// Initializes background subtraction object using MOG2 method (an OpenCV function)
	void Functions::InitializeForegroundMasking() {
		pointerToSubtractorObject_MOG2 = createBackgroundSubtractorMOG2(10, 16, false);
	}

	// Updates data input for background-subtraction.
	// Waits for certain number of frames to pass before updating.
	// A '0'-wait will mean that data input is updated every frame.
	// First few frames in background-subtraction are used for determining the background.
	// In general, you don't want to update input for background-subtraction every frame.
	// Ideally you would want background to be updated only a few times before it is fixed. 
	void Functions::UpdateForegroundMask() {
		// Initialize foreground mask
		if (updatedForegroundMask.rows == 0) {
			if (ocvDepthFrameData.rows != 0) {
				updatedForegroundMask = ocvDepthFrameData;
			}
		}

		framesPassedAfterLastUpdate++;
		cout << "Total frames passed: " << totalFramesPassed << "\n";
		if (framesPassedAfterLastUpdate == waitForFramesToPass) {
			cout << "Foreground mask #" << framesPassedAfterLastUpdate << " updated!";
			framesPassedAfterLastUpdate = 0;
			updatedForegroundMask = ocvDepthFrameData;
		}
		if (updatedForegroundMask.rows != 0)
			foregroundMaskIsAvailable = true;
	}

	// Performs background-subtraction and produces a masked-foreground
	void Functions::ApplyForegroundMasking() {
		if (totalFramesPassed < stopUpdatingBackgroundAfterFrames) {
			pointerToSubtractorObject_MOG2->apply(ocvDepthFrameData, foregroundMask_MOG2, 0.01);
		}
		else {
			pointerToSubtractorObject_MOG2->apply(ocvDepthFrameData, foregroundMask_MOG2, 0);
		}
	}

	// Function performs denoising of background-subtracted depth frame
	// 'Hole-filling' is performed before return to fill gaps in blobs
	void Functions::CleanForegroundMask() {
		Mat *toBeCleanedFrame, *cleanedFrame;
		toBeCleanedFrame = &foregroundMask_MOG2;
		cleanedFrame = &ocvDepthFrameFGMaskCleaned_MOG2;

		// De-noising
		medianBlur(*toBeCleanedFrame, *cleanedFrame, 7);
		medianBlur(*cleanedFrame, *cleanedFrame, 3);
		GaussianBlur(*cleanedFrame, *cleanedFrame, Size(3, 3), 3, 3);
		threshold(*cleanedFrame, *cleanedFrame, 200.0, 255.0, THRESH_BINARY);
		erode(*cleanedFrame, *cleanedFrame, morphingKernel_size3, Point(-1, -1), 3);

		//Hole-filling
		dilate(*cleanedFrame, *cleanedFrame, morphingKernel_size3, Point(-1, -1), 3);
		Mat foregroundComplement = (*cleanedFrame).clone();
		floodFill(foregroundComplement, Point(0, 0), Scalar(255));
		bitwise_not(foregroundComplement, foregroundComplement);
		*cleanedFrame = (*cleanedFrame | foregroundComplement);
		return;
	}

	// Use this function to initialize blob-tracker
	// Parameters are fixed
	void Functions::InitializeBlobTracker() {
		// Setup SimpleBlobDetector parameters.
		SimpleBlobDetector::Params params;

		params.minDistBetweenBlobs = 50;

		// Filter by color
		params.filterByColor = true;
		params.blobColor = 255;

		// Change thresholds
		params.minThreshold = 0;
		params.maxThreshold = 1000;

		// Filter by Area
		params.filterByArea = true;
		params.minArea = 1000;
		params.maxArea = 100000;

		// Filter by Circularity
		params.filterByCircularity = false;
		params.minCircularity = 0.1;

		// Filter by Convexity
		params.filterByConvexity = false;
		params.minConvexity = 0.87;

		// Filter by Inertia
		params.filterByInertia = false;
		params.minInertiaRatio = 0.01;

#if CV_MAJOR_VERSION < 3   // If you are using OpenCV 2

		// Set up detector with params
		SimpleBlobDetector detector(params);

		// You can use the detector this way
		// detector.detect( im, keypoints);

#else

		// Set up detector with params
		// SimpleBlobDetector::create creates a smart pointer. 
		// So you need to use arrow ( ->) instead of dot ( . )
		// detector->detect( im, keypoints);
		blobDetector = SimpleBlobDetector::create(params);
#endif
	}

	// Use function to create a circular visualizer for blob-tracking points
	void Functions::MyFilledCircle(Mat* img, Point center) {
		int w = 400;
		circle(*img,
			center,
			w / 32,
			Scalar(0, 223, 255),
			FILLED,
			LINE_8);
	}

	// Function will return total size of RGB frame
	long Functions::GetRGBDataSize() {
		return rgbFrameHeight * rgbFrameWidth * 4;
	}

	// Function will return height of RGB frame
	int Functions::GetRGBFrameHeight() {
		return rgbFrameHeight;
	}

	// Function will return width of RGB frame
	int Functions::GetRGBFrameWidth() {
		return rgbFrameWidth;
	}

	// Function will return total size of Depth frame
	long Functions::GetDepthDataSize() {
		return depthFrameHeight * depthFrameWidth;
	}

	// Function will return height of Depth frame
	int Functions::GetDepthFrameHeight() {
		return depthFrameHeight;
	}

	// Function will return width of Depth frame
	int Functions::GetDepthFrameWidth() {
		return depthFrameWidth;
	}

	// Function will return a byte-array containing data from latest RGB data
	byte* Functions::GetRGBData() {
		try {
			byte *rgbDataArray = rgbData;
			return rgbData;
			//return rgbDataArray;
		} catch (int e) {
			for (int i = 0; i < rgbFrameHeight*rgbFrameWidth; i++) {
				rgbData[4 * i] = 255;
				rgbData[4 * i + 1] = 255;
				rgbData[4 * i + 2] = 0;
				rgbData[4 * i + 3] = 255;
			}
			byte * rgbDataArray = rgbData;
			return rgbDataArray;
		}
	}

	// Function will return a byte-array containing raw depth data from latest Depth frame
	byte* Functions::GetRawDepthData() {
		try {
			byte * rawDepthDataArray = rawDepthData;
			return rawDepthDataArray;
		}
		catch (int e) {
			for (int i = 0; i < depthFrameHeight*depthFrameWidth; i++) {
				rawDepthData[i] = 128;
			}
			byte * rawDepthDataArray = rawDepthData;
			return rawDepthDataArray;
		}
	}

	// Function will return a byte-array containing range-limited depth data from latest Depth frame
	byte* Functions::GetRangeLimitedDepthData() {
		try {
			byte * rangeLimitedDepthDataArray = rangeLimitedDepthData;
			return rangeLimitedDepthDataArray;
		} catch (int e) {
			for (int i = 0; i < depthFrameHeight*depthFrameWidth; i++) {
				rangeLimitedDepthData[i] = 128;
			}
			byte * rangeLimitedDepthDataArray = rangeLimitedDepthData;
			return rangeLimitedDepthDataArray;
		}
	}

	// Function will return a byte-array containing depth data for only blob regions
	byte* Functions::GetBlobsBasedDepthData() {
		try {
			int size = ocvDepthFrameFGMaskCleaned_MOG2.total() * ocvDepthFrameFGMaskCleaned_MOG2.elemSize();
			memcpy(blobsBasedDepthData, ocvDepthFrameFGMaskCleaned_MOG2.data, size * sizeof(byte));
			//for (int i = 0; i < depthFrameHeight*depthFrameWidth; i++) {
			//	int xIndex = i % depthFrameWidth;
			//	int yIndex = i / depthFrameHeight;
			//	//if(ocvDepthFrameFGMaskCleaned_MOG2.at<uchar>(yIndex, xIndex) > 0)
			//		//blobsBasedDepthData[i] = rangeLimitedDepthData[i];
			//}
			byte * blobBasedDepthDataArray = blobsBasedDepthData;
			return blobBasedDepthDataArray;
		} catch (int e) {
			for (int i = 0; i < depthFrameHeight*depthFrameWidth; i++) {
				blobsBasedDepthData[i] = 128;
			}
			byte * blobBasedDepthDataArray = blobsBasedDepthData;
			return blobBasedDepthDataArray;
		}
	}

	// Function will return a byte-array containing depth data for visualization
	byte* Functions::GetDepthDataForVisualization() {
		try {
			int size = ocvDepthFrameDataForVisualization.total() * ocvDepthFrameDataForVisualization.elemSize();
			memcpy(depthDataForVisualization, ocvDepthFrameDataForVisualization.data, size * sizeof(byte));
			byte * depthDataForVisualizationArray = depthDataForVisualization;
			return depthDataForVisualizationArray;
		}
		catch (int e) {
			for (int i = 0; i < depthFrameHeight*depthFrameWidth; i++) {
				depthDataForVisualization[3 * i] = 255;
				depthDataForVisualization[3 * i + 1] = 255;
				depthDataForVisualization[3 * i + 2] = 0;
			}
			byte * depthDataForVisualizationArray = rangeLimitedDepthData;
			return depthDataForVisualization;
		}
	}

	// Function initializes depth sensor as well as OpenCV modules.
	// Initiates receiving of RGB and Depth streams from sensor as well.
	// Returns 'TRUE'/'FALSE'. Accepts min and max observable distances.
	// Function activates processing for visualizing tracking feed.
	// Visualization slows down tracking as new feed is generated.
	// Hence, default value for activateVisualization is FALSE.
	bool Functions::InitializeTracking(int minDistance, int maxDistance, bool activateVisualization) {
		if (!InitializeKinect(minDistance, maxDistance)) {
			return false;
		}
		showVisualization = activateVisualization;
		try {
			InitializeForegroundMasking();
			InitializeBlobTracker();
			return true;
		} catch (int e) {
			return false;
		}
	}

	// Function returns status of blob-tracking thread
	// Recommended to check status before calling 'TrackInFrame' function
	bool Functions::IsBlobTrackingThreadRunning() {
		return processingNow;
	}

	// Function will track blobs for only current frame
	bool Functions::TrackInFrame() {
		processingNow = true;
		thread kinectFrameFetchThread(ContinuousFetchFrameAsync);
		kinectFrameFetchThread.join();

		UpdateForegroundMask();
		//foregroundMaskIsAvailable = true;
		if (foregroundMaskIsAvailable) {
			ApplyForegroundMasking();
			CleanForegroundMask();
		}

		blobDetector->detect(ocvDepthFrameFGMaskCleaned_MOG2, keypoints);

		
		if (collectionOfBlobs.size() > 0) {
			collectionOfBlobs.clear();
		}
		
		MyBlob newBlob;
		for (int i = 0; i < keypoints.size(); i++) {
			int blobCoordinateX = keypoints.at(i).pt.x;
			int blobCoordinateY = keypoints.at(i).pt.y;
			newBlob.normalizedBlobCoordinates[0] = (double)(blobCoordinateX - depthFrameHalfWidth) / (double)depthFrameHalfWidth;
			newBlob.normalizedBlobCoordinates[1] = (double)(blobCoordinateY - depthFrameHalfHeight) / (double)depthFrameHalfHeight;
			newBlob.depthValue = rangeLimitedDepthData[depthFrameWidth * blobCoordinateY + blobCoordinateX];
			collectionOfBlobs.push_back(newBlob);
		}

		if (showVisualization) {
			for (int i = 0; i < keypoints.size(); i++) {
				Point center = keypoints.at(i).pt;
				MyFilledCircle(&ocvDepthFrameDataForVisualization, center);
			}
		}

		processingNow = false;
		return true;
	}

	// Function returns number of blobs detected
	int Functions::GetNumberOfBlobs() {
		if (!collectionOfBlobs.empty()) {
			return collectionOfBlobs.size();
		} else {
			return 0;
		}
	}

	// Get number of blobs detected before using this
	// Each blob has (x, y) coordinates pair and depth value of its center
	// [0] -> x-Coordinate, [1] -> y-Coordinate, [2] -> depth value at center
	double* Functions::GetBlobsData() {
		int numberOfBlobs = collectionOfBlobs.size();
		double *blobsData = new double[numberOfBlobs*3];
		int iteration = 0;
		for (list<MyBlob>::iterator blobIterator = collectionOfBlobs.begin(); blobIterator != collectionOfBlobs.end(); blobIterator++) {
			blobsData[3*iteration    ] = (*blobIterator).normalizedBlobCoordinates[0];
			blobsData[3*iteration + 1] = (*blobIterator).normalizedBlobCoordinates[1];
			blobsData[3*iteration + 2] = (double)(*blobIterator).depthValue;
			iteration++;
		}

		return blobsData;
	}

	// Use this function to delete blobs data and frees memory
	int Functions::DeleteBlobsData(double* blobsData) {
		delete[] blobsData;
		return 0;
	}

	// Function closes sensor and frees memory
	bool Functions::Close() {
		if (sensor)
			sensor->Close();

		if (rgbStreamReader)
			rgbStreamReader->Release();

		if(depthStreamReader)
			depthStreamReader->Release();

		if(blobDetector)
			blobDetector->clear();

		if(keypoints.size() > 0)
			keypoints.clear();

		if(collectionOfBlobs.size() > 0)
			collectionOfBlobs.clear();

		if(pointerToSubtractorObject_MOG2)
			pointerToSubtractorObject_MOG2->clear();

		return true;
	}
}
# OpenCV for Unity
This project is provided for implementing simple Computer Vision (CV) and Digital Image Processing (DIP) functions in C++ and using them inside Unity.

Since Unity is comparatively slower than a C++ program in executing matrix-based operations, functions are called from a C++ DLL to speed up DIP and CV tasks. Bindings have been provided for Kinect V1 (Kinect SDK 1.8) and will be updated for Kinect V2 ((Kinect SDK 2.0)) soon.

Unity versions above 2017 are supported. Also works for Unity version 5.5.4.

The Visual studio projects folders contain code for generating the DLL. All dependencies are included in the folders and are linked properly.

## Data-flow (Advanced users)
Both data flows are possible:
1. Easy to develop - Kinect SDK (C++) --> Unity (C#) --> OpenCV (C++) --> Unity (C#)
2. Good for optimization - Kinect SDK + OpenCV (C++) --> Unity

## NOTE 1: This project is not a one-stop solution for all OpenCV functions
It is rather a working guide to demonstrate how OpenCV can be linked with Unity via C++ DLLs. To demonstrate the capabilities, Kinect was chosen to provide a steady RGB/Depth feed. But in principle, OpenCV and Kinect can be used separately with Unity as well in this project.

## NOTE 2: Add functions and update the C++ DLL to increase functionality
The current project contains very basic functions available in OpenCV to demonstrate a proof-of-concept for the data-flow pipelines (see above). If there is a need to modify the project to include more functions from OpenCV, then they can be implemented by using the Visual Studio project. In such a case, a new DLL should be generated after once new functions are added in the C++ file, followed by replacing the older DLL by the newer one at the following path - 
Path-to-project\OpenCVForUnity\UnityProject\OpenCVForUnity\Assets\Plugins\x64


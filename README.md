# OpenCV for Unity
This project is provided for implementing simple Computer Vision (CV) and Digital Image Processing (DIP) functions in C++ and using them inside Unity.
Since Unity is comparatively slower in executing matrix-based operations, functions are called from a DLL to speed up DIP and CV tasks.
Bindings have been provided for Kinect V1 (Kinect SDK 1.8) and will be updated for Kinect V2 ((Kinect SDK 2.0)) soon.
Unity versions above 2017 are supported. Also works for Unity version 5.5.4.
The Visual studio projects folders contain code for generating the DLL. All dependencies are included in the folders and are linked properly.

##NOTE 1: This project is not a solution to solve all the problems but rather a working guide to demonstrate how Kinect and OpenCV can be linked with Unity via C++ DLLs, which goes to say that OpenCV and Kinect can be used separately with Unity as well without one requiring the other.
##NOTE 2: If there is a need to modify the project to include more functions from OpenCV. Then they can be implemented by using the Visual Studio project. A new DLL must be generated in such a case. 


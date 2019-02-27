using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BilinearInterpolation {

    private BilinearInterpolation instance;
    public BilinearInterpolation Instance
    {
        get
        {
            return this;
        }
    }

    // Get parameters necessary for performing Bilinear Interpolation
	public static void GetBilinearInterpolationParameters (ref int sourceWidth, ref int sourceHeight, ref int targetWidth, ref int targetHeight, ref float scaleX, ref float scaleY, ref int targetIndex,
														   ref float resultDeltaX, ref float resultDeltaY, ref int[] resultNeighborPixelsIndices)
	{
		//Optimizations: No need to calculate i and j for image boundaries
		//Ex. i = 0 for all pixels for 1st row OR j = width-1 for all pixels for last column
		//Boundary conditions: i+1 or j+1 should not exceed original height/width to avoid seg. fault
		int i = 0, j = 0;
		int p = targetIndex / targetWidth;
		int q = targetIndex % targetWidth;

		if (p == 0) {
			i = 0;
			resultDeltaY = 1;
		} else if (p == targetHeight - 1) {
			i = sourceHeight - 2;
			resultDeltaY = 0;
		} else {
			float i_exact = Mathf.Abs ((((float)(p + 1)) / scaleY) - 1);
			float i_nearest = Mathf.Round (i_exact * 100) / 100;
			i = (int)(i_nearest);
			resultDeltaY = i_exact - i_nearest;
			if (i > sourceHeight - 2)
				i = sourceHeight - 2;
		}

		if (q == 0) {
			j = 0;
			resultDeltaX = 1;
		} else if (q == targetWidth - 1) {
			j = sourceWidth - 2;
			resultDeltaX = 0;
		} else {
			float j_exact = Mathf.Abs ((((float)(q + 1)) / scaleX) - 1);
			float j_nearest = Mathf.Round (j_exact * 100) / 100;
			j = (int)(j_nearest);
			resultDeltaX = j_exact - j_nearest;
			if (j > sourceWidth - 2)
				j = sourceWidth - 2;
		}

		resultNeighborPixelsIndices [0] = i * sourceWidth + j;
		resultNeighborPixelsIndices [1] = resultNeighborPixelsIndices [0] + 1;
		resultNeighborPixelsIndices [2] = resultNeighborPixelsIndices [0] + sourceWidth;
		resultNeighborPixelsIndices [3] = resultNeighborPixelsIndices [2] + 1;
		return;
	}

    /// FLOAT
	// Gets the bilinear interpolated FLOAT value - 4 input FLOAT values
    public static void GetBilinearInterpolatedValue(ref float delta_x, ref float delta_y, ref float topLeftValue, ref float topRightValue, ref float bottomLeftValue, ref float bottomRightValue, ref float result)
    {
        result = (delta_y * (delta_x * topLeftValue + (1 - delta_x) * topRightValue) + (1 - delta_y) * (delta_x * bottomLeftValue + (1 - delta_x) * bottomRightValue));
        return;
    }

	// Gets the bilinear interpolated FLOAT value - 4 input indices and source FLOAT array
	public static void GetBilinearInterpolatedValue(ref float delta_x, ref float delta_y, ref int topLeftIndex, ref int topRightIndex, ref int bottomLeftIndex, ref int bottomRightIndex, ref float[] data, ref float result)
    {
        result = (delta_y * (delta_x * data[topLeftIndex] + (1 - delta_x) * data[topRightIndex]) + (1 - delta_y) * (delta_x * data[bottomLeftIndex] + (1 - delta_x) * data[bottomRightIndex]));
        return;
    }

	// Gets the bilinear interpolated FLOAT value - input FLOAT array having 4 values
    public static void GetBilinearInterpolatedValue(ref float delta_x, ref float delta_y, ref float[] normalizedDepthsAtNeighborPixels, ref float result)
    {
        result = (delta_y * (delta_x * normalizedDepthsAtNeighborPixels[0] + (1 - delta_x) * normalizedDepthsAtNeighborPixels[1]) + (1 - delta_y) * (delta_x * normalizedDepthsAtNeighborPixels[2] + (1 - delta_x) * normalizedDepthsAtNeighborPixels[3]));
        return;
    }

    /// USHORT
	/// Gets the bilinear interpolated USHORT value - 4 input USHORT values
    public static void GetBilinearInterpolatedValue(ref float delta_x, ref float delta_y, ref int topLeftIndex, ref int topRightIndex, ref int bottomLeftIndex, ref int bottomRightIndex, ref ushort[] data, ref ushort result)
    {
        result = (ushort)(delta_y * (delta_x * (float)data[topLeftIndex] + (1 - delta_x) * (float)data[topRightIndex]) + (1 - delta_y) * (delta_x * (float)data[bottomLeftIndex] + (1 - delta_x) * (float)data[bottomRightIndex]));
        return;
    }

	// Gets the bilinear interpolated USHORT value - 4 input indices and source USHORT array
    public static void GetBilinearInterpolatedValue(ref float delta_x, ref float delta_y, ref int[] indexesOfNeighborPixels, ref ushort[] data, ref ushort result)
    {
        result = (ushort)(delta_y * (delta_x * (float)data[indexesOfNeighborPixels[0]] + (1 - delta_x) * (float)data[indexesOfNeighborPixels[1]])
            + (1 - delta_y) * (delta_x * (float)data[indexesOfNeighborPixels[2]] + (1 - delta_x) * (float)data[indexesOfNeighborPixels[3]]));
        return;
    }

	// Gets the bilinear interpolated USHORT value - input USHORT array having 4 values
	public static void GetBilinearInterpolatedValue(ref float delta_x, ref float delta_y, ref ushort[] correctedDepthsAtNeighborPixels, ref ushort result)
    {
		result = (ushort)(delta_y * (delta_x * (float)correctedDepthsAtNeighborPixels[0] + (1 - delta_x) * (float)correctedDepthsAtNeighborPixels[1])
			+ (1 - delta_y) * (delta_x * (float)correctedDepthsAtNeighborPixels[2] + (1 - delta_x) * (float)correctedDepthsAtNeighborPixels[3]));
        return;
    }

    /// COLOR32
	// Gets the bilinear interpolated COLOR32 value - 4 input COLOR32 values
    public static void GetBilinearInterpolatedValue(ref float delta_x, ref float delta_y, ref Color32 topLeftColor, ref Color32 topRightColor, ref Color32 bottomLeftColor, ref Color32 bottomRightColor, ref Color32 result)
    {
        result.r = (byte)(delta_y * (delta_x * topLeftColor.r + (1 - delta_x) * topRightColor.r) + (1 - delta_y) * (delta_x * bottomLeftColor.r + (1 - delta_x) * bottomRightColor.r));
        result.g = (byte)(delta_y * (delta_x * topLeftColor.g + (1 - delta_x) * topRightColor.g) + (1 - delta_y) * (delta_x * bottomLeftColor.g + (1 - delta_x) * bottomRightColor.g));
        result.b = (byte)(delta_y * (delta_x * topLeftColor.b + (1 - delta_x) * topRightColor.b) + (1 - delta_y) * (delta_x * bottomLeftColor.b + (1 - delta_x) * bottomRightColor.b));
        result.a = 255;
        return;
    }

	// Gets the bilinear interpolated COLOR32 value - 4 input indices and source COLOR32 array
    public static void GetBilinearInterpolatedValue(ref float delta_x, ref float delta_y, ref Color32[] colorsOfNeighborPixels, ref Color32 result)
    {
        result.r = (byte)(delta_y * (delta_x * colorsOfNeighborPixels[0].r + (1 - delta_x) * colorsOfNeighborPixels[1].r) + (1 - delta_y) * (delta_x * colorsOfNeighborPixels[2].r + (1 - delta_x) * colorsOfNeighborPixels[3].r));
        result.g = (byte)(delta_y * (delta_x * colorsOfNeighborPixels[0].g + (1 - delta_x) * colorsOfNeighborPixels[1].g) + (1 - delta_y) * (delta_x * colorsOfNeighborPixels[2].g + (1 - delta_x) * colorsOfNeighborPixels[3].g));
        result.b = (byte)(delta_y * (delta_x * colorsOfNeighborPixels[0].b + (1 - delta_x) * colorsOfNeighborPixels[1].b) + (1 - delta_y) * (delta_x * colorsOfNeighborPixels[2].b + (1 - delta_x) * colorsOfNeighborPixels[3].b));
        result.a = 255;
        return;
    }

	// Gets the bilinear interpolated COLOR32 value - input COLOR32 array having 4 values
    public static void GetBilinearInterpolatedValue(ref float delta_x, ref float delta_y, ref int topLeftIndex, ref int topRightIndex, ref int bottomLeftIndex, ref int bottomRightIndex, ref Color32[] colorData, ref Color32 result)
    {
        result.r = (byte)(delta_y * (delta_x * colorData[topLeftIndex].r + (1 - delta_x) * colorData[topRightIndex].r) + (1 - delta_y) * (delta_x * colorData[bottomLeftIndex].r + (1 - delta_x) * colorData[bottomRightIndex].r));
        result.g = (byte)(delta_y * (delta_x * colorData[topLeftIndex].g + (1 - delta_x) * colorData[topRightIndex].g) + (1 - delta_y) * (delta_x * colorData[bottomLeftIndex].g + (1 - delta_x) * colorData[bottomRightIndex].g));
        result.b = (byte)(delta_y * (delta_x * colorData[topLeftIndex].b + (1 - delta_x) * colorData[topRightIndex].b) + (1 - delta_y) * (delta_x * colorData[bottomLeftIndex].b + (1 - delta_x) * colorData[bottomRightIndex].b));
        result.a = 255;
        return;
    }
}

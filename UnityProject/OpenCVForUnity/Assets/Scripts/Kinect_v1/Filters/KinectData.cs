using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinectData
{
	public long Size;
	public int Width, Height;

	public ushort[] RawDepths;
	public ushort[] CorrectedDepths;
	public float[] NormalizedDepths;
	public Color32[] DepthStreamColors;
	public Color32[] RawColorStreamColors;
	public Color32[] GradedDepthStreamColors;
	public Color32[] RegisteredColorStreamColors;

	public KinectData (int width, int height)
	{
		Width = width;
		Height = height;
		Size = width * height;

		RawDepths = new ushort[Size];
		NormalizedDepths = new float[Size];
		CorrectedDepths = new ushort[Size];
		DepthStreamColors = new Color32[Size];
		RawColorStreamColors = new Color32[Size];
		GradedDepthStreamColors = new Color32[Size];
		RegisteredColorStreamColors = new Color32[Size];
	}
}

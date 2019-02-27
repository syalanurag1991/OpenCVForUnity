using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KinectDisplay : MonoBehaviour {

	// For holding imported processed kinect data
	private KinectData lowResolutionKinectDataInstance;
	private KinectData fullResolutionKinectDataInstance;

	// Supporting textures for displaying streams
	private Texture2D depthStreamTexture;
	private Texture2D rawColorStreamTexture;
	private Texture2D gradedDepthStreamTexture;
	private Texture2D registeredColorStreamTexture;

	private Texture2D lowResolutionDepthStreamTexture;
	private Texture2D lowResolutionRawColorStreamTexture;
    private Texture2D lowResolutionGradedDepthStreamTexture;
	private Texture2D lowResolutionRegisteredColorStreamTexture;

	// Display area on UI for streams
    public RawImage depthStreamDisplay;
	public RawImage rawColorStreamDisplay;
	public RawImage gradedDepthStreamDisplay;
	public RawImage registeredColorStreamDisplay;

    public RawImage lowResolutionDepthStreamDisplay;
	public RawImage lowResolutionRawColorStreamDisplay;
	public RawImage lowResolutionGradedDepthStreamDisplay;
	public RawImage lowResolutionRegisteredColorStreamDisplay;

	// Use this for initialization
	void Start ()
	{
		lowResolutionKinectDataInstance = KinectManager.Instance.GetLowResolutionKinectDataInstance();
		fullResolutionKinectDataInstance = KinectManager.Instance.GetFullResolutionKinectDataInstance();

		depthStreamTexture = new Texture2D(fullResolutionKinectDataInstance.Width, fullResolutionKinectDataInstance.Height, TextureFormat.BGRA32, false);
		rawColorStreamTexture = new Texture2D(fullResolutionKinectDataInstance.Width, fullResolutionKinectDataInstance.Height, TextureFormat.BGRA32, false);
		gradedDepthStreamTexture = new Texture2D(fullResolutionKinectDataInstance.Width, fullResolutionKinectDataInstance.Height, TextureFormat.BGRA32, false);
		registeredColorStreamTexture = new Texture2D(fullResolutionKinectDataInstance.Width, fullResolutionKinectDataInstance.Height, TextureFormat.BGRA32, false);

		lowResolutionDepthStreamTexture = new Texture2D(lowResolutionKinectDataInstance.Width, lowResolutionKinectDataInstance.Height, TextureFormat.BGRA32, false);
		lowResolutionRawColorStreamTexture = new Texture2D(lowResolutionKinectDataInstance.Width, lowResolutionKinectDataInstance.Height, TextureFormat.BGRA32, false);
		lowResolutionGradedDepthStreamTexture = new Texture2D(lowResolutionKinectDataInstance.Width, lowResolutionKinectDataInstance.Height, TextureFormat.BGRA32, false);
		lowResolutionRegisteredColorStreamTexture = new Texture2D(lowResolutionKinectDataInstance.Width, lowResolutionKinectDataInstance.Height, TextureFormat.BGRA32, false);

		SetStreamTextures();
	}
	
	// Update is called once per frame
	void Update ()
	{
		UpdateStreamTextures();
	}

	void SetStreamTextures()
	{
		// Full resolution depth stream
        if (depthStreamDisplay != null)
            depthStreamDisplay.texture = depthStreamTexture;

		// Full resolution raw color stream
        if (rawColorStreamDisplay != null)
			rawColorStreamDisplay.texture = rawColorStreamTexture;

		// Full resolution graded depth stream
        if (gradedDepthStreamDisplay != null)
            gradedDepthStreamDisplay.texture = gradedDepthStreamTexture;

		// Low resolution registered color stream
		if (registeredColorStreamDisplay != null)
			registeredColorStreamDisplay.texture = registeredColorStreamTexture;
        
		// Low resolution depth stream
        if (lowResolutionDepthStreamDisplay != null)
			lowResolutionDepthStreamDisplay.texture = lowResolutionDepthStreamTexture;

		// Low resolution raw color stream
		if (lowResolutionRawColorStreamDisplay != null)
        	lowResolutionRawColorStreamDisplay.texture = lowResolutionRawColorStreamTexture;

		// Low resolution graded depth stream
		if (lowResolutionGradedDepthStreamDisplay != null)
			lowResolutionGradedDepthStreamDisplay.texture = lowResolutionGradedDepthStreamTexture;

		// Low resolution registered color stream
		if (lowResolutionRegisteredColorStreamDisplay != null)
			lowResolutionRegisteredColorStreamDisplay.texture = lowResolutionRegisteredColorStreamTexture;
    }

	// Update depth data, registered RGB and graded depth images
    void UpdateStreamTextures()
    {
        // Update depth stream
        depthStreamTexture.SetPixels32(fullResolutionKinectDataInstance.DepthStreamColors);

        // Update raw color stream
        rawColorStreamTexture.SetPixels32(fullResolutionKinectDataInstance.RawColorStreamColors);
        
        // Update graded depth stream
		gradedDepthStreamTexture.SetPixels32(fullResolutionKinectDataInstance.GradedDepthStreamColors);

        // Update registered color stream
		registeredColorStreamTexture.SetPixels32(fullResolutionKinectDataInstance.RegisteredColorStreamColors);


        // Update low resolution depth stream
		lowResolutionDepthStreamTexture.SetPixels32(lowResolutionKinectDataInstance.DepthStreamColors);

		// Update low resolution raw color stream
		lowResolutionRawColorStreamTexture.SetPixels32(lowResolutionKinectDataInstance.RawColorStreamColors);

        // Update low resolution graded depth stream
		lowResolutionGradedDepthStreamTexture.SetPixels32(lowResolutionKinectDataInstance.GradedDepthStreamColors);

		// Update low resolution registered color stream
		lowResolutionRegisteredColorStreamTexture.SetPixels32(lowResolutionKinectDataInstance.RegisteredColorStreamColors);

		// Apply all texture changes 
		depthStreamTexture.Apply(false, false);
		rawColorStreamTexture.Apply(false, false);
        gradedDepthStreamTexture.Apply(false, false);
        registeredColorStreamTexture.Apply(false, false);

		lowResolutionDepthStreamTexture.Apply(false, false);
		lowResolutionRawColorStreamTexture.Apply(false, false);
		lowResolutionGradedDepthStreamTexture.Apply(false, false);
		lowResolutionRegisteredColorStreamTexture.Apply();
    }
}

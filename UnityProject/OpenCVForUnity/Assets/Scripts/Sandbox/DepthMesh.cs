using UnityEngine;
using System.Collections;

public class DepthMesh : MonoBehaviour
{
    int KinectWidth = 640;
    int KinectHeight = 480;
    [HideInInspector]
    public Vector3[] newVertices;
    [HideInInspector]
    public Vector3[] newNormals;
    [HideInInspector]
    public Color32[] newColors;
    [HideInInspector]
    public Vector2[] newUV;
    [HideInInspector]
    public int[] newTriangles;
    Mesh MyMesh;

    [HideInInspector]
    public int finalWidth = KinectWrapper.Constants.TargetWidth;
	[HideInInspector]
	public int finalHeight = KinectWrapper.Constants.TargetHeight;
    public int OffsetX;
    public int OffsetY;
    private int minDepthValue = 0;
	private int maxDepthValue = (ushort.MaxValue) >> 4;
	[HideInInspector]
    public float MeshHeight = 100f;

    ushort MinDepthValueBuffer;
    ushort MaxDepthValueBuffer;
    ushort[] DepthImage;
    ushort[] interpolatedDepthData;
    float[] FloatValues;

    int WidthBuffer;
    int HeightBuffer;
    
    // Sandbox dimensions in mm
    public float length = 1000f;
    public float breadth = 800f;
    public float distanceFromKinect = 1000f;
    [HideInInspector]
    public float angle = 0.03f;
    private ushort[] mappingCorrections;

    KinectManager kinectManagerInstance;

    // Use this for initialization
    void Start()
    {
		kinectManagerInstance = KinectManager.Instance;

        WidthBuffer = finalWidth;
        HeightBuffer = finalHeight;

        MyMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = MyMesh;

        SetupArrays();

        mappingCorrections = new ushort[KinectWrapper.GetDepthHeight() * KinectWrapper.GetDepthWidth()];
    }

    // Update is called once per frame
    void Update()
    {
    	maxDepthValue = kinectManagerInstance.level_max_short;
		minDepthValue = kinectManagerInstance.level_min_short;

        //if (KinectDepth.pollDepth()) {
        DepthImage = kinectManagerInstance.GetCorrectedDepthData();
        CheckArrays();
		interpolatedDepthData = kinectManagerInstance.GetLowResolutionDepthData();
        //CropImage();
        //FloatValues = kinectManagerInstance.GetInterpolatedNormalizedDepthData();
        CalculateFloatValues();
        UpdateMesh();
        //}
    }

    void CheckArrays()
    {
        if ((finalWidth != WidthBuffer) || (finalHeight != HeightBuffer))
        {
            SetupArrays();
            WidthBuffer = finalWidth;
            HeightBuffer = finalHeight;
        }
    }

    void SetupArrays()
    {
        interpolatedDepthData = new ushort[finalWidth * finalHeight];
        FloatValues = new float[finalWidth * finalHeight];
        newVertices = new Vector3[finalWidth * finalHeight];
        newNormals = new Vector3[finalWidth * finalHeight];
        newColors = new Color32[finalWidth * finalHeight];
        newUV = new Vector2[finalWidth * finalHeight];
        newTriangles = new int[(finalWidth - 1) * (finalHeight - 1) * 6];

        Debug.Log("Number of triangles: " + newTriangles.Length);

        for (int H = 0; H < finalHeight; H++)
        {
            for (int W = 0; W < finalWidth; W++)
            {
                int Index = GetArrayIndex(W, H);
                newVertices[Index] = new Vector3(W, H, 0f);
                newNormals[Index] = new Vector3(0, 0, 1);
                newColors[Index] = new Color32(0, 0, 0, 255);
                newUV[Index] = new Vector2(W / (float)finalWidth, H / (float)finalHeight);

                if ((W != (finalWidth - 1)) && (H != (finalHeight - 1)))
                {
                    int TopLeft = Index;
                    int TopRight = Index + 1;
                    int BotLeft = Index + finalWidth;
                    int BotRight = Index + 1 + finalWidth;

                    int TrinagleIndex = W + H * (finalWidth - 1);
                    newTriangles[TrinagleIndex * 6 + 0] = TopLeft;
                    newTriangles[TrinagleIndex * 6 + 1] = BotLeft;
                    newTriangles[TrinagleIndex * 6 + 2] = TopRight;
                    newTriangles[TrinagleIndex * 6 + 3] = BotLeft;
                    newTriangles[TrinagleIndex * 6 + 4] = BotRight;
                    newTriangles[TrinagleIndex * 6 + 5] = TopRight;
                }
            }
        }

        MyMesh.Clear();
        MyMesh.vertices = newVertices;
        MyMesh.normals = newNormals;
        MyMesh.colors32 = newColors;
        MyMesh.uv = newUV;
        MyMesh.triangles = newTriangles;
    }

    void CropImage()
    {
        for (int H = 0; H < finalHeight; H++)
        {
            for (int W = 0; W < finalWidth; W++)
            {
                int Index = GetArrayIndex(W, H);
                ushort Value = (ushort)GetImageValue(W, H);
                interpolatedDepthData[Index] = Value;
            }
        }
    }

    void CalculateFloatValues()
    {
        for (int H = 0; H < finalHeight; H++)
        {
            for (int W = 0; W < finalWidth; W++)
            {
                int index = GetArrayIndex(W, H);
                int ImageValue = interpolatedDepthData[index];

                //Clamp Value

                if (ImageValue > MaxDepthValueBuffer)
                {
                    MaxDepthValueBuffer = (ushort)Mathf.Clamp(ImageValue, ImageValue, ushort.MaxValue);
                }

                if (ImageValue < MinDepthValueBuffer)
                {
                    MinDepthValueBuffer = (ushort)Mathf.Clamp(ImageValue, ushort.MinValue, ImageValue);
                }

                if (ImageValue > maxDepthValue)
                {
                    ImageValue = maxDepthValue;
                }

                if (ImageValue < minDepthValue)
                {
                    ImageValue = minDepthValue;
                }

                //Calculate
                float FloatValue = (ImageValue - minDepthValue) / (float)(maxDepthValue - minDepthValue);
                FloatValues[index] = FloatValue;
            }
        }

    }

    void UpdateMesh()
    {
        MinDepthValueBuffer = ushort.MaxValue;
        MaxDepthValueBuffer = ushort.MinValue;

		for (int H = 0; H < finalHeight; H++)
        {
            for (int W = 0; W < finalWidth; W++)
            {
                //ProcessPixel(W, H);
				int Index = GetArrayIndex(W, H);
				float FloatValue = FloatValues[Index];
				newVertices[Index].z = FloatValue * MeshHeight;
            }
        }

        MyMesh.vertices = newVertices;
        MyMesh.colors32 = newColors;
        MyMesh.RecalculateNormals();
    }

    void ProcessPixel(int W, int H)
    {
        int Index = GetArrayIndex(W, H);
        float FloatValue = FloatValues[Index];

        //Calc Normal
        //newNormals[Index] = CalculateNormal(W, H, FloatValue);

        //Calc Position
        newVertices[Index].z = FloatValue * MeshHeight;

        //Calc Color
        byte ByteValue = (byte)Mathf.RoundToInt(FloatValue * byte.MaxValue);

        //0-127 = 0 :: 127- 255 = 0 - 255
        byte R = (byte)(Mathf.Clamp((ByteValue - 127) * 2, 0, 255));
        //0 = 0; 127 = 255; 255 = 0
        byte G = (byte)(127 + (Mathf.Sign(127 - ByteValue) * ByteValue / 2));
        byte B = (byte)(255 - Mathf.Clamp(ByteValue * 2, 0, 255));
        newColors[Index] = new Color32(R, G, B, 255);
    }

    int GetImageIndex(int W, int H)
    {
        int ImageW = OffsetX + W;
        int ImageH = OffsetY + H;

        if ((ImageW < 0) || (ImageW > KinectWidth) || (ImageH < 0) || (ImageH > KinectHeight))
        {
            return -1;
        }

        return ImageW + ImageH * KinectWidth;
    }

    int GetImageValue(int W, int H)
    {
        int Index = GetImageIndex(W, H);
        if (Index < 0)
        {
            return (int)short.MaxValue;
        }

        int Value = DepthImage[Index];

        if (Value == 0)
        {
            return (int)short.MaxValue;
        }
        else
        {
            return Value;
        }    
    }

    /* Not needed since Unity provides a Recalculate Normals Funktion
    Vector3 CalculateNormal(int W, int H, float VertexFloat)
    {
        int TopIndex = GetArrayIndex(W, H + 1);
        int RightIndex = GetArrayIndex(W + 1, H);
        int BottomIndex = GetArrayIndex(W, H - 1);
        int LeftIndex = GetArrayIndex(W - 1, H);

        Vector3 Normal = Vector3.zero;

        //Get TopLeft
        Normal += CalculateTriangleNormal(LeftIndex, -1, TopIndex, 1, false, VertexFloat);
        //Get TopRight
        Normal += CalculateTriangleNormal(RightIndex, 1, TopIndex, 1, true, VertexFloat);
        //Get BottomLeft
        Normal += CalculateTriangleNormal(LeftIndex, -1, BottomIndex, -1, false, VertexFloat);
        //Get BottomRight
        Normal += CalculateTriangleNormal(RightIndex, 1, BottomIndex, -1, true, VertexFloat);

        return Normal.normalized;
    }

    Vector3 CalculateTriangleNormal(int XIndex, int XOffset, int YIndex, int YOffset, bool Swapped, float VertexFloat)
    {
        if((XIndex < 0) || (YIndex < 0))
        {
            return Vector3.zero;
        }

        if((XIndex >= FloatValues.Length) || (YIndex >= FloatValues.Length))
        {
            Debug.Log("OutofRange: " + FloatValues.Length + " - " + XIndex + " : " + YIndex);
            Debug.Break();
        }

        Vector3 XVector = new Vector3(XOffset, 0, FloatValues[XIndex] - VertexFloat);
        Vector3 YVector = new Vector3(0, YOffset, FloatValues[YIndex] - VertexFloat);

        if(Swapped)
        {
            return Vector3.Cross(XVector, YVector).normalized;
        }
        else
        {
            return Vector3.Cross(YVector, XVector).normalized;
        }
    }
    */

    int GetArrayIndex(int W, int H)
    {
        if ((W < 0) || (W >= finalWidth) || (H < 0) || (H >= finalHeight))
        {
            return -1;
        }

        return W + H * finalWidth;
    }

    int[] ShortToRGBA(short[] DepthImage)
    {
        int[] ImageData = new int[DepthImage.Length];

        for (int i = 0; i < DepthImage.Length; i++)
        {
            ImageData[i] = (int)((((int)DepthImage[i]) << 8) | 0x000000FF);
        }

        return ImageData;
    }

    int RGBAToShort(int Value)
    {
        return (Value >> 8);
    }
}

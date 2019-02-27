using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities : MonoBehaviour
{
	// RotationBetweenQuaternions returns a quaternion that represents a rotation qR such that qA * qR = quaternionB.
    public Quaternion RotationBetweenQuaternions(Quaternion quaternionA, Quaternion quaternionB)
    {
        Quaternion modifiedB = EnsureQuaternionNeighborhood(quaternionA, quaternionB);
        return Quaternion.Inverse(quaternionA) * modifiedB;
    }

    // EnhancedQuaternionSlerp performs a quaternion Slerp, after placing both input quaternions in the same 3D sphere.
    public Quaternion EnhancedQuaternionSlerp(Quaternion quaternionA, Quaternion quaternionB, float amount)
    {
        Quaternion modifiedB = EnsureQuaternionNeighborhood(quaternionA, quaternionB);
        return Quaternion.Slerp(quaternionA, modifiedB, amount);
    }

    // EnsureQuaternionNeighborhood ensures that quaternions qA and quaternionB are in the same 3D sphere in 4D space.
    public Quaternion EnsureQuaternionNeighborhood(Quaternion quaternionA, Quaternion quaternionB)
    {
        if (Quaternion.Dot(quaternionA, quaternionB) < 0)
        {
            // Negate the second quaternion, to place it in the opposite 3D sphere.
            //return -quaternionB;
			return new Quaternion(-quaternionB.x, -quaternionB.y, -quaternionB.z, -quaternionB.w);
        }

        return quaternionB;
    }

    // QuaternionAngle returns the amount of rotation in the given quaternion, in radians.
    public float QuaternionAngle(Quaternion rotation)
    {
        //rotation.Normalize();
        float angle = 2.0f * (float)Mathf.Acos(rotation.w);
        return angle;
    }

    // DistanceToLineSegment finds the distance from a point to a line.
    public Vector4 DistanceToLineSegment(Vector3 linePoint0, Vector3 linePoint1, Vector3 point)
    {
        // find the vector from x0 to x1
        Vector3 lineVec = linePoint1 - linePoint0;
        float lineLength = lineVec.magnitude;
        Vector3 lineToPoint = point - linePoint0;

        const float Epsilon = 0.0001f;

        // if the line is too short skip
        if (lineLength > Epsilon)
        {
            float t = Vector3.Dot(lineVec, lineToPoint) / lineLength;

            // projection is longer than the line itself so find distance to end point of line
            if (t > lineLength)
            {
                lineToPoint = point - linePoint1;
            }
            else if (t >= 0.0f)
            {
                // find distance to line
                Vector3 normalPoint = lineVec;

                // Perform the float->vector conversion once by combining t/fLineLength
                normalPoint *= t / lineLength;
                normalPoint += linePoint0;
                lineToPoint = point - normalPoint;
            }
        }

        // The distance is the size of the final computed line
        float distance = lineToPoint.magnitude;

        // The normal is the final line normalized
        Vector3 normal = lineToPoint / distance;

        return new Vector4(normal.x, normal.y, normal.z, distance);
    }

	// convert the matrix to quaternion, taking care of the mirroring
	private Quaternion ConvertMatrixToQuaternion(Matrix4x4 mOrient, int joint, bool flip)
	{
		Vector4 vZ = mOrient.GetColumn(2);
		Vector4 vY = mOrient.GetColumn(1);

		if(!flip) {
			vZ.y = -vZ.y;
			vY.x = -vY.x;
			vY.z = -vY.z;
		} else {
			vZ.x = -vZ.x;
			vZ.y = -vZ.y;
			vY.z = -vY.z;
		}
		
		if(vZ.x != 0.0f || vZ.y != 0.0f || vZ.z != 0.0f)
			return Quaternion.LookRotation(vZ, vY);
		else
			return Quaternion.identity;
	}

	// draws a line in a texture
	public void DrawLineOnTexture(Texture2D a_Texture, int x1, int y1, int x2, int y2, Color a_Color)
	{
		int width = a_Texture.width;  // KinectWrapper.Constants.DepthImageWidth;
		int height = a_Texture.height;  // KinectWrapper.Constants.DepthImageHeight;
		
		int dy = y2 - y1;
		int dx = x2 - x1;
	 
		int stepy = 1;
		if (dy < 0) {
			dy = -dy; 
			stepy = -1;
		}
		
		int stepx = 1;
		if (dx < 0) {
			dx = -dx; 
			stepx = -1;
		}
		
		dy <<= 1;
		dx <<= 1;
	 
		if(x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
			for(int x = -1; x <= 1; x++)
				for(int y = -1; y <= 1; y++)
					a_Texture.SetPixel(x1 + x, y1 + y, a_Color);
		
		if (dx > dy) {
			int fraction = dy - (dx >> 1);
			
			while (x1 != x2) {
				if (fraction >= 0) {
					y1 += stepy;
					fraction -= dx;
				}
				
				x1 += stepx;
				fraction += dy;
				
				if(x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
					for(int x = -1; x <= 1; x++)
						for(int y = -1; y <= 1; y++)
							a_Texture.SetPixel(x1 + x, y1 + y, a_Color);
			}
		} else {
			int fraction = dx - (dy >> 1);
			
			while (y1 != y2) {
				if (fraction >= 0) {
					x1 += stepx;
					fraction -= dy;
				}
				
				y1 += stepy;
				fraction += dx;
				
				if(x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
					for(int x = -1; x <= 1; x++)
						for(int y = -1; y <= 1; y++)
							a_Texture.SetPixel(x1 + x, y1 + y, a_Color);
			}
		}
	}
}

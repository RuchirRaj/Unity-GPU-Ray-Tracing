using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;
using Unity.Collections.LowLevel.Unsafe;
using System;
using System.Text;
using MPipeline;

public unsafe static class PipelineFunctions
{
    const int CASCADELEVELCOUNT = 4;
    const int Vector2IntSize = 8;
    const int CASCADECLIPSIZE = (CASCADELEVELCOUNT + 1) * sizeof(float);
    /// <summary>
    /// Get Frustum Planes
    /// </summary>
    /// <param name="invVp"></param> View Projection Inverse Matrix
    /// <param name="cullingPlanes"></param> Culling Planes results
    public static void GetCullingPlanes(ref Matrix4x4 invVp, Vector4* cullingPlanes, Vector4* farClipPlane, Vector4* nearClipPlane)
    {
        Vector3 nearLeftButtom = invVp.MultiplyPoint(new Vector3(-1, -1, 1));
        Vector3 nearLeftTop = invVp.MultiplyPoint(new Vector3(-1, 1, 1));
        Vector3 nearRightButtom = invVp.MultiplyPoint(new Vector3(1, -1, 1));
        Vector3 nearRightTop = invVp.MultiplyPoint(new Vector3(1, 1, 1));
        Vector3 farLeftButtom = invVp.MultiplyPoint(new Vector3(-1, -1, 0));
        Vector3 farLeftTop = invVp.MultiplyPoint(new Vector3(-1, 1, 0));
        Vector3 farRightButtom = invVp.MultiplyPoint(new Vector3(1, -1, 0));
        Vector3 farRightTop = invVp.MultiplyPoint(new Vector3(1, 1, 0));
        nearClipPlane[0] = nearLeftButtom;
        nearClipPlane[1] = nearRightButtom;
        nearClipPlane[2] = nearLeftTop;
        nearClipPlane[3] = nearRightTop;
        farClipPlane[0] = farLeftButtom;
        farClipPlane[1] = farRightButtom;
        farClipPlane[2] = farLeftTop;
        farClipPlane[3] = farRightTop;
        Plane plane;
        //Far
        plane = new Plane(farLeftButtom, farRightButtom, farRightTop);
        cullingPlanes[0] = plane.normal;
        cullingPlanes[0].w = plane.distance;
        //Up
        plane = new Plane(farLeftTop, farRightTop, nearRightTop);
        cullingPlanes[1] = plane.normal;
        cullingPlanes[1].w = plane.distance;
        //Down
        plane = new Plane(nearRightButtom, farRightButtom, farLeftButtom);
        cullingPlanes[2] = plane.normal;
        cullingPlanes[2].w = plane.distance;
        //Left
        plane = new Plane(farLeftButtom, farLeftTop, nearLeftTop);
        cullingPlanes[3] = plane.normal;
        cullingPlanes[3].w = plane.distance;
        //Right
        plane = new Plane(farRightButtom, nearRightButtom, nearRightTop);
        cullingPlanes[4] = plane.normal;
        cullingPlanes[4].w = plane.distance;
        //Near
        plane = new Plane(nearRightTop, nearRightButtom, nearLeftButtom);
        cullingPlanes[5] = plane.normal;
        cullingPlanes[5].w = plane.distance;

    }
    public static void GetCullingPlanes(ref Matrix4x4 invVp, Vector4* cullingPlanes)
    {
        Vector3 nearLeftButtom = invVp.MultiplyPoint(new Vector3(-1, -1, 1));
        Vector3 nearLeftTop = invVp.MultiplyPoint(new Vector3(-1, 1, 1));
        Vector3 nearRightButtom = invVp.MultiplyPoint(new Vector3(1, -1, 1));
        Vector3 nearRightTop = invVp.MultiplyPoint(new Vector3(1, 1, 1));
        Vector3 farLeftButtom = invVp.MultiplyPoint(new Vector3(-1, -1, 0));
        Vector3 farLeftTop = invVp.MultiplyPoint(new Vector3(-1, 1, 0));
        Vector3 farRightButtom = invVp.MultiplyPoint(new Vector3(1, -1, 0));
        Vector3 farRightTop = invVp.MultiplyPoint(new Vector3(1, 1, 0));
        Plane plane;
        //Far
        plane = new Plane(farLeftButtom, farRightButtom, farRightTop);
        cullingPlanes[0] = plane.normal;
        cullingPlanes[0].w = plane.distance;
        //Up
        plane = new Plane(farLeftTop, farRightTop, nearRightTop);
        cullingPlanes[1] = plane.normal;
        cullingPlanes[1].w = plane.distance;
        //Down
        plane = new Plane(nearRightButtom, farRightButtom, farLeftButtom);
        cullingPlanes[2] = plane.normal;
        cullingPlanes[2].w = plane.distance;
        //Left
        plane = new Plane(farLeftButtom, farLeftTop, nearLeftTop);
        cullingPlanes[3] = plane.normal;
        cullingPlanes[3].w = plane.distance;
        //Right
        plane = new Plane(farRightButtom, nearRightButtom, nearRightTop);
        cullingPlanes[4] = plane.normal;
        cullingPlanes[4].w = plane.distance;
        //Near
        plane = new Plane(nearRightTop, nearRightButtom, nearLeftButtom);
        cullingPlanes[5] = plane.normal;
        cullingPlanes[5].w = plane.distance;

    }

    public static bool FrustumCulling(ref Matrix4x4 ObjectToWorld, Vector3 extent, Vector4* frustumPlanes)
    {
        Vector3 right = new Vector3(ObjectToWorld.m00, ObjectToWorld.m10, ObjectToWorld.m20);
        Vector3 up = new Vector3(ObjectToWorld.m01, ObjectToWorld.m11, ObjectToWorld.m21);
        Vector3 forward = new Vector3(ObjectToWorld.m02, ObjectToWorld.m12, ObjectToWorld.m22);
        Vector3 position = new Vector3(ObjectToWorld.m03, ObjectToWorld.m13, ObjectToWorld.m23);
        for (int i = 0; i < 6; ++i)
        {
            ref Vector4 plane = ref frustumPlanes[i];
            Vector3 normal = new Vector3(plane.x, plane.y, plane.z);
            float distance = plane.w;
            float r = Vector3.Dot(position, normal);
            Vector3 absNormal = new Vector3(Mathf.Abs(Vector3.Dot(normal, right)), Mathf.Abs(Vector3.Dot(normal, up)), Mathf.Abs(Vector3.Dot(normal, forward)));
            float f = Vector3.Dot(absNormal, extent);
            if ((r - f) >= -distance)
                return false;
        }
        return true;
    }

    public static void GetViewProjectMatrix(Camera currentCam, out Matrix4x4 vp, out Matrix4x4 invVP)
    {
        vp = GL.GetGPUProjectionMatrix(currentCam.projectionMatrix, false) * currentCam.worldToCameraMatrix;
        invVP = vp.inverse;
    }

    public static RenderTexture GetTemporary(RenderTextureDescriptor descriptor, List<RenderTexture> collectList)
    {
        RenderTexture rt = RenderTexture.GetTemporary(descriptor);
        collectList.Add(rt);
        return rt;
    }


    public static RenderTexture GetTemporary(int width, int height, int depth, RenderTextureFormat format, FilterMode filterMode, List<RenderTexture> collectList)
    {
        RenderTexture rt = RenderTexture.GetTemporary(width, height, depth, format, RenderTextureReadWrite.Linear);
        rt.filterMode = filterMode;
        collectList.Add(rt);
        return rt;
    }

    public static RenderTexture GetTemporary(int width, int height, int depth, RenderTextureFormat format, RenderTextureReadWrite readWrite, FilterMode filterMode, List<RenderTexture> collectList)
    {
        RenderTexture rt = RenderTexture.GetTemporary(width, height, depth, format, readWrite);
        rt.filterMode = filterMode;
        collectList.Add(rt);
        return rt;
    }

    public static void ReleaseRenderTarget(List<RenderTexture> tar)
    {
        foreach (var i in tar)
        {
            RenderTexture.ReleaseTemporary(i);
        }
        tar.Clear();
    }

    public static void InsertTo<T>(this List<T> targetArray, T value, Func<T, T, int> compareResult)
    {
        Vector2Int range = new Vector2Int(0, targetArray.Count);
        while (true)
        {
            if (targetArray.Count == 0)
            {
                targetArray.Add(value);
                return;
            }
            else if (Math.Abs(range.x - range.y) == 1)
            {
                int compareX = compareResult(targetArray[range.x], value);
                if (compareX < 0)
                {
                    targetArray.Insert(range.x, value);
                    return;
                }
                else if (compareX > 0)
                {
                    if (range.y < targetArray.Count && compareResult(targetArray[range.y], value) == 0)
                    {
                        return;
                    }
                    else
                    {
                        targetArray.Insert(range.y, value);
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                int currentIndex = (int)((range.x + range.y) / 2f);
                int compare = compareResult(targetArray[currentIndex], value);
                if (compare == 0)
                {
                    return;
                }
                else
                {
                    if (compare < 0)
                    {
                        range.y = currentIndex;
                    }
                    else if (compare > 0)
                    {
                        range.x = currentIndex;
                    }
                }
            }
        }
    }
}
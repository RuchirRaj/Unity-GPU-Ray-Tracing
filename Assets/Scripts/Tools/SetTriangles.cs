using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public unsafe class SetTriangles : MonoBehaviour
{
    public Transform transformParent;
    private ComputeBuffer triangleBuffer;
    private ComputeBuffer objectBuffer;
    void Awake()
    {
        NativeArray<Point> newArr = new NativeArray<Point>(500, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        int count = 0;
        MeshFilter[] filters = transformParent.GetComponentsInChildren<MeshFilter>();
        NativeArray<TargetObject> objInfo = new NativeArray<TargetObject>(filters.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        for (int filtercount = 0; filtercount < filters.Length; ++filtercount)
        {
            var i = filters[filtercount];
            Mesh mesh = i.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            Vector4[] tangents = mesh.tangents;
            Vector2[] uv = mesh.uv;
            int[] triangle = mesh.triangles;
            TargetObject to;
            to.sphere= i.transform.position;
            to.startIndex = (uint)count;
            to.count = (uint)triangle.Length;
            float farestPoint = 0;
            for (int a = 0; a < triangle.Length; ++a)
            {
                int index = triangle[a];
                Point p;
                p.position = vertices[index];
                p.position = i.transform.localToWorldMatrix.MultiplyPoint(p.position);
                farestPoint = Mathf.Max(farestPoint, (p.position - i.transform.position).magnitude);
                p.normal = normals[index];
                p.normal = i.transform.localToWorldMatrix.MultiplyVector(p.normal);
                Vector3 tangent = tangents[index];
                p.tangent = i.transform.localToWorldMatrix.MultiplyVector(tangent);
                p.tangent.w = tangents[index].w;
                p.uv = uv[index];
                p.objIndex = (uint)filtercount;
                int last = count;
                count++;
                Resize(ref newArr, count);
                newArr[last] = p;
            }
            to.sphere.w = farestPoint;
            objInfo[filtercount] = to;
        }
        triangleBuffer = new ComputeBuffer(count, sizeof(Point));
        triangleBuffer.SetData(newArr, 0, 0, count);
        objectBuffer = new ComputeBuffer(filters.Length, sizeof(TargetObject));
        objectBuffer.SetData(objInfo);
        Shader.SetGlobalBuffer("_TriangleBuffer", triangleBuffer);
        Shader.SetGlobalBuffer("_ObjectBuffer", objectBuffer);
        Shader.SetGlobalInt("_ObjectCount", filters.Length);
        newArr.Dispose();
        objInfo.Dispose();
    }

    private void OnDestroy()
    {
        triangleBuffer.Dispose();
        objectBuffer.Dispose();
    }

    static void Resize<T>(ref NativeArray<T> arr, int count) where T : unmanaged
    {
        if (count > arr.Length)
        {
            NativeArray<T> newArr = new NativeArray<T>(arr.Length * 2, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            UnsafeUtility.MemCpy(newArr.GetUnsafePtr(), arr.GetUnsafePtr(), sizeof(T) * arr.Length);
            arr.Dispose();
            arr = newArr;
        }
    }
}

public struct Point
{
    public Vector3 position;
    public Vector3 normal;
    public Vector4 tangent;
    public Vector2 uv;
    public uint objIndex;
}

public struct TargetObject
{
    public Vector4 sphere;
    public uint startIndex;
    public uint count;
}
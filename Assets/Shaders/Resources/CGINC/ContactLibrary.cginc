#pragma target 5.0
struct Point
{
    float3 position;
    float3 normal;
    float4 tangent;
    float2 uv;
    uint objIndex;
};

struct TargetObject
{
    float4 sphere;
    uint startIndex;
    uint count;
};

float IntersectTriangle(float3 orig, float3 dir, float3 v0, float3 v1, float3 v2
        ,inout float t, inout float2 uv)
    {
        float3 e1 = v1 - v0;
        float3 e2 = v2 - v0;
        float3 p = cross(dir, e2);
        float det = dot(e1, p);
        float3 T;
        [flatten]
        if(det > 0)
        {
            T = orig - v0;
        }
        else
        {
            T = v0 - orig;
            det = -det;
        }
        if (det < 0.0001) return 0;
        uv.x = dot(T, p);
        if (uv.x < 0.0f || uv.x > det) return 0;
        float3 Q = cross(T, e1);
        uv.y = dot(dir, Q);
        if (uv.y < 0.0f || (uv.x + uv.y) > det) return 0;
        t = dot(e2, Q);
        float fInvDet = 1 / det;
        t *= fInvDet;
        uv *= fInvDet;
        return 1;
    }

    StructuredBuffer<Point> _TriangleBuffer;
            StructuredBuffer<TargetObject> _ObjectBuffer;
            uint _ObjectCount;
            
            float SphereContact(float3 orig, float3 dir, float4 sphere)
            {
                float3 ToSphere = sphere.xyz - orig;
                float len = length(ToSphere);
                ToSphere /= len;
                float dt = dot(dir, ToSphere);
                float3 contactPoint = orig + dir * len * dt;
                return (sphere.w > len ||
                (dt > 0 && distance(contactPoint, sphere.xyz) < sphere.w)
                ) ? 1 : 0;
            }

            float GetContactPixel(float3 orig, float3 dir, inout Point pt)
            {
                float t = 999999;
                float2 uv = 0;
                for(uint i = 0; i < _ObjectCount; ++i)
                {
                    TargetObject tar = _ObjectBuffer[i];
                    if(SphereContact(orig, dir, tar.sphere) < 0.5) continue;
                    for(uint x = tar.startIndex; x < tar.count + tar.startIndex;){
                    float current_t = 0;
                    float2 current_uv = 0;
                    Point v0 = _TriangleBuffer[x++];
                    Point v1 = _TriangleBuffer[x++];
                    Point v2 = _TriangleBuffer[x++];
                    float result = IntersectTriangle(orig, dir, v0.position,v1.position,v2.position, /*inout*/current_t, /*inout*/current_uv);
                    if(result > 0.5)
                    {
                        if(t > current_t && current_t > 0)
                        {
                            t = current_t;
                            uv = current_uv;
                            float minusUV = 1 - uv.x - uv.y;
                            pt.normal = minusUV * v0.normal + uv.x * v1.normal + uv.y * v2.normal;
                            pt.position = minusUV * v0.position + uv.x * v1.position + uv.y * v2.position;
                            pt.tangent = minusUV * v0.tangent + uv.x * v1.tangent + uv.y * v2.tangent;
                            pt.uv = minusUV * v0.uv + uv.x * v1.uv + uv.y * v2.uv;
                            pt.objIndex = 0;
                        }
                    }
                    }
                }
                return (t < 999998) ? 1 : 0;
            }
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MPipeline
{
    //[CreateAssetMenu]
    public class PipelineResources : ScriptableObject
    {
        public Shader gbufferShader;
        public Shader rayTracingTestShader;
    }
}
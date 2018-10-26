using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MPipeline
{
    [PipelineEvent(false, true)]
    public class DepthEvent : PipelineEvent
    {
        private Material gbufferMaterial;
        public Transform transformParent;
        private static readonly int _CameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");
        protected override void Init(PipelineResources resources)
        {
            gbufferMaterial = new Material(resources.gbufferShader);
        }
        protected override void Dispose()
        {
            Destroy(gbufferMaterial);
        }
        public override void FrameUpdate(PipelineCamera cam, ref PipelineCommandData data)
        {
            Graphics.SetRenderTarget(cam.depthTexture.colorBuffer, cam.renderTarget.depthBuffer);
            GL.Clear(true, true, Color.black);
            Shader.SetGlobalTexture(_CameraDepthTexture, cam.depthTexture);
            int childCount = transformParent.childCount;
            gbufferMaterial.SetPass(0);
            for(int i = 0; i < childCount; ++i)
            {
                Transform tr = transformParent.GetChild(i);
                MeshFilter filter = tr.GetComponent<MeshFilter>();
                Graphics.DrawMeshNow(filter.sharedMesh, tr.localToWorldMatrix);
            }
        }
    }
}
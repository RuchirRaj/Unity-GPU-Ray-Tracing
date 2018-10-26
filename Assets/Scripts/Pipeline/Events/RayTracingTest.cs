using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MPipeline
{
    [PipelineEvent(false, true)]
    public class RayTracingTest : PipelineEvent
    {
        private Material traceMat;
        public Cubemap environmentMap;
        protected override void Init(PipelineResources resources)
        {
            traceMat = new Material(resources.rayTracingTestShader);
        }
        protected override void Dispose()
        {
            Destroy(traceMat);
        }
        bool pressSpace = false;

        private void Update()
        {
            pressSpace = Input.GetKey(KeyCode.Space);
        }

        public override void FrameUpdate(PipelineCamera cam, ref PipelineCommandData data)
        {
            //Press Space To Start Tracing
            if (pressSpace)
            {
                pressSpace = false;
                Matrix4x4 invvp = GL.GetGPUProjectionMatrix(cam.cam.projectionMatrix, true) * cam.cam.worldToCameraMatrix;
                invvp = invvp.inverse;
                traceMat.SetMatrix(ShaderIDs._InvVP, invvp);
                traceMat.SetTexture(ShaderIDs._EnvMap, environmentMap);
                Graphics.SetRenderTarget(cam.renderTarget);
                GL.Clear(true, true, Color.black);
                traceMat.SetPass(0);
                Graphics.DrawMeshNow(GraphicsUtility.mesh, Matrix4x4.identity);
            }
        }
    }
}

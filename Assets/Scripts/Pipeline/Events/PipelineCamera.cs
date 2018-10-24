using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MPipeline
{
    [RequireComponent(typeof(Camera))]
    public class PipelineCamera : MonoBehaviour
    {
        [System.NonSerialized]
        public Camera cam;
        [System.NonSerialized]
        public RenderTexture renderTarget;
        private List<RenderTexture> temporaryTextures = new List<RenderTexture>(15);
        public Dictionary<PipelineEvent, IPerCameraData> postDatas = new Dictionary<PipelineEvent, IPerCameraData>();
        void Awake()
        {
            cam = GetComponent<Camera>();
            cam.renderingPath = RenderingPath.Forward;
            cam.cullingMask = 0;
            cam.clearFlags = CameraClearFlags.Nothing;
        }

        private void OnDisable()
        {
            foreach (var i in postDatas.Values)
            {
                i.DisposeProperty();
            }
            postDatas.Clear();
            if (renderTarget != null)
            {
                renderTarget.Release();
                Destroy(renderTarget);
                renderTarget = null;
            }
        }

        private void SetRenderTarget()
        {
            if(renderTarget != null && (renderTarget.width != cam.pixelWidth || renderTarget.height != cam.pixelHeight))
            {
                renderTarget.Release();
                Destroy(renderTarget);
                renderTarget = null;
            }
            if(renderTarget == null)
            {
                renderTarget = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 24, RenderTextureFormat.ARGBFloat);
                renderTarget.enableRandomWrite = true;
                renderTarget.filterMode = FilterMode.Bilinear;
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (RenderPipeline.singleton)
            {
                SetRenderTarget();
                RenderPipeline.singleton.Render(this, destination);
                PipelineFunctions.ReleaseRenderTarget(temporaryTextures);
            }
            else
            {
                Graphics.Blit(source, destination);
            }
        }
    }
}

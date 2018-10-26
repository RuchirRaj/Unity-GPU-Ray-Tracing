using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MPipeline
{
    [RequireComponent(typeof(Camera))]
    public class PipelineCamera : MonoBehaviour
    {
        public Camera cam { get; private set; }
        public RenderTexture renderTarget { get { return m_renderTarget; } }
        public RenderTexture depthTexture { get { return m_depth; } }
        private RenderTexture m_renderTarget;
        private RenderTexture m_depth;
        public Dictionary<PipelineEvent, IPerCameraData> postDatas { get; private set; }
        void Awake()
        {
            postDatas = new Dictionary<PipelineEvent, IPerCameraData>();
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
                m_renderTarget = null;
            }
        }

        private static void SetRenderTarget(ref RenderTexture renderTarget, Camera cam, int depth, RenderTextureFormat format)
        {
            if (renderTarget != null && (renderTarget.width != cam.pixelWidth || renderTarget.height != cam.pixelHeight))
            {
                renderTarget.Release();
                Destroy(renderTarget);
                renderTarget = null;
            }
            if (renderTarget == null)
            {
                renderTarget = new RenderTexture(cam.pixelWidth, cam.pixelHeight, depth, format);
                renderTarget.enableRandomWrite = true;
                renderTarget.filterMode = FilterMode.Bilinear;
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (RenderPipeline.singleton)
            {
                SetRenderTarget(ref m_renderTarget, cam, 24, RenderTextureFormat.ARGBFloat);
                SetRenderTarget(ref m_depth, cam, 0, RenderTextureFormat.RFloat);
                RenderPipeline.singleton.Render(this, destination);
            }
            else
            {
                Graphics.Blit(source, destination);
            }
        }
    }
}

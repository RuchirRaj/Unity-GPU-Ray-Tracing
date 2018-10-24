using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MPipeline
{
    /// <summary>
    /// 绘制一个蓝底白方块
    /// </summary>
    [PipelineEvent(false, true)]    //不需要对摄像机做矩阵变换之类的操作，不需要pre event，只需要post event
    public class ExampleEvent : PipelineEvent
    {
        private Material unlitMaterial;
        public MeshFilter targetMesh;
        static readonly int _Color = Shader.PropertyToID("_Color");
        protected override void Init(PipelineResources resources)
        {
            //初始化材质
            //利用Scriptable Object统一管理Shader
            //在Assets/Pipeline Resource下统一管理
            unlitMaterial = new Material(resources.unlitTestShader);
            //设置白色
            unlitMaterial.SetColor(_Color, Color.white);
        }

        protected override void Dispose()
        {
            //结束后干掉材质释放内存
            Destroy(unlitMaterial);
        }
        /// <summary>
        /// 主要执行函数，获取当前摄像机信息和通用信息
        /// </summary>
        /// <param name="cam"></param>摄像机信息
        /// <param name="data"></param>通用信息
        public override void FrameUpdate(PipelineCamera cam, ref PipelineCommandData data)
        {
            //设置渲染目标
            Graphics.SetRenderTarget(cam.renderTarget);
            //清空背景+蓝底
            GL.Clear(true, true, Color.blue);
            //Set Pass Call
            unlitMaterial.SetPass(0);
            //传入Mesh和Model Matrix
            Graphics.DrawMeshNow(targetMesh.sharedMesh, targetMesh.transform.localToWorldMatrix);
        }
    }
}

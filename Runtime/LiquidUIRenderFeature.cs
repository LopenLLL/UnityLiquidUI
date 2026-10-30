using System;
using LiquidUI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class LiquidUIRenderFeature : ScriptableRendererFeature
{
    [SerializeField] LiquidUIRenderFeatureSettings settings;
    LiquidUIRenderFeaturePass m_ScriptablePass;
    

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new LiquidUIRenderFeaturePass(settings);

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

        // You can request URP color texture and depth buffer as inputs by uncommenting the line below,
        // URP will ensure copies of these resources are available for sampling before executing the render pass.
        // Only uncomment it if necessary, it will have a performance impact, especially on mobiles and other TBDR GPUs where it will break render passes.
        //m_ScriptablePass.ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth);

        // You can request URP to render to an intermediate texture by uncommenting the line below.
        // Use this option for passes that do not support rendering directly to the backbuffer.
        // Only uncomment it if necessary, it will have a performance impact, especially on mobiles and other TBDR GPUs where it will break render passes.
        //m_ScriptablePass.requiresIntermediateTexture = true;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        
        renderer.EnqueuePass(m_ScriptablePass);
    }

    // Use this class to pass around settings from the feature to the pass
    [Serializable]
    public class LiquidUIRenderFeatureSettings
    {
        public ClampedIntParameter maxIterations;
    }

    class LiquidUIRenderFeaturePass : ScriptableRenderPass
    {
        readonly LiquidUIRenderFeatureSettings settings;
        private BlurMidPyramid _blurMidPyramid;

        public LiquidUIRenderFeaturePass(LiquidUIRenderFeatureSettings settings)
        {
            _blurMidPyramid = new();
            this.settings = settings;
        }

        // This class stores the data needed by the RenderGraph pass.
        // It is passed as a parameter to the delegate function that executes the RenderGraph pass.
        private class PassData
        {
            public BlurMidPyramid blurMidPyramid;
        }

        // This static method is passed as the RenderFunc delegate to the RenderGraph render pass.
        // It is used to execute draw commands.
        static void ExecutePass(PassData data, RasterGraphContext context)
        {
            
        }

        // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
        // FrameData is a context container through which URP resources can be accessed and managed.
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            var sourceTexture = resourceData.cameraColor;
            var sourceDesc = sourceTexture.GetDescriptor(renderGraph);
            
            _blurMidPyramid.Update(sourceDesc,settings.maxIterations.value,renderGraph);
            
            const string passName = "BlurPyramid";
            using (var builder = renderGraph.AddUnsafePass<PassData>(passName, out var passData))
            {
                for (int i = 0; i < _blurMidPyramid.MidCount; i++)
                {
                    builder.UseTexture(_blurMidPyramid.mipUpPyramidTextures[i],AccessFlags.ReadWrite);
                    builder.UseTexture(_blurMidPyramid.mipDownPyramidTextures[i],AccessFlags.ReadWrite);
                }
                
                builder.SetRenderFunc((PassData data, UnsafeGraphContext context) =>
                {
                    var cmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
                    
                });
            }
            
            // This adds a raster render pass to the graph, specifying the name and the data type that will be passed to the ExecutePass function.
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
            {
                passData.blurMidPyramid = _blurMidPyramid;
          
                // Setup pass inputs and outputs through the builder interface.
                // Eg:
                // builder.UseTexture(sourceTexture);
                // TextureDesc colorDescriptor = resourceData.activeColorTexture.GetDescriptor(renderGraph);
                // colorDescriptor.name = "Destination Texture";
                // TextureHandle destination = renderGraph.CreateTexture(colorDescriptor);
                // This sets the render target of the pass to the active color texture. Change it to your own render target as needed.
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);

                // Assigns the ExecutePass function to the render pass delegate. This will be called by the render graph when executing the pass.
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
        }
    }
}

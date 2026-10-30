using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace LiquidUI
{
    public class BlurMidPyramid
    {
        public const int k_MaxPyramidSize = 16;

        public int MidCount { get; private set; }
        
        public TextureHandle[] mipDownPyramidTextures;
        public TextureHandle[] mipUpPyramidTextures;
        
        public string[] _mipDownPyramidNames;
        public string[] _mipUpPyramidNames;
        
        public BlurMidPyramid(int size = k_MaxPyramidSize)
        {
            // Arrays for Bloom pyramid TextureHandle names.
            _mipDownPyramidNames = new string[size];
            _mipUpPyramidNames = new string[size];

            for (int i = 0; i < size; i++)
            {
                _mipDownPyramidNames[i] = "_BloomMipDown" + i;
                _mipUpPyramidNames[i] = "_BloomMipUp" + i;
            }

            // Arrays for Bloom pyramid TextureHandles.
            mipDownPyramidTextures = new TextureHandle[size];
            mipUpPyramidTextures = new TextureHandle[size];
        }

        public void Update(TextureDesc sourceTextureDesc,int maxIteration,RenderGraph renderGraph)
        {
            var startResolution = CalcResolution(sourceTextureDesc, downRes: 1);
            MidCount = CalcMipCount(maxIterations: maxIteration, bloomResolution: startResolution);

            var textureDesc = sourceTextureDesc;
            MakeCompatible(ref textureDesc);

            
            for (int i = 0; i < MidCount; i++)
            {
                ref var upPyramid = ref mipUpPyramidTextures[i];
                ref var downPyramid = ref mipDownPyramidTextures[i];
                
                textureDesc.width = startResolution.x >> (i + 1);
                textureDesc.height = startResolution.y >> (i + 1);

                textureDesc.name = _mipUpPyramidNames[i];
                upPyramid = renderGraph.CreateTexture(textureDesc);
                
                textureDesc.name = _mipDownPyramidNames[i];
                downPyramid = renderGraph.CreateTexture(textureDesc);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int CalcResolution(in TextureDesc bloomSourceDesc,int downRes = 1)
        {

            //We should set the limit the downres result to ensure we dont turn 1x1 textures, which should technically be valid
            //into 0x0 textures which will be invalid
            int tw = Mathf.Max(1, bloomSourceDesc.width >> downRes);
            int th = Mathf.Max(1, bloomSourceDesc.height >> downRes);

            return new Vector2Int(tw, th);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CalcMipCount(int maxIterations, in Vector2Int bloomResolution)
        {
            // Determine the iteration count
            int maxSize = Mathf.Max(bloomResolution.x, bloomResolution.y);
            int iterations = Mathf.FloorToInt(Mathf.Log(maxSize, 2f) - 1);
            int mipCount = Mathf.Clamp(iterations, 1, maxIterations);
            return mipCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MakeCompatible(ref TextureDesc desc)
        {
            desc.msaaSamples = MSAASamples.None;
            desc.useMipMap = false;
            desc.autoGenerateMips = false;
            desc.anisoLevel = 0;
            desc.discardBuffer = false;
        }
    }
}
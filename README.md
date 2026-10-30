# Liquid UI (Unity Package)

Liquid UI 是基于 **Unity 6 / URP 17+ 原生 RenderGraph 架构** 开发的高性能渲染特性与高斯/金字塔模糊方案，专为 **Liquid UI / 毛玻璃 / 液体折射 / 画面后处理** 设计。

---

## 📦 安装方式 (Installation)

### 方式 1：通过 Git URL 安装（推荐）
在 Unity 编辑器中打开：
1. 菜单栏点击 **Window > Package Manager**。
2. 点击左上角的 **`+`** 按钮，选择 **Add package from git URL...**。
3. 输入本仓库的 Git 地址：
   ```text
   https://github.com/<your-username>/<your-repo-name>.git
   ```

### 方式 2：作为本地 Package（Local Package）
1. 在 **Package Manager** 中点击 **`+`** > **Add package from disk...**。
2. 选择本 Package 根目录下的 `package.json` 文件即可。

---

## 🚀 核心特性

1. **原生 RenderGraph 兼容**：
   - 采用 URP 17+ 标准的 `RecordRenderGraph`，无需开启过时的 Compatibility Mode，具备最佳的 GPU 性能与 Pass 合并能力。
2. **捕获摄像机画面 (Screen Capture)**：
   - 自动在指定的渲染管线时机抓取当前摄像机颜色缓冲。
   - 自动暴露为全局贴图属性，任何 UI Shader 或场景 Shader 都可以实时读取。
3. **模糊金字塔支持 (Blur Pyramid)**：
   - 内置 `BlurMidPyramid` 多级分辨率降采样与升采样金字塔管理，高效生成平滑的毛玻璃柔化背景。

---

## 📁 目录结构

```text
Packages/com.liquidui/
├── package.json               // Package 描述文件
├── README.md                  // 使用文档
├── CHANGELOG.md               // 更新日志
├── LICENSE.md                 // 开源协议
├── Runtime/                   // 运行时代码与资产
│   ├── LiquidUI.Runtime.asmdef
│   ├── LiquidUIRenderFeature.cs
│   ├── BlurMidPyramid.cs
│   └── Shaders/
└── Editor/                    // 编辑器相关扩展（可选）
```

---

## 🛠️ 使用指南

### 1. 配置 Universal Renderer Feature
1. 在 Project 窗口找到当前项目使用的 Renderer Data 资源（如 `UniversalRendererData`）。
2. 选中该资源，在 Inspector 底部点击 **Add Renderer Feature**。
3. 选择 **LiquidUIRenderFeature**。
4. 根据需求调整 `maxIterations` 参数控制金字塔迭代层级。

### 2. 在自定义 Shader 中采样
在片元着色器中计算屏幕 UV 并采样抓取的画面：

```hlsl
// 1. 声明贴图和采样器
TEXTURE2D_X(_LiquidUIScreenTexture);
SAMPLER(sampler_LinearClamp);

// 2. 在片元着色器中计算屏幕 UV
float2 screenUV = input.positionNDC.xy / input.positionNDC.w;

// 3. 加上您的液体扰动偏移量 (offset)
float2 distortedUV = screenUV + customDistortionOffset;

// 4. 采样抓取的画面
half4 cameraColor = SAMPLE_TEXTURE2D_X(_LiquidUIScreenTexture, sampler_LinearClamp, distortedUV);
```

---

## 📄 许可协议

本项目基于 [MIT 许可协议](LICENSE.md)。

namespace DxPlanets
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [System.STAThread]
        static void Main()
        {
            System.Windows.Forms.Application.SetHighDpiMode(System.Windows.Forms.HighDpiMode.SystemAware);
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            var form = new Form(600, 400);
            var pipelineSettings = new PipelineSettings
            {
                frameCount = 2,
                width = form.ClientSize.Width,
                height = form.ClientSize.Height,
                formHandle = form.Handle
            };
            var pipeline = LoadPipeline(pipelineSettings);
            var pipelineAssets = LoadAssets(pipeline);
            var viewport = new SharpDX.ViewportF(0, 0, pipelineSettings.width, pipelineSettings.height);
            var scissorRectangle = new SharpDX.Rectangle(0, 0, pipelineSettings.width, pipelineSettings.height);

            form.SizeChanged += (object sender, System.EventArgs e) =>
            {
                WaitForGpu(pipelineAssets);
                pipelineSettings.width = form.ClientSize.Width;
                pipelineSettings.height = form.ClientSize.Height;
                viewport = new SharpDX.ViewportF(0, 0, pipelineSettings.width, pipelineSettings.height);
                scissorRectangle = new SharpDX.Rectangle(0, 0, pipelineSettings.width, pipelineSettings.height);
                foreach (var renderTarget in pipeline.renderTargets)
                {
                    renderTarget.Dispose();
                }
                pipeline.swapChain3.ResizeBuffers(pipelineSettings.frameCount, pipelineSettings.width, pipelineSettings.height, SharpDX.DXGI.Format.R8G8B8A8_UNorm, SharpDX.DXGI.SwapChainFlags.AllowModeSwitch);
                pipeline.frameIndex = pipeline.swapChain3.CurrentBackBufferIndex;
                var rtvHandle = pipeline.renderTargetViewHeap.CPUDescriptorHandleForHeapStart;
                for (int i = 0; i < pipelineSettings.frameCount; i++)
                {
                    pipeline.renderTargets[i] = pipeline.swapChain3.GetBackBuffer<SharpDX.Direct3D12.Resource>(i);
                    pipeline.device.CreateRenderTargetView(pipeline.renderTargets[i], null, rtvHandle);
                    rtvHandle += pipeline.rtvDescriptorSize;
                    pipelineAssets.fenceValues[i] = pipelineAssets.fenceValues[pipeline.frameIndex];
                }
            };

            form.Paint += (object sender, System.Windows.Forms.PaintEventArgs e) =>
            {
                Render(pipelineAssets, viewport, scissorRectangle);
            };

            System.Windows.Forms.Application.Run(form);
        }

        class PipelineSettings
        {
            public int frameCount;
            public int width;
            public int height;
            public System.IntPtr formHandle;
        }

        class Pipeline
        {
            public PipelineSettings pipelineSettings;
            public SharpDX.Direct3D12.Device device;
            public SharpDX.Direct3D12.CommandAllocator[] commandAllocators;
            public SharpDX.Direct3D12.DescriptorHeap renderTargetViewHeap;
            public SharpDX.Direct3D12.Resource[] renderTargets;
            public SharpDX.Direct3D12.CommandQueue commandQueue;
            public SharpDX.DXGI.SwapChain3 swapChain3;
            public int rtvDescriptorSize;
            public int frameIndex;
        }

        class PipelineAssets
        {
            public Pipeline pipeline;
            public SharpDX.Direct3D12.GraphicsCommandList commandList;
            public SharpDX.Direct3D12.RootSignature rootSignature;
            public SharpDX.Direct3D12.Fence fence;
            public System.Threading.AutoResetEvent fenceEvent;
            public SharpDX.Direct3D12.PipelineState pipelineState;
            public SharpDX.Direct3D12.VertexBufferView vertexBufferView;
            public int[] fenceValues;
        }

        struct Vertex
        {
            public SharpDX.Vector3 Position;
            public SharpDX.Vector4 Color;
        };

        static Pipeline LoadPipeline(PipelineSettings pipelineSettings)
        {
            var device = new SharpDX.Direct3D12.Device(null, SharpDX.Direct3D.FeatureLevel.Level_12_1);
            var queueDescription = new SharpDX.Direct3D12.CommandQueueDescription(SharpDX.Direct3D12.CommandListType.Direct);
            var commandQueue = device.CreateCommandQueue(queueDescription);
            var swapChainDescription = new SharpDX.DXGI.SwapChainDescription()
            {
                BufferCount = pipelineSettings.frameCount,
                ModeDescription = new SharpDX.DXGI.ModeDescription(pipelineSettings.width, pipelineSettings.height, new SharpDX.DXGI.Rational(60, 1), SharpDX.DXGI.Format.R8G8B8A8_UNorm),
                Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
                SwapEffect = SharpDX.DXGI.SwapEffect.FlipDiscard,
                OutputHandle = pipelineSettings.formHandle,
                Flags = SharpDX.DXGI.SwapChainFlags.None,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                IsWindowed = true
            };
            var rtvHeapDescription = new SharpDX.Direct3D12.DescriptorHeapDescription()
            {
                DescriptorCount = pipelineSettings.frameCount,
                Flags = SharpDX.Direct3D12.DescriptorHeapFlags.None,
                Type = SharpDX.Direct3D12.DescriptorHeapType.RenderTargetView
            };
            var renderTargetViewHeap = device.CreateDescriptorHeap(rtvHeapDescription);
            var rtvHandle = renderTargetViewHeap.CPUDescriptorHandleForHeapStart;
            var factory = new SharpDX.DXGI.Factory4();
            var swapChain = new SharpDX.DXGI.SwapChain(factory, commandQueue, swapChainDescription);
            var swapChain3 = swapChain.QueryInterface<SharpDX.DXGI.SwapChain3>();
            var frameIndex = swapChain3.CurrentBackBufferIndex;
            var renderTargets = new SharpDX.Direct3D12.Resource[pipelineSettings.frameCount];
            var commandAllocators = new SharpDX.Direct3D12.CommandAllocator[pipelineSettings.frameCount];
            var rtvDescriptorSize = device.GetDescriptorHandleIncrementSize(SharpDX.Direct3D12.DescriptorHeapType.RenderTargetView);
            for (int i = 0; i < pipelineSettings.frameCount; i++)
            {
                renderTargets[i] = swapChain3.GetBackBuffer<SharpDX.Direct3D12.Resource>(i);
                commandAllocators[i] = device.CreateCommandAllocator(SharpDX.Direct3D12.CommandListType.Direct);
                device.CreateRenderTargetView(renderTargets[i], null, rtvHandle);
                rtvHandle += rtvDescriptorSize;
            }
            return new Pipeline
            {
                pipelineSettings = pipelineSettings,
                device = device,
                commandAllocators = commandAllocators,
                renderTargetViewHeap = renderTargetViewHeap,
                renderTargets = renderTargets,
                commandQueue = commandQueue,
                swapChain3 = swapChain3,
                rtvDescriptorSize = rtvDescriptorSize,
                frameIndex = frameIndex
            };
        }

        static PipelineAssets LoadAssets(Pipeline pipeline)
        {
            var rootSignatureDescription = new SharpDX.Direct3D12.RootSignatureDescription(SharpDX.Direct3D12.RootSignatureFlags.AllowInputAssemblerInputLayout);
            var rootSignature = pipeline.device.CreateRootSignature(rootSignatureDescription.Serialize());

            var vertexShader = new SharpDX.Direct3D12.ShaderBytecode(SharpDX.D3DCompiler.ShaderBytecode.CompileFromFile("src/Shaders/FlatColor.hlsl", "VSMain", "vs_5_0"));
            var pixelShader = new SharpDX.Direct3D12.ShaderBytecode(SharpDX.D3DCompiler.ShaderBytecode.CompileFromFile("src/Shaders/FlatColor.hlsl", "PSMain", "ps_5_0"));
            var inputElementDescs = new[]
            {
                    new SharpDX.Direct3D12.InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0),
                    new SharpDX.Direct3D12.InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 12, 0)
            };
            var pipelineStateObjectDescription = new SharpDX.Direct3D12.GraphicsPipelineStateDescription()
            {
                InputLayout = new SharpDX.Direct3D12.InputLayoutDescription(inputElementDescs),
                RootSignature = rootSignature,
                VertexShader = vertexShader,
                PixelShader = pixelShader,
                RasterizerState = SharpDX.Direct3D12.RasterizerStateDescription.Default(),
                BlendState = SharpDX.Direct3D12.BlendStateDescription.Default(),
                DepthStencilFormat = SharpDX.DXGI.Format.D32_Float,
                DepthStencilState = new SharpDX.Direct3D12.DepthStencilStateDescription() { IsDepthEnabled = false, IsStencilEnabled = false },
                SampleMask = int.MaxValue,
                PrimitiveTopologyType = SharpDX.Direct3D12.PrimitiveTopologyType.Triangle,
                RenderTargetCount = 1,
                Flags = SharpDX.Direct3D12.PipelineStateFlags.None,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                StreamOutput = new SharpDX.Direct3D12.StreamOutputDescription()
            };
            pipelineStateObjectDescription.RenderTargetFormats[0] = SharpDX.DXGI.Format.R8G8B8A8_UNorm;
            var pipelineState = pipeline.device.CreateGraphicsPipelineState(pipelineStateObjectDescription);

            var fenceEvent = new System.Threading.AutoResetEvent(false);
            var fence = pipeline.device.CreateFence(0, SharpDX.Direct3D12.FenceFlags.None);
            var fenceValues = new int[pipeline.pipelineSettings.frameCount];
            for (int i = 0; i < pipeline.pipelineSettings.frameCount; i++)
            {
                fenceValues[i] = 1;
            }
            var commandList = pipeline.device.CreateCommandList(SharpDX.Direct3D12.CommandListType.Direct, pipeline.commandAllocators[pipeline.frameIndex], pipelineState);
            commandList.Close();

            var triangleVertices = new[]
            {
                    new Vertex() { Position=new SharpDX.Vector3(0.0f, 0.25f, 0.0f ), Color=new SharpDX.Vector4(1.0f, 0.0f, 0.0f, 1.0f ) },
                    new Vertex() { Position=new SharpDX.Vector3(0.25f, -0.25f, 0.0f), Color=new SharpDX.Vector4(0.0f, 1.0f, 0.0f, 1.0f) },
                    new Vertex() { Position=new SharpDX.Vector3(-0.25f, -0.25f, 0.0f), Color=new SharpDX.Vector4(0.0f, 0.0f, 1.0f, 1.0f ) }
            };
            var vertexBufferSize = SharpDX.Utilities.SizeOf(triangleVertices);
            var vertexBuffer = pipeline.device.CreateCommittedResource(new SharpDX.Direct3D12.HeapProperties(SharpDX.Direct3D12.HeapType.Upload), SharpDX.Direct3D12.HeapFlags.None, SharpDX.Direct3D12.ResourceDescription.Buffer(vertexBufferSize), SharpDX.Direct3D12.ResourceStates.GenericRead);
            var pVertexDataBegin = vertexBuffer.Map(0);
            SharpDX.Utilities.Write(pVertexDataBegin, triangleVertices, 0, triangleVertices.Length);
            vertexBuffer.Unmap(0);
            var vertexBufferView = new SharpDX.Direct3D12.VertexBufferView();
            vertexBufferView.BufferLocation = vertexBuffer.GPUVirtualAddress;
            vertexBufferView.StrideInBytes = SharpDX.Utilities.SizeOf<Vertex>();
            vertexBufferView.SizeInBytes = vertexBufferSize;

            return new PipelineAssets
            {
                pipeline = pipeline,
                commandList = commandList,
                rootSignature = rootSignature,
                fence = fence,
                fenceEvent = fenceEvent,
                pipelineState = pipelineState,
                vertexBufferView = vertexBufferView,
                fenceValues = fenceValues
            };
        }

        static void PopulateCommandList(PipelineAssets pipelineAssets, SharpDX.ViewportF viewport, SharpDX.Rectangle scissorRectangle)
        {
            pipelineAssets.pipeline.commandAllocators[pipelineAssets.pipeline.frameIndex].Reset();
            pipelineAssets.commandList.Reset(pipelineAssets.pipeline.commandAllocators[pipelineAssets.pipeline.frameIndex], pipelineAssets.pipelineState);
            pipelineAssets.commandList.SetGraphicsRootSignature(pipelineAssets.rootSignature);
            pipelineAssets.commandList.SetViewport(viewport);
            pipelineAssets.commandList.SetScissorRectangles(scissorRectangle);
            pipelineAssets.commandList.ResourceBarrierTransition(pipelineAssets.pipeline.renderTargets[pipelineAssets.pipeline.frameIndex], SharpDX.Direct3D12.ResourceStates.Present, SharpDX.Direct3D12.ResourceStates.RenderTarget);

            var rtvHandle = pipelineAssets.pipeline.renderTargetViewHeap.CPUDescriptorHandleForHeapStart + pipelineAssets.pipeline.frameIndex * pipelineAssets.pipeline.rtvDescriptorSize;
            pipelineAssets.commandList.SetRenderTargets(rtvHandle, null);
            pipelineAssets.commandList.ClearRenderTargetView(rtvHandle, new SharpDX.Color4(0, 0.2f, 0.6f, 1), 0, null);
            pipelineAssets.commandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            pipelineAssets.commandList.SetVertexBuffer(0, pipelineAssets.vertexBufferView);
            pipelineAssets.commandList.DrawInstanced(3, 1, 0, 0);

            pipelineAssets.commandList.ResourceBarrierTransition(pipelineAssets.pipeline.renderTargets[pipelineAssets.pipeline.frameIndex], SharpDX.Direct3D12.ResourceStates.RenderTarget, SharpDX.Direct3D12.ResourceStates.Present);
            pipelineAssets.commandList.Close();
        }

        static void Render(PipelineAssets pipelineAssets, SharpDX.ViewportF viewport, SharpDX.Rectangle scissorRectangle)
        {
            PopulateCommandList(pipelineAssets, viewport, scissorRectangle);
            pipelineAssets.pipeline.commandQueue.ExecuteCommandList(pipelineAssets.commandList);
            pipelineAssets.pipeline.swapChain3.Present(1, 0);
            MoveToNextFrame(pipelineAssets);
        }

        static void MoveToNextFrame(PipelineAssets pipelineAssets)
        {
            var currentFenceValue = pipelineAssets.fenceValues[pipelineAssets.pipeline.frameIndex];
            pipelineAssets.pipeline.commandQueue.Signal(pipelineAssets.fence, currentFenceValue);
            pipelineAssets.pipeline.frameIndex = pipelineAssets.pipeline.swapChain3.CurrentBackBufferIndex;
            if (pipelineAssets.fence.CompletedValue < pipelineAssets.fenceValues[pipelineAssets.pipeline.frameIndex])
            {
                pipelineAssets.fence.SetEventOnCompletion(pipelineAssets.fenceValues[pipelineAssets.pipeline.frameIndex], pipelineAssets.fenceEvent.SafeWaitHandle.DangerousGetHandle());
                pipelineAssets.fenceEvent.WaitOne();
            }
            pipelineAssets.fenceValues[pipelineAssets.pipeline.frameIndex] = currentFenceValue + 1;
        }

        static void WaitForGpu(PipelineAssets pipelineAssets)
        {
            pipelineAssets.pipeline.commandQueue.Signal(pipelineAssets.fence, pipelineAssets.fenceValues[pipelineAssets.pipeline.frameIndex]);
            pipelineAssets.fence.SetEventOnCompletion(pipelineAssets.fenceValues[pipelineAssets.pipeline.frameIndex], pipelineAssets.fenceEvent.SafeWaitHandle.DangerousGetHandle());
            pipelineAssets.fenceEvent.WaitOne();
            pipelineAssets.fenceValues[pipelineAssets.pipeline.frameIndex] += 1;
        }
    }
}

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

            var form = new Form(800, 450);
            // form.SizeChanged += (object sender, EventArgs e) =>
            // {
            //     // TODO: handle size change.
            // };

            var viewport = new SharpDX.ViewportF(0, 0, form.Width, form.Height);
            var scissorRectangle = new SharpDX.Rectangle(0, 0, form.Width, form.Height);
            var pipelineSettings = new PipelineSettings
            {
                frameCount = 2,
                width = form.Width,
                height = form.Height,
                formHandle = form.Handle
            };
            var pipelineContext = LoadPipeline(pipelineSettings);
            var renderContext = LoadAssets(pipelineContext);

            form.Shown += (object sender, System.EventArgs e) =>
            {
                while (true)
                {
                    Render(renderContext, viewport, scissorRectangle);
                }
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

        class PipelineContext
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

        class RenderContext
        {
            public PipelineContext pipelineContext;
            public SharpDX.Direct3D12.GraphicsCommandList commandList;
            public SharpDX.Direct3D12.RootSignature rootSignature;
            public SharpDX.Direct3D12.Fence fence;
            public System.Threading.AutoResetEvent fenceEvent;
            public int[] fenceValues;
        }

        static PipelineContext LoadPipeline(PipelineSettings pipelineSettings)
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
                // Flags = SharpDX.DXGI.SwapChainFlags.None,
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
            return new PipelineContext
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

        static RenderContext LoadAssets(PipelineContext pipelineContext)
        {
            var rootSignatureDescription = new SharpDX.Direct3D12.RootSignatureDescription(SharpDX.Direct3D12.RootSignatureFlags.AllowInputAssemblerInputLayout);
            var rootSignature = pipelineContext.device.CreateRootSignature(rootSignatureDescription.Serialize());
            var fenceEvent = new System.Threading.AutoResetEvent(false);
            var fence = pipelineContext.device.CreateFence(0, SharpDX.Direct3D12.FenceFlags.None);
            var fenceValues = new int[pipelineContext.pipelineSettings.frameCount];
            for (int i = 0; i < pipelineContext.pipelineSettings.frameCount; i++)
            {
                fenceValues[i] = 1;
            }
            var commandList = pipelineContext.device.CreateCommandList(SharpDX.Direct3D12.CommandListType.Direct, pipelineContext.commandAllocators[pipelineContext.frameIndex], null);
            commandList.Close();
            return new RenderContext
            {
                pipelineContext = pipelineContext,
                commandList = commandList,
                rootSignature = rootSignature,
                fence = fence,
                fenceEvent = fenceEvent,
                fenceValues = fenceValues
            };
        }

        static void PopulateCommandList(RenderContext renderContext, SharpDX.ViewportF viewport, SharpDX.Rectangle scissorRectangle)
        {
            renderContext.pipelineContext.commandAllocators[renderContext.pipelineContext.frameIndex].Reset();
            renderContext.commandList.Reset(renderContext.pipelineContext.commandAllocators[renderContext.pipelineContext.frameIndex], null);
            renderContext.commandList.SetGraphicsRootSignature(renderContext.rootSignature);
            renderContext.commandList.SetViewport(viewport);
            renderContext.commandList.SetScissorRectangles(scissorRectangle);
            renderContext.commandList.ResourceBarrierTransition(renderContext.pipelineContext.renderTargets[renderContext.pipelineContext.frameIndex], SharpDX.Direct3D12.ResourceStates.Present, SharpDX.Direct3D12.ResourceStates.RenderTarget);
            var rtvHandle = renderContext.pipelineContext.renderTargetViewHeap.CPUDescriptorHandleForHeapStart + renderContext.pipelineContext.frameIndex * renderContext.pipelineContext.rtvDescriptorSize;
            renderContext.commandList.SetRenderTargets(rtvHandle, null);
            renderContext.commandList.ClearRenderTargetView(rtvHandle, renderContext.pipelineContext.frameIndex == 0 ? new SharpDX.Color4(0, 0.2f, 0.6f, 1) : new SharpDX.Color4(0.6f, 0.2f, 0, 1), 0, null);
            renderContext.commandList.ResourceBarrierTransition(renderContext.pipelineContext.renderTargets[renderContext.pipelineContext.frameIndex], SharpDX.Direct3D12.ResourceStates.RenderTarget, SharpDX.Direct3D12.ResourceStates.Present);
            renderContext.commandList.Close();
        }

        static void Render(RenderContext renderContext, SharpDX.ViewportF viewport, SharpDX.Rectangle scissorRectangle)
        {
            PopulateCommandList(renderContext, viewport, scissorRectangle);
            renderContext.pipelineContext.commandQueue.ExecuteCommandList(renderContext.commandList);
            renderContext.pipelineContext.swapChain3.Present(1, 0);
            MoveToNextFrame(renderContext);
        }

        static void MoveToNextFrame(RenderContext renderContext)
        {
            var currentFenceValue = renderContext.fenceValues[renderContext.pipelineContext.frameIndex];
            renderContext.pipelineContext.commandQueue.Signal(renderContext.fence, currentFenceValue);
            renderContext.pipelineContext.frameIndex = renderContext.pipelineContext.swapChain3.CurrentBackBufferIndex;
            if (renderContext.fence.CompletedValue < renderContext.fenceValues[renderContext.pipelineContext.frameIndex])
            {
                renderContext.fence.SetEventOnCompletion(renderContext.fenceValues[renderContext.pipelineContext.frameIndex], renderContext.fenceEvent.SafeWaitHandle.DangerousGetHandle());
                renderContext.fenceEvent.WaitOne();
            }
            renderContext.fenceValues[renderContext.pipelineContext.frameIndex] = currentFenceValue + 1;
        }
    }
}

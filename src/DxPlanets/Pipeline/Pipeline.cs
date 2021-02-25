namespace DxPlanets.Pipeline
{
    class Pipeline
    {
        public int FrameCount { get; private set; }
        public System.Drawing.Size Size { get; private set; }
        public SharpDX.Direct3D12.Device D3D12Device { get; private set; }
        public SharpDX.Direct3D12.CommandAllocator[] CommandAllocators { get; private set; }
        public SharpDX.Direct3D12.DescriptorHeap RenderTargetViewHeap { get; private set; }
        public SharpDX.Direct3D12.Resource[] RenderTargets { get; private set; }
        public SharpDX.Direct3D12.CommandQueue CommandQueue { get; private set; }
        public SharpDX.DXGI.SwapChain3 SwapChain3 { get; private set; }
        public SharpDX.Direct3D12.Fence Fence { get; private set; }
        public System.Threading.AutoResetEvent FenceEvent { get; private set; }
        public SharpDX.Direct3D11.Device D3D11Device { get; private set; }
        public SharpDX.Direct3D11.Device11On12 D3D11On12Device { get; private set; }
        public SharpDX.Direct3D11.Resource[] WrappedBackBuffers { get; private set; }
        public SharpDX.Direct2D1.RenderTarget[] D2DRenderTargets { get; private set; }
        public int[] FenceValues { get; private set; }
        public int RtvDescriptorSize { get; private set; }
        public int FrameIndex { get; private set; }

        public Pipeline(int frameCount, System.Drawing.Size size, System.IntPtr windowHandle)
        {
            // Fields
            FrameCount = frameCount;
            Size = size;

            // Pipeline
            var d3d12Device = new SharpDX.Direct3D12.Device(null, SharpDX.Direct3D.FeatureLevel.Level_12_1);

            var queueDescription = new SharpDX.Direct3D12.CommandQueueDescription(SharpDX.Direct3D12.CommandListType.Direct);
            var commandQueue = d3d12Device.CreateCommandQueue(queueDescription);
            var swapChainDescription = new SharpDX.DXGI.SwapChainDescription()
            {
                BufferCount = frameCount,
                ModeDescription = new SharpDX.DXGI.ModeDescription(Size.Width, Size.Height, new SharpDX.DXGI.Rational(60, 1), SharpDX.DXGI.Format.R8G8B8A8_UNorm),
                Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
                SwapEffect = SharpDX.DXGI.SwapEffect.FlipDiscard,
                OutputHandle = windowHandle,
                Flags = SharpDX.DXGI.SwapChainFlags.None,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                IsWindowed = true
            };
            var rtvHeapDescription = new SharpDX.Direct3D12.DescriptorHeapDescription()
            {
                DescriptorCount = frameCount,
                Flags = SharpDX.Direct3D12.DescriptorHeapFlags.None,
                Type = SharpDX.Direct3D12.DescriptorHeapType.RenderTargetView
            };
            var renderTargetViewHeap = d3d12Device.CreateDescriptorHeap(rtvHeapDescription);
            var rtvHandle = renderTargetViewHeap.CPUDescriptorHandleForHeapStart;
            var dxgiFactory = new SharpDX.DXGI.Factory4();
            var swapChain = new SharpDX.DXGI.SwapChain(dxgiFactory, commandQueue, swapChainDescription);
            var swapChain3 = swapChain.QueryInterface<SharpDX.DXGI.SwapChain3>();
            var frameIndex = swapChain3.CurrentBackBufferIndex;
            var renderTargets = new SharpDX.Direct3D12.Resource[frameCount];
            var commandAllocators = new SharpDX.Direct3D12.CommandAllocator[frameCount];
            var rtvDescriptorSize = d3d12Device.GetDescriptorHandleIncrementSize(SharpDX.Direct3D12.DescriptorHeapType.RenderTargetView);

            var d2d1Factory = new SharpDX.Direct2D1.Factory();
            var d3d11Device = SharpDX.Direct3D11.Device.CreateFromDirect3D12(d3d12Device, SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport, null, null, commandQueue);
            var d3d11On12Device = d3d11Device.QueryInterface<SharpDX.Direct3D11.Device11On12>();
            var wrappedBackBuffers = new SharpDX.Direct3D11.Resource[frameCount];
            var d2dRenderTargets = new SharpDX.Direct2D1.RenderTarget[frameCount];

            for (int i = 0; i < frameCount; i++)
            {
                renderTargets[i] = swapChain3.GetBackBuffer<SharpDX.Direct3D12.Resource>(i);
                commandAllocators[i] = d3d12Device.CreateCommandAllocator(SharpDX.Direct3D12.CommandListType.Direct);
                d3d12Device.CreateRenderTargetView(renderTargets[i], null, rtvHandle);
                rtvHandle += rtvDescriptorSize;

                var format = new SharpDX.Direct3D11.D3D11ResourceFlags()
                {
                    BindFlags = (int)SharpDX.Direct3D11.BindFlags.RenderTarget,
                    CPUAccessFlags = (int)SharpDX.Direct3D11.CpuAccessFlags.None
                };
                d3d11On12Device.CreateWrappedResource(renderTargets[i], format, (int)SharpDX.Direct3D12.ResourceStates.Present, (int)SharpDX.Direct3D12.ResourceStates.RenderTarget, typeof(SharpDX.Direct3D11.Resource).GUID, out wrappedBackBuffers[i]);
                var surface = wrappedBackBuffers[i].QueryInterface<SharpDX.DXGI.Surface>();
                d2dRenderTargets[i] = new SharpDX.Direct2D1.RenderTarget(d2d1Factory, surface, new SharpDX.Direct2D1.RenderTargetProperties(new SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied)));
            }

            // Assets
            var fenceEvent = new System.Threading.AutoResetEvent(false);
            var fence = d3d12Device.CreateFence(0, SharpDX.Direct3D12.FenceFlags.None);
            var fenceValues = new int[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                fenceValues[i] = 1;
            }

            D3D12Device = d3d12Device;
            CommandAllocators = commandAllocators;
            RenderTargetViewHeap = renderTargetViewHeap;
            RenderTargets = renderTargets;
            CommandQueue = commandQueue;
            SwapChain3 = swapChain3;
            Fence = fence;
            FenceEvent = fenceEvent;
            D3D11Device = d3d11Device;
            D3D11On12Device = d3d11On12Device;
            WrappedBackBuffers = wrappedBackBuffers;
            D2DRenderTargets = d2dRenderTargets;
            FenceValues = fenceValues;
            RtvDescriptorSize = rtvDescriptorSize;
            FrameIndex = frameIndex;
        }

        public void Resize(System.Drawing.Size size)
        {
            Size = size;
            foreach (var renderTarget in RenderTargets)
            {
                renderTarget.Dispose();
            }
            SwapChain3.ResizeBuffers(FrameCount, size.Width, size.Height, SharpDX.DXGI.Format.R8G8B8A8_UNorm, SharpDX.DXGI.SwapChainFlags.AllowModeSwitch);
            FrameIndex = SwapChain3.CurrentBackBufferIndex;
            var rtvHandle = RenderTargetViewHeap.CPUDescriptorHandleForHeapStart;
            for (int i = 0; i < FrameCount; i++)
            {
                RenderTargets[i] = SwapChain3.GetBackBuffer<SharpDX.Direct3D12.Resource>(i);
                D3D12Device.CreateRenderTargetView(RenderTargets[i], null, rtvHandle);
                rtvHandle += RtvDescriptorSize;
                FenceValues[i] = FenceValues[FrameIndex];
            }
        }

        public void MoveToNextFrame()
        {
            var currentFenceValue = FenceValues[FrameIndex];
            CommandQueue.Signal(Fence, currentFenceValue);
            FrameIndex = SwapChain3.CurrentBackBufferIndex;
            if (Fence.CompletedValue < FenceValues[FrameIndex])
            {
                Fence.SetEventOnCompletion(FenceValues[FrameIndex], FenceEvent.SafeWaitHandle.DangerousGetHandle());
                FenceEvent.WaitOne();
            }
            FenceValues[FrameIndex] = currentFenceValue + 1;
        }

        public void WaitForGpu()
        {
            CommandQueue.Signal(Fence, FenceValues[FrameIndex]);
            Fence.SetEventOnCompletion(FenceValues[FrameIndex], FenceEvent.SafeWaitHandle.DangerousGetHandle());
            FenceEvent.WaitOne();
            FenceValues[FrameIndex] += 1;
        }
    }
}

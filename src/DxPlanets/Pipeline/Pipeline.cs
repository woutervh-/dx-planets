namespace DxPlanets.Pipeline
{
    class Pipeline
    {
        private int frameCount;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public SharpDX.Direct3D12.Device Device { get; private set; }
        public SharpDX.Direct3D12.CommandAllocator[] CommandAllocators { get; private set; }
        public SharpDX.Direct3D12.DescriptorHeap RenderTargetViewHeap { get; private set; }
        public SharpDX.Direct3D12.Resource[] RenderTargets { get; private set; }
        public SharpDX.Direct3D12.CommandQueue CommandQueue { get; private set; }
        public SharpDX.DXGI.SwapChain3 SwapChain3 { get; private set; }
        public SharpDX.Direct3D12.Fence Fence { get; private set; }
        public System.Threading.AutoResetEvent FenceEvent { get; private set; }
        public int[] FenceValues { get; private set; }
        public int RtvDescriptorSize { get; private set; }
        public int FrameIndex { get; private set; }

        public Pipeline(int frameCount, int width, int height, System.IntPtr formHandle)
        {
            // Fields
            this.frameCount = frameCount;
            Width = width;
            Height = height;

            // Pipeline
            var device = new SharpDX.Direct3D12.Device(null, SharpDX.Direct3D.FeatureLevel.Level_12_1);
            var queueDescription = new SharpDX.Direct3D12.CommandQueueDescription(SharpDX.Direct3D12.CommandListType.Direct);
            var commandQueue = device.CreateCommandQueue(queueDescription);
            var swapChainDescription = new SharpDX.DXGI.SwapChainDescription()
            {
                BufferCount = frameCount,
                ModeDescription = new SharpDX.DXGI.ModeDescription(width, height, new SharpDX.DXGI.Rational(60, 1), SharpDX.DXGI.Format.R8G8B8A8_UNorm),
                Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
                SwapEffect = SharpDX.DXGI.SwapEffect.FlipDiscard,
                OutputHandle = formHandle,
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
            var renderTargetViewHeap = device.CreateDescriptorHeap(rtvHeapDescription);
            var rtvHandle = renderTargetViewHeap.CPUDescriptorHandleForHeapStart;
            var factory = new SharpDX.DXGI.Factory4();
            var swapChain = new SharpDX.DXGI.SwapChain(factory, commandQueue, swapChainDescription);
            var swapChain3 = swapChain.QueryInterface<SharpDX.DXGI.SwapChain3>();
            var frameIndex = swapChain3.CurrentBackBufferIndex;
            var renderTargets = new SharpDX.Direct3D12.Resource[frameCount];
            var commandAllocators = new SharpDX.Direct3D12.CommandAllocator[frameCount];
            var rtvDescriptorSize = device.GetDescriptorHandleIncrementSize(SharpDX.Direct3D12.DescriptorHeapType.RenderTargetView);
            for (int i = 0; i < frameCount; i++)
            {
                renderTargets[i] = swapChain3.GetBackBuffer<SharpDX.Direct3D12.Resource>(i);
                commandAllocators[i] = device.CreateCommandAllocator(SharpDX.Direct3D12.CommandListType.Direct);
                device.CreateRenderTargetView(renderTargets[i], null, rtvHandle);
                rtvHandle += rtvDescriptorSize;
            }

            // Assets
            var fenceEvent = new System.Threading.AutoResetEvent(false);
            var fence = device.CreateFence(0, SharpDX.Direct3D12.FenceFlags.None);
            var fenceValues = new int[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                fenceValues[i] = 1;
            }

            Device = device;
            CommandAllocators = commandAllocators;
            RenderTargetViewHeap = renderTargetViewHeap;
            RenderTargets = renderTargets;
            CommandQueue = commandQueue;
            SwapChain3 = swapChain3;
            Fence = fence;
            FenceEvent = fenceEvent;
            FenceValues = fenceValues;
            RtvDescriptorSize = rtvDescriptorSize;
            FrameIndex = frameIndex;
        }

        public void Resize(int width, int height)
        {
            Width = width;
            Height = height;
            foreach (var renderTarget in RenderTargets)
            {
                renderTarget.Dispose();
            }
            SwapChain3.ResizeBuffers(frameCount, width, height, SharpDX.DXGI.Format.R8G8B8A8_UNorm, SharpDX.DXGI.SwapChainFlags.AllowModeSwitch);
            FrameIndex = SwapChain3.CurrentBackBufferIndex;
            var rtvHandle = RenderTargetViewHeap.CPUDescriptorHandleForHeapStart;
            for (int i = 0; i < frameCount; i++)
            {
                RenderTargets[i] = SwapChain3.GetBackBuffer<SharpDX.Direct3D12.Resource>(i);
                Device.CreateRenderTargetView(RenderTargets[i], null, rtvHandle);
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

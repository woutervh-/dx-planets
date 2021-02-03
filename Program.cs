using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dx_planets
{
    static class Program
    {
        const int width = 800;
        const int height = 450;
        const int frameCount = 2;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var form = new Form(Program.width, Program.height);
            // form.SizeChanged += (object sender, EventArgs e) =>
            // {
            //     // TODO: handle size change.
            // };
            form.Shown += (object sender, EventArgs e) =>
            {
                var device = new SharpDX.Direct3D12.Device(null, SharpDX.Direct3D.FeatureLevel.Level_12_1);

                var queueDescription = new SharpDX.Direct3D12.CommandQueueDescription(SharpDX.Direct3D12.CommandListType.Direct);
                var commandQueue = device.CreateCommandQueue(queueDescription);

                var swapChainDescription = new SharpDX.DXGI.SwapChainDescription()
                {
                    BufferCount = Program.frameCount,
                    ModeDescription = new SharpDX.DXGI.ModeDescription(Program.width, Program.height, new SharpDX.DXGI.Rational(60, 1), SharpDX.DXGI.Format.R8G8B8A8_UNorm),
                    Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
                    SwapEffect = SharpDX.DXGI.SwapEffect.FlipDiscard,
                    OutputHandle = form.Handle,
                    // Flags = SharpDX.DXGI.SwapChainFlags.None,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                    IsWindowed = true
                };

                var rootSignatureDescription = new SharpDX.Direct3D12.RootSignatureDescription(SharpDX.Direct3D12.RootSignatureFlags.AllowInputAssemblerInputLayout);
                var rootSignature = device.CreateRootSignature(rootSignatureDescription.Serialize());

                var rtvHeapDescription = new SharpDX.Direct3D12.DescriptorHeapDescription()
                {
                    DescriptorCount = Program.frameCount,
                    Flags = SharpDX.Direct3D12.DescriptorHeapFlags.None,
                    Type = SharpDX.Direct3D12.DescriptorHeapType.RenderTargetView
                };

                var renderTargetViewHeap = device.CreateDescriptorHeap(rtvHeapDescription);
                var rtvDescriptorSize = device.GetDescriptorHandleIncrementSize(SharpDX.Direct3D12.DescriptorHeapType.RenderTargetView);
                var rtvHandle = renderTargetViewHeap.CPUDescriptorHandleForHeapStart;

                var factory = new SharpDX.DXGI.Factory4();
                var swapChain = new SharpDX.DXGI.SwapChain(factory, commandQueue, swapChainDescription);
                var swapChain3 = swapChain.QueryInterface<SharpDX.DXGI.SwapChain3>();
                var frameIndex = swapChain3.CurrentBackBufferIndex;
                var renderTargets = new SharpDX.Direct3D12.Resource[Program.frameCount];
                var commandAllocators = new SharpDX.Direct3D12.CommandAllocator[Program.frameCount];

                for (int i = 0; i < Program.frameCount; i++)
                {
                    renderTargets[i] = swapChain3.GetBackBuffer<SharpDX.Direct3D12.Resource>(i);
                    commandAllocators[i] = device.CreateCommandAllocator(SharpDX.Direct3D12.CommandListType.Direct);
                    device.CreateRenderTargetView(renderTargets[i], null, rtvHandle);
                    rtvHandle += rtvDescriptorSize;
                }

                var fenceEvent = new System.Threading.AutoResetEvent(false);
                var fence = device.CreateFence(0, SharpDX.Direct3D12.FenceFlags.None);
                var fenceValues = new int[Program.frameCount];
                for (int i = 0; i < Program.frameCount; i++)
                {
                    fenceValues[i] = 1;
                }

                var viewPort = new SharpDX.ViewportF(0, 0, form.Width, form.Height);
                var scissorRectangle = new SharpDX.Rectangle(0, 0, form.Width, form.Height);

                var commandList = device.CreateCommandList(SharpDX.Direct3D12.CommandListType.Direct, commandAllocators[frameIndex], null);
                commandList.Close();

                while (true)
                {
                    rtvHandle = renderTargetViewHeap.CPUDescriptorHandleForHeapStart;
                    rtvHandle += frameIndex * rtvDescriptorSize;

                    commandAllocators[frameIndex].Reset();
                    commandList.Reset(commandAllocators[frameIndex], null);
                    commandList.SetGraphicsRootSignature(rootSignature);
                    commandList.SetViewport(viewPort);
                    commandList.SetScissorRectangles(scissorRectangle);
                    commandList.ResourceBarrierTransition(renderTargets[frameIndex], SharpDX.Direct3D12.ResourceStates.Present, SharpDX.Direct3D12.ResourceStates.RenderTarget);
                    commandList.SetRenderTargets(rtvHandle, null);
                    commandList.ClearRenderTargetView(rtvHandle, frameIndex == 0 ? new SharpDX.Color4(0, 0.2f, 0.6f, 1) : new SharpDX.Color4(0.6f, 0.2f, 0, 1), 0, null);
                    commandList.ResourceBarrierTransition(renderTargets[frameIndex], SharpDX.Direct3D12.ResourceStates.RenderTarget, SharpDX.Direct3D12.ResourceStates.Present);
                    commandList.Close();

                    commandQueue.ExecuteCommandList(commandList);
                    swapChain3.Present(1, 0);

                    var currentFenceValue = fenceValues[frameIndex];
                    commandQueue.Signal(fence, currentFenceValue);
                    frameIndex = swapChain3.CurrentBackBufferIndex;
                    if (fence.CompletedValue < fenceValues[frameIndex])
                    {
                        fence.SetEventOnCompletion(fenceValues[frameIndex], fenceEvent.SafeWaitHandle.DangerousGetHandle());
                        fenceEvent.WaitOne();
                    }
                    fenceValues[frameIndex] = currentFenceValue + 1;
                }
            };

            Application.Run(form);
        }
    }
}

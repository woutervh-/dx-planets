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
            var pipeline = new Pipeline.Pipeline(2, form.GraphicsPanel.ClientSize, form.GraphicsPanel.Handle);
            var pipelineAssets = new Pipeline.PipelineAssets(pipeline);
            var fpsCounter = new FpsCounter();

            fpsCounter.FpsChanged += (object sender, double fps) =>
            {
                form.FpsLabel.Text = fps.ToString("N1") + " fps";
            };
            fpsCounter.Initialize();

            form.GraphicsPanel.SizeChanged += (object sender, System.EventArgs e) =>
            {
                pipeline.WaitForGpu();
                pipeline.Resize(form.GraphicsPanel.ClientSize);
            };

            // System.Windows.Forms.Application.Idle += (object sender, System.EventArgs e) =>
            // {
            //     SharpDX.Win32.NativeMessage msg;
            //     while (PeekMessage(out msg, System.IntPtr.Zero, 0, 0, 0) == 0)
            //     {
            //         Render(pipeline, pipelineAssets);
            //         fpsCounter.OnFrame();
            //         pipeline.MoveToNextFrame();
            //     }
            // };

            form.Show();
            while (form.Created)
            {
                System.Windows.Forms.Application.DoEvents();
                Render(pipeline, pipelineAssets);
                fpsCounter.OnFrame();
                pipeline.MoveToNextFrame();
            }

            // System.Windows.Forms.Application.Run(form);
        }

        static void PopulateCommandList(Pipeline.Pipeline pipeline, Pipeline.PipelineAssets pipelineAssets)
        {
            var viewport = new SharpDX.ViewportF(0, 0, pipeline.Size.Width, pipeline.Size.Height);
            var scissorRectangle = new SharpDX.Rectangle(0, 0, pipeline.Size.Width, pipeline.Size.Height);

            pipeline.CommandAllocators[pipeline.FrameIndex].Reset();
            pipelineAssets.CommandList.Reset(pipeline.CommandAllocators[pipeline.FrameIndex], pipelineAssets.PipelineState);
            pipelineAssets.CommandList.SetGraphicsRootSignature(pipelineAssets.RootSignature);
            pipelineAssets.CommandList.SetViewport(viewport);
            pipelineAssets.CommandList.SetScissorRectangles(scissorRectangle);
            pipelineAssets.CommandList.ResourceBarrierTransition(pipeline.RenderTargets[pipeline.FrameIndex], SharpDX.Direct3D12.ResourceStates.Present, SharpDX.Direct3D12.ResourceStates.RenderTarget);

            var rtvHandle = pipeline.RenderTargetViewHeap.CPUDescriptorHandleForHeapStart + pipeline.FrameIndex * pipeline.RtvDescriptorSize;
            pipelineAssets.CommandList.SetRenderTargets(rtvHandle, null);
            pipelineAssets.CommandList.ClearRenderTargetView(rtvHandle, new SharpDX.Color4(0, 0.2f, 0.6f, 1), 0, null);
            pipelineAssets.CommandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            pipelineAssets.CommandList.SetVertexBuffer(0, pipelineAssets.VertexBufferView);
            pipelineAssets.CommandList.DrawInstanced(3, 1, 0, 0);

            pipelineAssets.CommandList.ResourceBarrierTransition(pipeline.RenderTargets[pipeline.FrameIndex], SharpDX.Direct3D12.ResourceStates.RenderTarget, SharpDX.Direct3D12.ResourceStates.Present);
            pipelineAssets.CommandList.Close();
        }

        static void Render(Pipeline.Pipeline pipeline, Pipeline.PipelineAssets pipelineAssets)
        {
            PopulateCommandList(pipeline, pipelineAssets);
            pipeline.CommandQueue.ExecuteCommandList(pipelineAssets.CommandList);
            pipeline.SwapChain3.Present(1, 0);
            pipeline.MoveToNextFrame();
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "PeekMessage")]
        static extern int PeekMessage(out SharpDX.Win32.NativeMessage lpMsg, System.IntPtr hWnd, int wMsgFilterMin, int wMsgFilterMax, int wRemoveMsg);

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetMessage")]
        static extern int GetMessage(out SharpDX.Win32.NativeMessage lpMsg, System.IntPtr hWnd, int wMsgFilterMin, int wMsgFilterMax);

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "TranslateMessage")]
        static extern int TranslateMessage(ref SharpDX.Win32.NativeMessage lpMsg);

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "DispatchMessage")]
        static extern int DispatchMessage(ref SharpDX.Win32.NativeMessage lpMsg);
    }
}

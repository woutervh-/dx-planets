namespace DxPlanets.Engine
{
    class Engine
    {
        public Settings.GraphicsSettings GraphicsSettings = new Settings.GraphicsSettings();
        public Camera Camera;

        public Engine()
        {
            Camera = new Camera(GraphicsSettings.Projection);
        }

        Pipeline.PipelineAssets.ConstantBuffer constantBufferData = new Pipeline.PipelineAssets.ConstantBuffer()
        {
            viewProjectionMatrix = SharpDX.Matrix.Identity
        };

        void PopulateCommandList(Pipeline.Pipeline pipeline, Pipeline.PipelineAssets pipelineAssets)
        {
            var viewport = new SharpDX.ViewportF(0, 0, pipeline.Size.Width, pipeline.Size.Height);
            var scissorRectangle = new SharpDX.Rectangle(0, 0, pipeline.Size.Width, pipeline.Size.Height);

            pipeline.CommandAllocators[pipeline.FrameIndex].Reset();
            pipelineAssets.CommandList.Reset(pipeline.CommandAllocators[pipeline.FrameIndex], pipelineAssets.PipelineState);

            pipelineAssets.CommandList.SetGraphicsRootSignature(pipelineAssets.RootSignature);
            pipelineAssets.CommandList.SetDescriptorHeaps(1, new SharpDX.Direct3D12.DescriptorHeap[] { pipelineAssets.ConstantBufferViewHeap });
            pipelineAssets.CommandList.SetGraphicsRootDescriptorTable(0, pipelineAssets.ConstantBufferViewHeap.GPUDescriptorHandleForHeapStart);

            pipelineAssets.CommandList.SetViewport(viewport);
            pipelineAssets.CommandList.SetScissorRectangles(scissorRectangle);
            pipelineAssets.CommandList.ResourceBarrierTransition(pipeline.RenderTargets[pipeline.FrameIndex], SharpDX.Direct3D12.ResourceStates.Present, SharpDX.Direct3D12.ResourceStates.RenderTarget);

            var rtvHandle = pipeline.RenderTargetViewHeap.CPUDescriptorHandleForHeapStart + pipeline.FrameIndex * pipeline.RtvDescriptorSize;
            pipelineAssets.CommandList.SetRenderTargets(rtvHandle, null);
            pipelineAssets.CommandList.ClearRenderTargetView(rtvHandle, GraphicsSettings.ClearColor.Value, 0, null);
            pipelineAssets.CommandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            pipelineAssets.CommandList.SetVertexBuffer(0, pipelineAssets.VertexBufferView);
            pipelineAssets.CommandList.DrawInstanced(3, 1, 0, 0);

            pipelineAssets.CommandList.ResourceBarrierTransition(pipeline.RenderTargets[pipeline.FrameIndex], SharpDX.Direct3D12.ResourceStates.RenderTarget, SharpDX.Direct3D12.ResourceStates.Present);
            pipelineAssets.CommandList.Close();
        }

        public void Render(Pipeline.Pipeline pipeline, Pipeline.PipelineAssets pipelineAssets)
        {
            PopulateCommandList(pipeline, pipelineAssets);
            pipeline.CommandQueue.ExecuteCommandList(pipelineAssets.CommandList);

            var brush = new SharpDX.Direct2D1.SolidColorBrush(pipeline.D2DRenderTargets[0], SharpDX.Color4.White);
            pipeline.D3D11On12Device.AcquireWrappedResources(new SharpDX.Direct3D11.Resource[] { pipeline.WrappedBackBuffers[pipeline.FrameIndex] }, 1);
            pipeline.D2DRenderTargets[pipeline.FrameIndex].BeginDraw();
            pipeline.D2DRenderTargets[pipeline.FrameIndex].FillRectangle(new SharpDX.Mathematics.Interop.RawRectangleF(5f, 5f, 100f, 100f), brush);
            // textBrush.Color = Color4.Lerp(colors[t], colors[t + 1], f);
            // pipeline.D2DRenderTargets[pipeline.FrameIndex].DrawText("Hello Text", textFormat, new SharpDX.Mathematics.Interop.RawRectangleF((float)Math.Sin(Environment.TickCount / 1000.0F) * 200 + 400, 10, 2000, 500), textBrush);
            pipeline.D2DRenderTargets[pipeline.FrameIndex].EndDraw();
            pipeline.D3D11On12Device.ReleaseWrappedResources(new SharpDX.Direct3D11.Resource[] { pipeline.WrappedBackBuffers[pipeline.FrameIndex] }, 1);
            pipeline.D3D11Device.ImmediateContext.Flush();
            brush.Dispose();

            pipeline.SwapChain3.Present(1, 0);
            pipeline.MoveToNextFrame();
        }

        public void Update(Pipeline.Pipeline pipeline, Pipeline.PipelineAssets pipelineAssets, System.TimeSpan total, System.TimeSpan delta)
        {
            Camera.Update(total, delta);

            SharpDX.Matrix worldMatrix;
            SharpDX.Matrix viewProjectionMatrix;

            // worldMatrix = SharpDX.Matrix.RotationY((float)total.TotalSeconds);
            worldMatrix = SharpDX.Matrix.Identity;
            viewProjectionMatrix = Camera.GetViewProjection(pipeline.Size);

            constantBufferData.viewProjectionMatrix = worldMatrix * viewProjectionMatrix;
            SharpDX.Utilities.Write(pipelineAssets.ConstantBufferPointer, ref constantBufferData);
        }
    }
}
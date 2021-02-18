namespace DxPlanets.Engine
{
    class Engine
    {
        public Settings.GraphicsSettings GraphicsSettings = new Settings.GraphicsSettings();
        public Camera Camera = new Camera();

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
            pipeline.SwapChain3.Present(1, 0);
            pipeline.MoveToNextFrame();
        }

        public void Update(Pipeline.Pipeline pipeline, Pipeline.PipelineAssets pipelineAssets, System.TimeSpan total, System.TimeSpan delta)
        {
            SharpDX.Matrix worldMatrix;
            SharpDX.Matrix viewMatrix;
            SharpDX.Matrix projectionMatrix;

            // worldMatrix = SharpDX.Matrix.RotationY((float)total.TotalSeconds);
            worldMatrix = SharpDX.Matrix.Identity;
            viewMatrix = SharpDX.Matrix.LookAtLH(Camera.Position.Value, Camera.Position.Value + SharpDX.Vector3.ForwardLH, SharpDX.Vector3.Up);

            if (GraphicsSettings.Projection.Value == Settings.GraphicsSettings.ProjectionSetting.ORTHOGRAPHIC)
            {
                projectionMatrix = SharpDX.Matrix.OrthoOffCenterLH(-1f, 1f, -1f, 1f, 0f, 100f);
            }
            else
            {
                var fov = (float)System.Math.PI / 3f;
                var aspect = (float)pipeline.Size.Width / pipeline.Size.Height;
                projectionMatrix = SharpDX.Matrix.PerspectiveFovLH(fov, aspect, 0f, 100f);
            }

            constantBufferData.viewProjectionMatrix = worldMatrix * viewMatrix * projectionMatrix;
            SharpDX.Utilities.Write(pipelineAssets.ConstantBufferPointer, ref constantBufferData);
        }
    }
}
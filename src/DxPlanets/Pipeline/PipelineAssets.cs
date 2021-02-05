namespace DxPlanets.Pipeline
{
    class PipelineAssets
    {
        public SharpDX.Direct3D12.GraphicsCommandList CommandList { get; private set; }
        public SharpDX.Direct3D12.PipelineState PipelineState { get; private set; }
        public SharpDX.Direct3D12.RootSignature RootSignature { get; private set; }
        public SharpDX.Direct3D12.VertexBufferView VertexBufferView { get; private set; }

        public PipelineAssets(Pipeline pipeline)
        {
            var rootSignatureDescription = new SharpDX.Direct3D12.RootSignatureDescription(SharpDX.Direct3D12.RootSignatureFlags.AllowInputAssemblerInputLayout);
            var rootSignature = pipeline.Device.CreateRootSignature(rootSignatureDescription.Serialize());

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
            var pipelineState = pipeline.Device.CreateGraphicsPipelineState(pipelineStateObjectDescription);

            var commandList = pipeline.Device.CreateCommandList(SharpDX.Direct3D12.CommandListType.Direct, pipeline.CommandAllocators[pipeline.FrameIndex], pipelineState);
            commandList.Close();

            var triangleVertices = new[]
            {
                    new Vertex() { Position=new SharpDX.Vector3(0.0f, 0.25f, 0.0f ), Color=new SharpDX.Vector4(1.0f, 0.0f, 0.0f, 1.0f ) },
                    new Vertex() { Position=new SharpDX.Vector3(0.25f, -0.25f, 0.0f), Color=new SharpDX.Vector4(0.0f, 1.0f, 0.0f, 1.0f) },
                    new Vertex() { Position=new SharpDX.Vector3(-0.25f, -0.25f, 0.0f), Color=new SharpDX.Vector4(0.0f, 0.0f, 1.0f, 1.0f ) }
            };
            var vertexBufferSize = SharpDX.Utilities.SizeOf(triangleVertices);
            var vertexBuffer = pipeline.Device.CreateCommittedResource(new SharpDX.Direct3D12.HeapProperties(SharpDX.Direct3D12.HeapType.Upload), SharpDX.Direct3D12.HeapFlags.None, SharpDX.Direct3D12.ResourceDescription.Buffer(vertexBufferSize), SharpDX.Direct3D12.ResourceStates.GenericRead);
            var pVertexDataBegin = vertexBuffer.Map(0);
            SharpDX.Utilities.Write(pVertexDataBegin, triangleVertices, 0, triangleVertices.Length);
            vertexBuffer.Unmap(0);
            var vertexBufferView = new SharpDX.Direct3D12.VertexBufferView();
            vertexBufferView.BufferLocation = vertexBuffer.GPUVirtualAddress;
            vertexBufferView.StrideInBytes = SharpDX.Utilities.SizeOf<Vertex>();
            vertexBufferView.SizeInBytes = vertexBufferSize;

            CommandList = commandList;
            PipelineState = pipelineState;
            RootSignature = rootSignature;
            VertexBufferView = vertexBufferView;
        }

        struct Vertex
        {
            public SharpDX.Vector3 Position;
            public SharpDX.Vector4 Color;
        };
    }
}
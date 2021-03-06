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

            var form = new Form(800, 600);
            var pipeline = new Pipeline.Pipeline(2, form.GraphicsPanel.ClientSize, form.GraphicsPanel.Handle);
            var pipelineAssets = new Pipeline.PipelineAssets(pipeline);
            var fpsCounter = new FpsCounter();
            var engine = new Engine.Engine();
            var bridge = new UI.Bridge(engine, fpsCounter);

            form.GraphicsPanel.KeyDown += (object sender, System.Windows.Forms.KeyEventArgs e) =>
            {
                switch (e.KeyData)
                {
                    case System.Windows.Forms.Keys.W:
                        engine.Camera.IsMovingForward = true;
                        break;
                    case System.Windows.Forms.Keys.A:
                        engine.Camera.IsMovingLeft = true;
                        break;
                    case System.Windows.Forms.Keys.S:
                        engine.Camera.IsMovingBackward = true;
                        break;
                    case System.Windows.Forms.Keys.D:
                        engine.Camera.IsMovingRight = true;
                        break;
                }
            };

            form.GraphicsPanel.KeyUp += (object sender, System.Windows.Forms.KeyEventArgs e) =>
            {
                switch (e.KeyData)
                {
                    case System.Windows.Forms.Keys.W:
                        engine.Camera.IsMovingForward = false;
                        break;
                    case System.Windows.Forms.Keys.A:
                        engine.Camera.IsMovingLeft = false;
                        break;
                    case System.Windows.Forms.Keys.S:
                        engine.Camera.IsMovingBackward = false;
                        break;
                    case System.Windows.Forms.Keys.D:
                        engine.Camera.IsMovingRight = false;
                        break;
                }
            };

            form.GraphicsPanel.MouseMove += (object sender, System.Windows.Forms.MouseEventArgs e) =>
            {
            };

            form.WebView.CoreWebView2InitializationCompleted += (object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e) =>
            {
                bridge.SetCoreWebView2(form.WebView.CoreWebView2);
            };

            form.GraphicsPanel.SizeChanged += (object sender, System.EventArgs e) =>
            {
                pipeline.WaitForGpu();
                pipeline.Resize(form.GraphicsPanel.ClientSize);
            };

            form.Show();
            fpsCounter.Initialize();
            var start = System.Diagnostics.Stopwatch.GetTimestamp();
            var last = start;
            while (form.Created)
            {
                fpsCounter.OnFrame();

                var now = System.Diagnostics.Stopwatch.GetTimestamp();
                var total = System.TimeSpan.FromTicks(now - start);
                var delta = System.TimeSpan.FromTicks(now - last);
                last = now;

                engine.Update(pipeline, pipelineAssets, total, delta);
                engine.Render(pipeline, pipelineAssets);
                pipeline.MoveToNextFrame();

                System.Windows.Forms.Application.DoEvents();
            }
        }
    }
}

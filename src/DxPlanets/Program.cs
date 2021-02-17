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
            var bridge = new UI.Bridge();

            form.WebView.CoreWebView2InitializationCompleted += (object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e) =>
            {
                bridge.SetCoreWebView2(form.WebView.CoreWebView2);
            };

            fpsCounter.FpsChanged += (object sender, double fps) =>
            {
                bridge.State.Fps = fps;
            };

            form.GraphicsPanel.SizeChanged += (object sender, System.EventArgs e) =>
            {
                pipeline.WaitForGpu();
                pipeline.Resize(form.GraphicsPanel.ClientSize);
            };

            form.Show();
            fpsCounter.Initialize();
            var game = new Game() { State = bridge.State };
            var start = System.Diagnostics.Stopwatch.GetTimestamp();
            var last = start;
            while (form.Created)
            {
                fpsCounter.OnFrame();

                var now = System.Diagnostics.Stopwatch.GetTimestamp();
                var total = System.TimeSpan.FromTicks(now - start);
                var delta = System.TimeSpan.FromTicks(now - last);
                last = now;

                game.Update(pipeline, pipelineAssets, total, delta);
                game.Render(pipeline, pipelineAssets);
                pipeline.MoveToNextFrame();

                System.Windows.Forms.Application.DoEvents();
            }
        }
    }
}

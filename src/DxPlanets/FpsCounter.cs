namespace DxPlanets
{
    class FpsCounter
    {
        private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        public event FpsEventHandler FpsChanged;
        private int frameCount = 0;

        public delegate void FpsEventHandler(object sender, double fps);

        public void Initialize()
        {
            if (FpsChanged != null)
            {
                FpsChanged(null, 0.0);
            }
            stopwatch.Start();
        }

        public void OnFrame()
        {
            frameCount += 1;
            var elapsedTime = stopwatch.ElapsedTicks / System.Diagnostics.Stopwatch.Frequency;
            if (elapsedTime >= 1f)
            {
                var fps = (double)frameCount / elapsedTime;
                if (FpsChanged != null)
                {
                    FpsChanged(this, fps);
                }
                frameCount = 0;
                stopwatch.Restart();
            }
        }
    }
}

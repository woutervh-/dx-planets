namespace DxPlanets
{
    class FpsCounter
    {
        private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        private int frameCount = 0;

        public System.Reactive.Subjects.BehaviorSubject<double> Fps = new System.Reactive.Subjects.BehaviorSubject<double>(0.0);

        public void Initialize()
        {
            stopwatch.Start();
        }

        public void OnFrame()
        {
            frameCount += 1;
            var elapsedTime = stopwatch.ElapsedTicks / System.Diagnostics.Stopwatch.Frequency;
            if (elapsedTime >= 1f)
            {
                var fps = (double)frameCount / elapsedTime;
                Fps.OnNext(fps);
                frameCount = 0;
                stopwatch.Restart();
            }
        }
    }
}

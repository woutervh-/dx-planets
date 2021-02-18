namespace DxPlanets.UI
{
    class State
    {
        public System.Reactive.Subjects.BehaviorSubject<Game.Settings.GraphicsSettings.ProjectionSetting> Projection;
        public System.Reactive.Subjects.BehaviorSubject<double> Fps;
        public System.Reactive.Subjects.BehaviorSubject<SharpDX.Color4> ClearColor;
    }
}

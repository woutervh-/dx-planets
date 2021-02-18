namespace DxPlanets.UI
{
    class State
    {
        public System.Reactive.Subjects.BehaviorSubject<Game.Settings.GraphicsSettings.ProjectionSetting> Projection;
        public System.Reactive.Subjects.BehaviorSubject<double> Fps;
    }
}

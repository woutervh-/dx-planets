namespace DxPlanets.Game.Settings
{
    class GraphicsSettings
    {
        public enum ProjectionSetting
        {
            ORTHOGRAPHIC,
            PERSPECTIVE
        }

        public System.Reactive.Subjects.BehaviorSubject<ProjectionSetting> Projection = new System.Reactive.Subjects.BehaviorSubject<ProjectionSetting>(ProjectionSetting.ORTHOGRAPHIC);
        public System.Reactive.Subjects.BehaviorSubject<SharpDX.Color4> ClearColor = new System.Reactive.Subjects.BehaviorSubject<SharpDX.Color4>(new SharpDX.Color4(0, 0.2f, 0.6f, 1));
    }
}
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
    }
}
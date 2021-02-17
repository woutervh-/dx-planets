namespace DxPlanets.UI
{
    class UIState
    {
        private double fps = double.NaN;
        public double Fps { get { return fps; } set { fps = value; FpsChanged?.Invoke(this, System.EventArgs.Empty); } }
        public event System.EventHandler FpsChanged;

        private ProjectionType projection = ProjectionType.Orthographic;
        public ProjectionType Projection { get { return projection; } set { projection = value; ProjectionChanged?.Invoke(this, System.EventArgs.Empty); } }
        public event System.EventHandler ProjectionChanged;

        public class ProjectionType
        {
            public string Value { get; private set; }

            private ProjectionType(string value)
            {
                this.Value = value;
            }

            public static ProjectionType Orthographic = new ProjectionType("orthographic");
            public static ProjectionType Perspective = new ProjectionType("perspective");
        }
    }
}

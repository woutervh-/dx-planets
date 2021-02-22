namespace DxPlanets.Engine
{
    class Camera
    {
        public System.Reactive.Subjects.BehaviorSubject<SharpDX.Vector3> Position = new System.Reactive.Subjects.BehaviorSubject<SharpDX.Vector3>(new SharpDX.Vector3(0f, 0f, -5f));
        public System.Reactive.Subjects.BehaviorSubject<bool> IsMovingForward = new System.Reactive.Subjects.BehaviorSubject<bool>(false);
        public System.Reactive.Subjects.BehaviorSubject<bool> IsMovingBackward = new System.Reactive.Subjects.BehaviorSubject<bool>(false);
        public System.Reactive.Subjects.BehaviorSubject<bool> IsMovingLeft = new System.Reactive.Subjects.BehaviorSubject<bool>(false);
        public System.Reactive.Subjects.BehaviorSubject<bool> IsMovingRight = new System.Reactive.Subjects.BehaviorSubject<bool>(false);
        public System.Reactive.Subjects.BehaviorSubject<SharpDX.Matrix> ViewProjection;

        private static SharpDX.Matrix GetViewProjection(SharpDX.Vector3 position, Settings.GraphicsSettings.ProjectionSetting projection)
        {

        }

        public Camera(System.Reactive.Subjects.BehaviorSubject<Settings.GraphicsSettings.ProjectionSetting> projection)
        {
            ViewProjection = System.Reactive.Linq.CombineLatest<SharpDX.Vector3, Settings.GraphicsSettings.ProjectionSetting, SharpDX.Matrix>(Position, projection, GetViewProjection);
        }

        public void MoveForward(float amount)
        {
            Position.OnNext(Position.Value + SharpDX.Vector3.ForwardLH * amount);
        }

        public void MoveBackward(float amount)
        {
            Position.OnNext(Position.Value + SharpDX.Vector3.BackwardLH * amount);
        }

        public void MoveLeft(float amount)
        {
            Position.OnNext(Position.Value + SharpDX.Vector3.Left * amount);
        }

        public void MoveRight(float amount)
        {
            Position.OnNext(Position.Value + SharpDX.Vector3.Right * amount);
        }

        public void Update(System.TimeSpan total, System.TimeSpan delta)
        {
            if (IsMovingForward.Value)
            {
                MoveForward((float)delta.TotalSeconds);
            }
            if (IsMovingBackward.Value)
            {
                MoveBackward((float)delta.TotalSeconds);
            }
            if (IsMovingLeft.Value)
            {
                MoveLeft((float)delta.TotalSeconds);
            }
            if (IsMovingRight.Value)
            {
                MoveRight((float)delta.TotalSeconds);
            }
        }
    }
}
namespace DxPlanets.Engine
{
    class Camera
    {
        public bool IsMovingForward = false;
        public bool IsMovingBackward = false;
        public bool IsMovingLeft = false;
        public bool IsMovingRight = false;

        private SharpDX.Vector3 position = new SharpDX.Vector3(0f, 0f, -5f);
        private System.Reactive.Subjects.BehaviorSubject<Settings.GraphicsSettings.ProjectionSetting> projection;

        public Camera(System.Reactive.Subjects.BehaviorSubject<Settings.GraphicsSettings.ProjectionSetting> projection)
        {
            this.projection = projection;
        }

        public SharpDX.Matrix GetViewProjection(System.Drawing.Size size)
        {
            SharpDX.Matrix viewMatrix = SharpDX.Matrix.LookAtLH(position, position + SharpDX.Vector3.ForwardLH, SharpDX.Vector3.Up);
            SharpDX.Matrix projectionMatrix;

            if (projection.Value == Settings.GraphicsSettings.ProjectionSetting.ORTHOGRAPHIC)
            {
                projectionMatrix = SharpDX.Matrix.OrthoOffCenterLH(-1f, 1f, -1f, 1f, 0f, 100f);
            }
            else
            {
                var fov = (float)System.Math.PI / 3f;
                var aspect = (float)size.Width / size.Height;
                projectionMatrix = SharpDX.Matrix.PerspectiveFovLH(fov, aspect, 0f, 100f);
            }

            return viewMatrix * projectionMatrix;
        }

        public void MoveForward(float amount)
        {
            position += SharpDX.Vector3.ForwardLH * amount;
        }

        public void MoveBackward(float amount)
        {
            position += SharpDX.Vector3.BackwardLH * amount;
        }

        public void MoveLeft(float amount)
        {
            position += SharpDX.Vector3.Left * amount;
        }

        public void MoveRight(float amount)
        {
            position += SharpDX.Vector3.Right * amount;
        }

        public void Update(System.TimeSpan total, System.TimeSpan delta)
        {
            if (IsMovingForward)
            {
                MoveForward((float)delta.TotalSeconds);
            }
            if (IsMovingBackward)
            {
                MoveBackward((float)delta.TotalSeconds);
            }
            if (IsMovingLeft)
            {
                MoveLeft((float)delta.TotalSeconds);
            }
            if (IsMovingRight)
            {
                MoveRight((float)delta.TotalSeconds);
            }
        }
    }
}
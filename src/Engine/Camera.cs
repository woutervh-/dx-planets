namespace DxPlanets.Engine
{
    class Camera
    {
        public System.Reactive.Subjects.BehaviorSubject<SharpDX.Vector3> Position = new System.Reactive.Subjects.BehaviorSubject<SharpDX.Vector3>(new SharpDX.Vector3(0f, 0f, -5f));

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
    }
}
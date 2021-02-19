namespace DxPlanets.Engine
{
    class Camera
    {
        public System.Reactive.Subjects.BehaviorSubject<SharpDX.Vector3> Position = new System.Reactive.Subjects.BehaviorSubject<SharpDX.Vector3>(new SharpDX.Vector3(0f, 0f, -5f));
        public System.Reactive.Subjects.BehaviorSubject<bool> IsMovingForward = new System.Reactive.Subjects.BehaviorSubject<bool>(false);
        public System.Reactive.Subjects.BehaviorSubject<bool> IsMovingBackward = new System.Reactive.Subjects.BehaviorSubject<bool>(false);
        public System.Reactive.Subjects.BehaviorSubject<bool> IsMovingLeft = new System.Reactive.Subjects.BehaviorSubject<bool>(false);
        public System.Reactive.Subjects.BehaviorSubject<bool> IsMovingRight = new System.Reactive.Subjects.BehaviorSubject<bool>(false);

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
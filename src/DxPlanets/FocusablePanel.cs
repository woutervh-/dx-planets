namespace DxPlanets
{
    class FocusablePanel : System.Windows.Forms.Panel
    {
        public FocusablePanel()
        {
            this.SetStyle(System.Windows.Forms.ControlStyles.Selectable, true);
            this.TabStop = true;
        }

        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            this.Focus();
            base.OnMouseDown(e);
        }

        protected override void OnPreviewKeyDown(System.Windows.Forms.PreviewKeyDownEventArgs e)
        {
            switch (e.KeyData)
            {
                case System.Windows.Forms.Keys.Up:
                case System.Windows.Forms.Keys.Down:
                case System.Windows.Forms.Keys.Left:
                case System.Windows.Forms.Keys.Right:
                    e.IsInputKey = true;
                    break;
            }
        }

        protected override bool IsInputKey(System.Windows.Forms.Keys keyData)
        {
            switch (keyData)
            {
                case System.Windows.Forms.Keys.Up:
                case System.Windows.Forms.Keys.Down:
                case System.Windows.Forms.Keys.Left:
                case System.Windows.Forms.Keys.Right:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        protected override void OnEnter(System.EventArgs e)
        {
            this.Invalidate();
            base.OnEnter(e);
        }

        protected override void OnLeave(System.EventArgs e)
        {
            this.Invalidate();
            base.OnLeave(e);
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            base.OnPaint(e);
            if (this.Focused)
            {
                var rectangle = this.ClientRectangle;
                rectangle.Inflate(-2, -2);
                System.Windows.Forms.ControlPaint.DrawFocusRectangle(e.Graphics, rectangle);
            }
        }
    }
}
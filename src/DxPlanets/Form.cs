namespace DxPlanets
{
    public partial class Form : System.Windows.Forms.Form
    {
        private System.ComponentModel.IContainer components = null;
        public System.Windows.Forms.Panel GraphicsPanel { get; private set; }

        public Form(int width, int height)
        {
            components = new System.ComponentModel.Container();
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(width, height);
            Text = "Form1";

            GraphicsPanel = new System.Windows.Forms.Panel();
            GraphicsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            GraphicsPanel.BackColor = System.Drawing.Color.BlueViolet;
            Controls.Add(GraphicsPanel);

            var uiPanel = new System.Windows.Forms.Panel();
            uiPanel.Dock = System.Windows.Forms.DockStyle.Right;
            uiPanel.Width = 240;
            uiPanel.BackColor = System.Drawing.Color.Brown;
            Controls.Add(uiPanel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            for (int i = 0; i < Controls.Count; i++)
            {
                Controls[i].Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

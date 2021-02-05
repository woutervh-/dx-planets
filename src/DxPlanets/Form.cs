namespace DxPlanets
{
    public partial class Form : System.Windows.Forms.Form
    {
        private System.ComponentModel.IContainer components = null;

        public Form(int width, int height)
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(width, height);
            this.Text = "Form1";

            // TODO: make panels for UI and render
            var panel = new System.Windows.Forms.Panel();
            panel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            panel.Width = 100;
            panel.BackColor = System.Drawing.Color.Brown;
            this.Controls.Add(panel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

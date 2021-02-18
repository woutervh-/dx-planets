namespace DxPlanets
{
    partial class Form : System.Windows.Forms.Form
    {
        private System.ComponentModel.IContainer components = null;
        public Microsoft.Web.WebView2.WinForms.WebView2 WebView { get; private set; }
        public System.Windows.Forms.Panel GraphicsPanel { get; private set; }

        public Form(int width, int height)
        {
            components = new System.ComponentModel.Container();
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(width, height);
            Text = "Form1";

            GraphicsPanel = new FocusablePanel();
            GraphicsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            GraphicsPanel.BackColor = System.Drawing.Color.Black;
            GraphicsPanel.KeyDown += (object sender, System.Windows.Forms.KeyEventArgs e) =>
            {
                System.Diagnostics.Trace.WriteLine(e);
            };
            Controls.Add(GraphicsPanel);

            var uiPanel = new System.Windows.Forms.TableLayoutPanel();
            uiPanel.Dock = System.Windows.Forms.DockStyle.Right;
            uiPanel.Width = 240;
            Controls.Add(uiPanel);

            // Requires Edge (Chromium):
            // https://www.microsoftedgeinsider.com/en-us/download
            WebView = new Microsoft.Web.WebView2.WinForms.WebView2();
            WebView.Location = System.Drawing.Point.Empty;
            uiPanel.Controls.Add(WebView);

            WebView.Margin = WebView.Padding = System.Windows.Forms.Padding.Empty;
            WebView.Size = uiPanel.Size;
            uiPanel.Resize += (object sender, System.EventArgs e) =>
            {
                WebView.Size = uiPanel.Size;
            };
            InitializeAsync(WebView);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        async void InitializeAsync(Microsoft.Web.WebView2.WinForms.WebView2 webView)
        {
            var localAppData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            var cacheFolder = System.IO.Path.Combine(localAppData, "WindowsFormsWebView2");
            var environment = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(userDataFolder: cacheFolder);
            await webView.EnsureCoreWebView2Async(environment);
            webView.CoreWebView2.Navigate(System.String.Format("file:///{0}/resources/index.html", System.IO.Directory.GetCurrentDirectory()));
        }
    }
}

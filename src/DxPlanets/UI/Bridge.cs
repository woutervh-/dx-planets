namespace DxPlanets.UI
{
    class Bridge
    {
        public UIState State { get; private set; } = new UIState();
        private Microsoft.Web.WebView2.Core.CoreWebView2 coreWebView2 = null;

        public void SetCoreWebView2(Microsoft.Web.WebView2.Core.CoreWebView2 coreWebView2)
        {
            this.coreWebView2 = coreWebView2;
            coreWebView2.DOMContentLoaded += (object sender, Microsoft.Web.WebView2.Core.CoreWebView2DOMContentLoadedEventArgs e) =>
            {
                synchronizeFullState();
            };
            registerOutgoingEvents();
            registerIncomingEvents();
        }

        private void registerOutgoingEvents()
        {
            State.FpsChanged += (object sender, System.EventArgs e) =>
            {
                sendState("fps", State.Fps);
            };
            State.ProjectionChanged += (object sender, System.EventArgs e) =>
            {
                sendState("projection", State.Projection.Value);
            };
        }

        private void registerIncomingEvents()
        {
            if (coreWebView2 == null)
            {
                return;
            }

            coreWebView2.WebMessageReceived += (object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e) =>
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(e.WebMessageAsJson);
                var document = System.Text.Json.JsonDocument.Parse(bytes);

                var projection = document.RootElement.GetProperty("projection").GetString();
                if (projection == "orthographic")
                {
                    State.Projection = UIState.ProjectionType.Orthographic;
                }
                else if (projection == "perspective")
                {
                    State.Projection = UIState.ProjectionType.Perspective;
                }
            };
        }

        private void synchronizeFullState()
        {
            var fullState = new System.Collections.Generic.Dictionary<string, object>
            {
                { "fps", State.Fps },
                { "projection", State.Projection.Value }
            };
            sendState(fullState);
        }

        private void sendState(string propertyName)
        {
            sendState(new System.Collections.Generic.Dictionary<string, object> { { propertyName, null } });
        }

        private void sendState(string propertyName, double value)
        {
            sendState(new System.Collections.Generic.Dictionary<string, object> { { propertyName, value } });
        }

        private void sendState(string propertyName, string value)
        {
            sendState(new System.Collections.Generic.Dictionary<string, object> { { propertyName, value } });
        }

        private void sendState(System.Collections.Generic.Dictionary<string, object> properties)
        {
            if (coreWebView2 == null)
            {
                return;
            }

            using (var stream = new System.IO.MemoryStream())
            {
                using (var writer = new System.Text.Json.Utf8JsonWriter(stream))
                {
                    writer.WriteStartObject();
                    writer.WriteString("type", "state");
                    writer.WriteStartObject("state");

                    foreach (var pair in properties)
                    {
                        if (pair.Value == null)
                        {
                            writer.WriteNull(pair.Key);
                        }
                        else if (pair.Value is double)
                        {
                            writer.WriteNumber(pair.Key, (double)pair.Value);
                        }
                        else if (pair.Value is string)
                        {
                            writer.WriteString(pair.Key, (string)pair.Value);
                        }
                        else
                        {
                            throw new System.ArgumentException("Unsupported type for property '" + pair.Key + "'.");
                        }
                    }

                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }

                string json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
                coreWebView2.PostWebMessageAsJson(json);
            }
        }
    }
}

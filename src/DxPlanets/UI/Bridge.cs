namespace DxPlanets.UI
{
    class Bridge
    {
        public State State { get; private set; }
        private Microsoft.Web.WebView2.Core.CoreWebView2 coreWebView2 = null;

        public Bridge(Game.Game game, FpsCounter fpsCounter)
        {
            State = new State
            {
                Projection = game.GraphicsSettings.Projection,
                Fps = fpsCounter.Fps,
                ClearColor = game.GraphicsSettings.ClearColor
            };
        }

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
            System.ObservableExtensions.Subscribe(State.Fps, (fps) => sendState("fps", fps));
            System.ObservableExtensions.Subscribe(State.Projection, (projection) => sendState("projection", BridgeHelper.ProjectionToString(projection)));
            System.ObservableExtensions.Subscribe(State.ClearColor, (clearColor) => sendState("clearColor", BridgeHelper.ColorToString(clearColor)));
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

                System.Text.Json.JsonElement projection;
                if (document.RootElement.TryGetProperty("projection", out projection))
                {
                    State.Projection.OnNext(BridgeHelper.ProjectionFromString(projection.GetString()));
                }

                System.Text.Json.JsonElement clearColor;
                if (document.RootElement.TryGetProperty("clearColor", out clearColor))
                {
                    State.ClearColor.OnNext(BridgeHelper.ColorFromString(clearColor.GetString()));
                }
            };
        }

        private void synchronizeFullState()
        {
            var fullState = new System.Collections.Generic.Dictionary<string, object>
            {
                { "fps", State.Fps.Value },
                { "projection", BridgeHelper.ProjectionToString(State.Projection.Value) },
                { "clearColor", BridgeHelper.ColorToString(State.ClearColor.Value) }
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

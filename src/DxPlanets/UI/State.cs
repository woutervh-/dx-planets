namespace DxPlanets.UI
{
    class State
    {
        // TODO: some things are one way, some things are two way... find out a nice bridge system...
        public double Fps;

        public string ToJson()
        {
            using (var stream = new System.IO.MemoryStream())
            {
                using (var writer = new System.Text.Json.Utf8JsonWriter(stream))
                {
                    writer.WriteStartObject();
                    writer.WriteNumber("fps", Fps);
                    writer.WriteEndObject();
                }

                return System.Text.Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        private void readFps(System.Text.Json.Utf8JsonReader reader)
        {
            reader.GetDouble
        }

        public void ReadJson(string json)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var reader = new System.Text.Json.Utf8JsonReader(bytes);
            string propertyName = null;
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case System.Text.Json.JsonTokenType.PropertyName:
                        propertyName = reader.GetString();
                        break;
                    case System.Text.Json.JsonTokenType.Null:
                    case System.Text.Json.JsonTokenType.True:
                    case System.Text.Json.JsonTokenType.False:
                    case System.Text.Json.JsonTokenType.Number:
                    case System.Text.Json.JsonTokenType.String:
                }
            }
        }
    }
}

namespace DxPlanets.UI
{
    static class BridgeHelper
    {
        public static string ProjectionToString(Engine.Settings.GraphicsSettings.ProjectionSetting projection)
        {
            switch (projection)
            {
                case Engine.Settings.GraphicsSettings.ProjectionSetting.ORTHOGRAPHIC:
                    return "orthographic";
                case Engine.Settings.GraphicsSettings.ProjectionSetting.PERSPECTIVE:
                    return "perspective";
            }
            throw new System.ArgumentException("Invalid projection.");
        }

        public static Engine.Settings.GraphicsSettings.ProjectionSetting ProjectionFromString(string projection)
        {
            switch (projection)
            {
                case "orthographic":
                    return Engine.Settings.GraphicsSettings.ProjectionSetting.ORTHOGRAPHIC;
                case "perspective":
                    return Engine.Settings.GraphicsSettings.ProjectionSetting.PERSPECTIVE;
            }
            throw new System.ArgumentException("Invalid projection.");
        }

        public static string ColorToString(SharpDX.Color4 color)
        {
            var rgba = color.ToRgba();
            var r = (rgba) & 255;
            var g = (rgba >> 8) & 255;
            var b = (rgba >> 16) & 255;
            var hex = $"#{r:X2}{g:X2}{b:X2}";
            return hex;
        }

        public static SharpDX.Color4 ColorFromString(string hex)
        {
            var r = int.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
            var g = int.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
            var b = int.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
            var rgba = r | (g << 8) | (b << 16) | (255 << 24);
            return new SharpDX.Color4(rgba);
        }
    }
}
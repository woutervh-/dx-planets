namespace DxPlanets.UI
{
    static class BridgeHelper
    {
        public static string ProjectionToString(Game.Settings.GraphicsSettings.ProjectionSetting projection)
        {
            switch (projection)
            {
                case Game.Settings.GraphicsSettings.ProjectionSetting.ORTHOGRAPHIC:
                    return "orthographic";
                case Game.Settings.GraphicsSettings.ProjectionSetting.PERSPECTIVE:
                    return "perspective";
            }
            return null;
        }

        public static Game.Settings.GraphicsSettings.ProjectionSetting? ProjectionFromString(string projection)
        {
            switch (projection)
            {
                case "orthographic":
                    return Game.Settings.GraphicsSettings.ProjectionSetting.ORTHOGRAPHIC;
                case "perspective":
                    return Game.Settings.GraphicsSettings.ProjectionSetting.PERSPECTIVE;
            }
            return null;
        }
    }
}
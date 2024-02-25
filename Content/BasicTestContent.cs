namespace DtgeCore
{
    public class BasicTestContent
    {
        public BasicTestContent()
        {

        }

        public static bool PopulateSceneManager(SceneManager sceneManager)
        {
            Scene farWest= new Scene("far_west");
            farWest.SceneText = "You are far to the west.";
            farWest.AddOption(new Option(null, null, "West", false));
            farWest.AddOption(new Option("to_west_from_far_west", "west", "East"));
        
            Scene west= new Scene("west");
            west.SceneText = "You are to the west.";
            west.AddOption(new Option("to_far_west", "far_west", "West"));
            west.AddOption(new Option("to_center_from_west", "center", "East"));

            Scene center= new Scene("center");
            center.SceneText = "You are in the center of the area.";
            center.AddOption(new Option("to_west_from_center", "west", "West"));
            center.AddOption(new Option("to_east_from_center", "east", "East"));

            Scene east= new Scene("east");
            east.SceneText = "You are to the east.";
            east.AddOption(new Option("to_center_from_east", "center", "West"));
            east.AddOption(new Option("to_far_east", "far_east", "East"));

            Scene farEast= new Scene("far_east");
            farEast.SceneText = "You are far to the east.";
            farEast.AddOption(new Option("to_east_from_far_east", "east", "West"));
            farEast.AddOption(new Option(null, null, "East", false));

            sceneManager.addScene(farWest);
            sceneManager.addScene(west);
            sceneManager.addScene(center);
            sceneManager.addScene(east);
            sceneManager.addScene(farEast);

            return true;
        }

        public static string GetStartingSceneId()
        {
            return "center";
        }
    }
}
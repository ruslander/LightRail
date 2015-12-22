namespace StableStorage
{
    public class OplogConfig
    {
        public string Name;
        public string BasePath;
        public long Quota;
        public bool Fixed;

        public static OplogConfig IoOptimised(string name)
        {
            return new OplogConfig()
            {
                Name = name,
                Quota = 4 * Units.MEGA,
                Fixed = true,
                BasePath = "ops"
            };
        }

        public static OplogConfig IoQuoted(string name, long q)
        {
            return new OplogConfig()
            {
                Name = name,
                Quota = q,
                Fixed = true,
                BasePath = "ops"
            };
        } 
    }
}
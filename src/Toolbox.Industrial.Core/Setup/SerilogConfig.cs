namespace Toolbox.Industrial.Core.Setup;

public class SerilogConfig
{
    public serilog Serilog { get; set; } = new();

    public class serilog
    {
        public string[] Using { get; set; } = [];
        public MinimumLevelConfig MinimumLevel { get; set; } = new();
        public string[] Enrich { get; set; } = [];
        public WriteToConfig[] WriteTo { get; set; } = [];
    }
}

public class MinimumLevelConfig
{
    public string Default { get; set; } = "Information";
    public Dictionary<string, string> Override { get; set; } = new();
}

public class WriteToConfig
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, object> Args { get; set; } = new();
}


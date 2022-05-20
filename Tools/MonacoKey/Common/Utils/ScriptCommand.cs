namespace Common.Utils
{
    using System.Collections.Generic;

    public class ScriptCommand
    {
        public readonly List<string> Scripts;
        public readonly string Executable = "";
        public string Arguments = "";

        public ScriptCommand(string exe, List<string> scripts, string parameters = "")
        {
            Scripts = scripts;
            Executable = exe;
            Arguments = parameters;
        }
    }
}

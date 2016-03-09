namespace ProtoZBuffer.Console
{
    static class Program
    {
        static void Main(string[] args)
        {
            var success = CommandLineExecuter.Execute(args);
            Logger.Info("Files have " + (!success ? "not " : "") + "been generated correctly");
            if (!success)
                System.Environment.Exit(160); // ERROR_BAD_ARGUMENTS
        }
    }
}


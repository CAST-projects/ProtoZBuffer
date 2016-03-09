using log4net;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace ProtoZBuffer.Utils
{
    public static class Logger
    {
        private static ILog Log { get; set; }

        static Logger()
        {
            Log = LogManager.GetLogger("ProtoZBuffer");
        }

        /// <summary>
        /// Write an information message in the log
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="args">arguments</param>
        public static void Info(string message, params object[] args)
        {
            Log.InfoFormat(message, args);
        }

        /// <summary>
        /// Write a warning message in the log
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="args">arguments</param>
        public static void Warning(string message, params object[] args)
        {
            Log.WarnFormat(message, args);
        }

        /// <summary>
        /// Write an error message in the log
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="args">arguments</param>
        public static void Error(string message, params object[] args)
        {
            Log.ErrorFormat(message, args);
        }

        /// <summary>
        /// Throw an exception to force the program to stop
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="args">arguments</param>
        public static void Fatal(string message, params object[] args)
        {
            Log.FatalFormat(message, args);
        }
    }
}

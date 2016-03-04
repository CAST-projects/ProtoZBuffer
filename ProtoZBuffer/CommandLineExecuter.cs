using System.Text;
using protozbuffer.Generators;

namespace protozbuffer
{
    class CommandLineExecuter : IOptionVisitor
    {
        private CommandLineExecuter()
        {
        }

        public static bool Execute(string[] args)
        {
            var builder = new StringBuilder("protozbuffer.exe");
            foreach (var arg in args)
            {
                builder.Append(" ");
                builder.Append(arg);
            }

            Logger.Info(builder.ToString());

            IOption subOption = null;
            var options = new Options();

            if (!CommandLine.Parser.Default.ParseArguments(args, options, (_, sub) => { subOption = sub as IOption; }))
            {
                return false;
            }

            try
            {
                return Execute(subOption);
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message);
                return false;
            }
        }

        private static bool Execute(IOption option)
        {
            return option != null && option.Accept(new CommandLineExecuter());
        }

        static bool CommonVisit<T>(AbstractOption option) where T : AbstractGenerator, new()
        {
            if (string.IsNullOrEmpty(option.Namespace) || string.IsNullOrWhiteSpace(option.Namespace))
            {
                Logger.Fatal("Namespace option must not be empty or whitespace only");
                return false;
            }
            var generator = new T
            {
                Namespace = option.Namespace,
                OutputFolder = option.Folder,
                ProtoZFile = option.File,
                ProtoGenFolder = option.ProtobufPath
            };

            return generator.Launch();
        }

        bool IOptionVisitor.Visit(CSharpOption option)
        {
            return CommonVisit<CSharpGenerator>(option);
        }

        bool IOptionVisitor.Visit(JavaOption option)
        {
            return CommonVisit<JavaGenerator>(option);
        }

        bool IOptionVisitor.Visit(CppOption option)
        {
            return CommonVisit<CppGenerator>(option);
        }

    }

}

using CommandLine;
using CommandLine.Text;
using ProtoZBuffer.Properties;

namespace ProtoZBuffer
{
    class Options
    {
        [VerbOption("java", HelpText = "Generate the Java code corresponding to your protoZBuffer file")]
        public JavaOption Java { get; [UsedImplicitly] set; }

        [VerbOption("cpp", HelpText = "Generate the C++ code corresponding to your protoZBuffer file")]
        public CppOption Cpp { get; [UsedImplicitly] set; }

        [VerbOption("csharp", HelpText = "Generate the C# code corresponding to your protoZBuffer file")]
        public CSharpOption CSharp { get; [UsedImplicitly] set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            return HelpText.AutoBuild(this, verb);
        }
    }

    interface IOption
    {
        bool Accept(IOptionVisitor visitor);
    }

    interface IOptionVisitor
    {
        bool Visit(CSharpOption option);
        bool Visit(CppOption option);
        bool Visit(JavaOption option);
    }

    abstract class AbstractOption : IOption
    {
        [Option('i', "input", Required = true, HelpText = "Xml protozbuffer file")]
        public string File { get; [UsedImplicitly] set; }

        [Option('o', "output", Required = true, HelpText = "Output folder")]
        public string Folder { get; [UsedImplicitly] set; }

        [Option('n', "namespace", Required = true, HelpText = "Namespace")]
        public string Namespace { get; [UsedImplicitly] set; }

        [Option('p', "protobufPath", HelpText = "Path to the ProtoGen executable, by default, the path to this executable",
            Required = false)]
        public string ProtobufPath { get; set; }

        public abstract bool Accept(IOptionVisitor visitor);
    }

    [UsedImplicitly]
    class CSharpOption : AbstractOption
    {
        public override bool Accept(IOptionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    [UsedImplicitly]
    class CppOption : AbstractOption
    {
        public override bool Accept(IOptionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    [UsedImplicitly]
    class JavaOption : AbstractOption
    {
        public override bool Accept(IOptionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

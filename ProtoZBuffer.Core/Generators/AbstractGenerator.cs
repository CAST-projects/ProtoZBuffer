using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using ProtoZBuffer.Utils;

namespace ProtoZBuffer.Core.Generators
{
    public abstract class AbstractGenerator : IGenerator
    {
        protected AbstractGenerator()
        {
            ProtoGenFolder = null;
        }

        ///<summary>Namespace in which the generated code will be put</summary> 
        public string Namespace { get; set; }

        /// <summary>Namespace for the generated objects, except the final client classes</summary>
        protected string GeneratedNamespace
        {
            get { return (string.IsNullOrEmpty(Namespace) ? "" : Namespace + NamespaceSeparator) + "generated"; }
        }

        ///<summary>Where to store the generated code</summary> 
        public string OutputFolder { get; set; }

        ///<summary>Path to the input protoz file</summary> 
        public string ProtoZFile { get; set; }

        protected string ProtocBinary
        {
            get
            {
                return Path.Combine(ProtoGenFolder, "protoc.exe");
            }
        }

        ///<summary>Path to the protoc executable (or protogen.exe for C#)</summary> 
        public string ProtoGenFolder
        {
            set
            {
                _myProtoGenFolder = string.IsNullOrEmpty(value) ?
                    Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "protobuf-generator") :
                    value;
            }

            protected get
            {
                return _myProtoGenFolder;
            }
        }

        private string _myProtoGenFolder;

        ///<summary>Base name for the generated .proto file, and the .lazy file</summary> 
        protected string DocumentName
        {
            get { return Path.GetFileNameWithoutExtension(ProtoZFile).Capitalize(); }
        }

        ///<summary>Path to the .proto file passed to the protobuf generators</summary> 
        protected string ProtoFile
        {
            get { return Path.Combine(OutputFolder, DocumentName + ".proto"); }
        }

        ///<summary>E.g. for C++: "::", for Java and C#: ".", etc</summary> 
        protected abstract string NamespaceSeparator { get; }

        ///<summary>Namespace to be used for the resources shared between all generated code</summary> 
        protected abstract string ResourceNamespace { get; }

        ///<summary>Subfolder of the 'res' folder in which the resources are located</summary> 
        protected abstract string ResourceFolder { get; }

        ///<summary>Copy all resources to the output folder</summary> 
        protected abstract void InstallResources();

        ///<summary>Command line used to launch protoc.exe, with its options</summary> 
        protected abstract string ProtocArguments { get; }

        ///<summary>Entry point</summary> 
        public bool Launch()
        {
            InstallResources();

            var p = ProtozbuffLoader.Load(ProtoZFile);

            // protobufs package option (i.e. namespace) needs dots
            var protobufPackage = GeneratedNamespace.Replace(NamespaceSeparator, ".");
            ProtoGenerator.Generate(p, ProtoFile, protobufPackage); // generate .proto file

            CallProtocExe();

            return GenerateLazyImplementation(p);
        }

        protected void CopyResourceToOutput(Assembly assembly, string resource, string outputFolder, string nspace)
        {
            Directory.CreateDirectory(outputFolder);

            using (var output = GetStream(outputFolder, resource, nspace))
            using (var input = assembly.GetManifestResourceStream(assembly.GetName().Name + ".res." + ResourceFolder + "." + resource))
            {
                if (input == null)
                {
                    Logger.Warning("Protozbuf resource " + resource + " couldn't be found");
                    return;
                }
                var content = new StreamReader(input).ReadToEnd();
                output.Write(ReplaceNamespaceInContent(content, nspace));
            }
        }

        ///<summary>Replaces the namespace placeholder by the actual namespace (for resources)</summary> 
        protected virtual string ReplaceNamespaceInContent(string content, string nspace)
        {
            return content.Replace("%NAMESPACE%", nspace);
        }

        ///<summary>Runs protoc.exe on the generated .proto file</summary> 
        private void CallProtocExe()
        {
            var psi = new ProcessStartInfo(ProtocBinary)
            {
                WorkingDirectory = Environment.CurrentDirectory,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = ProtocArguments
            };

            var process = Process.Start(psi);

            if (process == null)
            {
                Logger.Fatal("Unable to create process to generate files");
                return;
            }

            process.Start();
            process.WaitForExit();

            if (process.ExitCode == 0) return;
            Logger.Fatal(process.StandardOutput.ReadToEnd());
            throw new Exception("Failed: " + process.StandardError.ReadToEnd());
        }

        protected virtual bool GenerateLazyImplementation(protozbuffType p)
        {
            // in generated namespace
            GenerateProtoOrBuilderInterface();
            GenerateToStringFormatters();
            GenerateHeaderOrBuilderInterfaces(p);
            GenerateAbstractBaseClasses(p);

            // in client namespace
            GenerateFinalClientClasses(p);
            return true;
        }

        internal abstract void GenerateProtoOrBuilderInterface();
        internal abstract void GenerateToStringFormatters();

        internal abstract void GenerateHeaderOrBuilderInterface(messageType message);
        internal virtual void GenerateHeaderOrBuilderInterfaces(protozbuffType node)
        {
            foreach (var message in node.Items.OfType<messageType>())
            {
                GenerateHeaderOrBuilderInterface(message);
            }
        }

        internal virtual void GenerateAbstractBaseClass(messageType message)
        {
            InitializeAbstractClass(message);
            GenerateClassFields(message);
            GenerateClassConstructor(message);
            GeneratePrivateOrBuilderImpl(message);
            GenerateBuild(message);
            GenerateFlush(message);
            GenerateSerialization(message);
            GenerateEqualsAndHashCode(message);
            GenerateToString(message);
            GenerateFieldsAccessors(message);
            GenerateIndexesAccessors(message);
            EndAbstractClass(message);
        }

        internal virtual void GenerateAbstractBaseClasses(protozbuffType node)
        {
            foreach (var message in node.Items.OfType<messageType>())
            {
                GenerateAbstractBaseClass(message);
            }
        }

        internal abstract void GenerateFinalClientClass(messageType message);
        internal virtual void GenerateFinalClientClasses(protozbuffType node)
        {
            foreach (var message in node.Items.OfType<messageType>())
            {
                GenerateFinalClientClass(message);
            }
        }

        protected abstract void InitializeAbstractClass(messageType message);
        protected abstract void GenerateClassFields(messageType message);
        protected abstract void GenerateClassConstructor(messageType message);
        protected abstract void GeneratePrivateOrBuilderImpl(messageType message);
        protected abstract void GenerateBuild(messageType message);
        protected abstract void GenerateFlush(messageType message);
        protected abstract void GenerateSerialization(messageType message);
        protected abstract void GenerateEqualsAndHashCode(messageType message);
        protected abstract void GenerateToString(messageType message);
        protected abstract void EndAbstractClass(messageType message);

        private void GenerateFieldsAccessors(messageType message)
        {
            foreach (var field in message.field.OrderBy(_ => _.id))
            {
                switch (field.type)
                {
                    case typeType.nestedMessage:
                        GenerateClassNestedField(message, field);
                        break;
                    case typeType.referenceMessage:
                        GenerateClassReferenceField(message, field);
                        break;
                    default:
                        GenerateClassSimpleField(message, field);
                        break;
                }
            }
        }

        protected abstract void GenerateClassNestedField(messageType message, fieldType field);
        protected abstract void GenerateClassReferenceField(messageType message, fieldType field);
        protected abstract void GenerateClassSimpleField(messageType message, fieldType field);

        protected abstract void GenerateClassIndex(messageType message, indexType index);
        private void GenerateIndexesAccessors(messageType message)
        {
            foreach (var index in message.index.OrderBy(_ => _.id))
            {
                GenerateClassIndex(message, index);
            }
        }

        protected virtual void WriteAutoGenerationWarning(TextWriter writer)
        {
            writer.WriteLine(
@"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------");
        }

        ///<summary>Removes any file (not directory) named dir before creating it</summary> 
        protected static void SafeDirectoryCreation(string dir)
        {
            if (File.Exists(dir))
                File.Delete(dir);
            Directory.CreateDirectory(dir);
        }

        ///<summary>Creates a file 'name' in a subfolder of 'folder', built from nspace (e.g. boo::bar::athon becomes boo/bar/athon)</summary> 
        protected virtual string GetFilePath(string folder, string name, string nspace)
        {
            SafeDirectoryCreation(folder);
            var currentFolder = folder;
            foreach (var localDir in nspace.Split(new string[] { NamespaceSeparator }, StringSplitOptions.RemoveEmptyEntries))
            {
                currentFolder = Path.Combine(currentFolder, localDir);
                SafeDirectoryCreation(currentFolder);
            }

            return Path.Combine(currentFolder, name);
        }

        protected virtual StreamWriter GetStream(string folder, string name, string nspace)
        {
            return GetStreamFromPath(GetFilePath(folder, name, nspace));
        }

        protected static StreamWriter GetStreamFromPath(string filename)
        {
            return new StreamWriter(File.Open(filename, FileMode.Create, FileAccess.Write));
        }
    }
}

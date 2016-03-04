using System.Diagnostics;
using System.IO;

namespace protozbuffer
{
    class ProtoGenerator : IAstNodeVisitor
    {
        readonly TextWriter _strm;
        private readonly string _namespace;
        private messageType _currentMsg; 

        private ProtoGenerator(TextWriter strm, string nspace)
        {
            _strm = strm;
            _namespace = nspace;
        }

        public static void Generate(protozbuffType p, string protoFilename, string nspace)
        {
            using (var textfile = File.CreateText(protoFilename))
            {
                // Test for now
                textfile.WriteLine("syntax='proto3';");
                Generate(p, textfile, nspace);
            }
        }

        private static void Generate(IAstNode p, TextWriter streamWriter, string nspace)
        {
            p.Accept(new ProtoGenerator(streamWriter,nspace));
        }

        public void Visit(enumElementType node)
        {
            _strm.WriteLine("    {0}{1};"
                , node.name
                , (node.value != null) ? string.Format("={0}", node.value) : ""
                );
        }

        public void Visit(enumType node)
        {
            _strm.WriteLine("enum {0}", node.name);
            _strm.WriteLine("{");
            foreach (var enumElement in node.enumItem)
            {
                enumElement.Accept(this);
            }
            _strm.WriteLine("}");
        }

        public void Visit(fieldType node)
        {
            if (node.type == typeType.nestedMessage || node.type == typeType.referenceMessage)
                _strm.WriteLine("  //{0}", FormatField(node, node.messageType + "Header"));

            _strm.WriteLine("    {0}", FormatField(node, ProtoTypeString(node)));
        }

        public void Visit(indexType node)
        {
            Debug.Assert(node.referenceField != null);

            _strm.WriteLine("  //{0}", FormatIndex(node, node.referenceField.messageType + "Header"));
            _strm.WriteLine("    {0}", FormatIndex(node, ProtoTypeString(node.referenceField)));
        }

        private static string ProtoTypeString(fieldType node)
        {
            switch (node.type)
            {
                case typeType.nestedMessage:
                    return "uint32";
                case typeType.referenceMessage:
                    return "LocalMessageDescriptor";
                case typeType.@enum:
                    return node.enumType;
                default:
                    return node.type.ToString();
            }
        }

        private static string FormatIndex(indexType node, string type)
        {
            var val = string.Format("{0} {1} {2}= {3}{4};"
                , node.referenceField.modifier
                , type
                , node.name
                , node.referenceField.id
                , (node.referenceField.@default != null) ? string.Format(" [default={0}]", node.referenceField.@default) : "");
            return val;
        }

        private static string FormatField(fieldType node, string type)
        {
            var isRepeatedPrimitive = node.modifier == modifierType.repeated 
                && (node.@type != typeType.referenceMessage && node.@type != typeType.@string && node.@type != typeType.bytes);

            var val = string.Format("{0} {1} {2}= {3}{4}{5};"
                , node.modifier
                , type
                , node.name
                , node.id
                , (node.@default != null) ? string.Format(" [default={0}]", node.@default) : ""
                , isRepeatedPrimitive ? " [packed=true]" : "");
            return val;
        }

        public void Visit(messageType node)
        {
            _currentMsg = node;
            _strm.WriteLine("message {0}Header", node.name);
            _strm.WriteLine("{");

            foreach (var field in node.field)
            {
                field.Accept(this);
            }

            foreach (var index in node.index)
            {
                index.Accept(this);
            }

            _strm.WriteLine("}");
            _currentMsg = null;
        }

        public void Visit(protozbuffType node)
        {
            _strm.WriteLine("package {0};", _namespace);

            foreach (IAstNode message in node.Items)
            {
                _strm.WriteLine();
                message.Accept(this);
            }

            GenerateLocalMessageDescriptor();
        }

        private void GenerateLocalMessageDescriptor()
        {
            _strm.WriteLine();
            _strm.WriteLine("message LocalMessageDescriptor");
            _strm.WriteLine("{");
            _strm.WriteLine("	repeated int32 coordinate = 1 [packed=true];");
            _strm.WriteLine("}");
        }
    }
}

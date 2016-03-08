using System.IO;
using System.Linq;

namespace protozbuffer.Generators
{
    class CSharpGenerator : AbstractGenerator
    {
        protected override string NamespaceSeparator
        {
            get { return "."; }
        }

        protected override string ResourceFolder
        {
            get { return "csharp"; }
        }

        protected override string ResourceNamespace
        {
            get { return Namespace + ".Common"; }
        }

        protected override void InstallResources()
        {
            SafeDirectoryCreation(OutputFolder);

            var assembly = GetType().Assembly;
            CopyResourceToOutput(assembly, "IStretchable.cs", OutputFolder, ResourceNamespace);
            CopyResourceToOutput(assembly, "Stretchable.cs", OutputFolder, ResourceNamespace);
            CopyResourceToOutput(assembly, "WeakStretchable.cs", OutputFolder, ResourceNamespace);
        }

        protected override string ProtocCommandLine
        {
            get
            {
                var cmd = Path.Combine(ProtoGenFolder, @"ProtoGen.exe");
                cmd += string.Format(" --proto_path=\"{0}\"", Path.GetDirectoryName(ProtoFile)); // where to search the .proto file
                cmd += string.Format(" -output_directory=\"{0}\"", Path.GetDirectoryName(ProtoFile)); // where to output the generated protobuf files
                cmd += string.Format(" \"{0}\"", ProtoFile);
                return cmd;
            }
        }

        protected TextWriter Writer { get; set; }

        protected override bool GenerateLazyImplementation(protozbuffType p)
        {
            using (Writer = GetStream(OutputFolder, DocumentName + ".lazy.cs", GeneratedNamespace))
            {
                WriteAutoGenerationWarning(Writer);
                WriteUsings();

                Writer.WriteLine(
                    @"namespace {0}
{{
", Namespace);

                base.GenerateLazyImplementation(p);

                Writer.WriteLine("}");
            }

            return true;
        }

        internal override void GenerateProtoOrBuilderInterface()
        {
            Writer.WriteLine(
@"namespace generated
{
    public interface ProtoOrBuilder
    {
        ProtoOrBuilder Root { get; }
        void AddCoordinates(IList<int> coordinates);
        LocalMessageDescriptor LocalMessageDescriptor { get; }
        ProtoOrBuilder Decode(IList<int> coordinates, int index);
        ProtoOrBuilder Decode(LocalMessageDescriptor field);
        Stream ContentStream { get; }
        string ToString(IFormat format);
    }
}
");
        }

        internal override void GenerateToStringFormatters()
        {
            Writer.WriteLine(
@"namespace generated
{  
    public interface IFormat
    {
        int Indentation { get; set; }
        string NewLine { get; set; }
        string Tabulations { get; }
        void FormatHeader(StringBuilder builder, string title);
        void FormatFooter(StringBuilder builder);
        void FormatField<T>(StringBuilder builder, string name, T obj, Expression<Func<T, object>> prop, bool has = true) where T : ProtoOrBuilder;
        void FormatField<T>(StringBuilder builder, string name, T obj, Expression<Func<T, ProtoOrBuilder>> prop, bool has = true) where T : ProtoOrBuilder;
        void FormatField<T>(StringBuilder builder, string name, T obj, Expression<Func<T, IEnumerable<object>>> prop) where T : ProtoOrBuilder;
        void FormatField<T>(StringBuilder builder, string name, T obj, Expression<Func<T, IEnumerable<ProtoOrBuilder>>> prop) where T : ProtoOrBuilder;
    }

    public class BaseFormat : IFormat
    {
        public int Indentation { get; set; }
        public string NewLine { get; set; }

        public string Tabulations
        {
            get
            {
                return new String('\t', Indentation);
            }
        }

        public void FormatHeader(StringBuilder builder, string title)
        {
            builder.Append(Tabulations);
            builder.Append(title);
            builder.Append(NewLine);
            builder.Append(Tabulations);
            builder.Append(""{"");
            builder.Append(NewLine);
            Indentation++;
        }

        public void FormatFooter(StringBuilder builder)
        {
            Indentation--;
            builder.Append(Tabulations);
            builder.Append(""}"");
            builder.Append(NewLine);
        }

        public void FormatField<T>(StringBuilder builder, string name, T obj, Expression<Func<T, object>> prop, bool has = true) where T : ProtoOrBuilder
        {
            if (!has) return;

            var value = has ? prop.Compile()(obj) : null;
            builder.Append(Tabulations);
            builder.Append(name);
            builder.Append("": "");
            builder.Append(has ? value : ""not set"");
            builder.Append(NewLine);
            
        }

        public void FormatField<T>(StringBuilder builder, string name, T obj, Expression<Func<T, ProtoOrBuilder>> prop, bool has = true) where T : ProtoOrBuilder
        {
            if (!has) return;

            var value = has ? prop.Compile()(obj) : null;
            FormatHeader(builder, name);
            builder.Append(has ? value.ToString(this) : Tabulations + ""null"");
            builder.Append(NewLine);
            FormatFooter(builder);
        }

        public void FormatField<T>(StringBuilder builder, string name, T obj, Expression<Func<T, IEnumerable<object>>> prop) where T : ProtoOrBuilder
        {
            var list = prop.Compile()(obj);
            if (!list.Any()) return;

            builder.Append(Tabulations);
            builder.Append(name);
            builder.Append("": "");
            builder.Append(list.Any() ? string.Join("", "", list) : ""empty"");
            builder.Append(NewLine);
        }

        public void FormatField<T>(StringBuilder builder, string name, T obj, Expression<Func<T, IEnumerable<ProtoOrBuilder>>> prop) where T : ProtoOrBuilder
        {
            var list = prop.Compile()(obj);
            if (!list.Any()) return;

            builder.Append(Tabulations);
            builder.Append(name);
            builder.Append("": "");
            builder.Append(NewLine);
            var first = true;
            foreach (var value in list)
            {
                builder.Append(value.ToString(this));
                first = false;
            }
            if (first)
            {
                builder.Append(Tabulations);
                builder.Append(""empty"");
            }
            builder.Append(NewLine);
        }
    }
}
");
        }

        internal override void GenerateHeaderOrBuilderInterface(messageType message)
        {
            Writer.WriteLine(
@"namespace generated
{{
    public interface {0}HeaderOrBuilder
    {{"
                , message.name);
            foreach (var field in message.field)
            {
                var type = (field.type == typeType.referenceMessage) ? "LocalMessageDescriptor" :
                              (field.type == typeType.nestedMessage) ? "uint"
                              : FieldType(field, "HeaderOrBuilder");

                if (field.modifier == modifierType.optional)
                {
                    Writer.WriteLine(
@"        ///  <summary>
        ///  {1}
        ///  </summary>  
        bool Has{0}{{ get; }}"
                        , field.name.Capitalize(), field.description.Safe());
                }
                if (field.modifier == modifierType.repeated)
                {
                    Writer.WriteLine(
@"        ///  <summary>
        ///  {2}
        ///  </summary>  
        {1} Get{0}(int index);
        int {0}Count{{ get; }}"
                        , field.name.Capitalize(), type, field.description.Safe());
                }
                else
                {
                    Writer.WriteLine(
@"        ///  <summary>
        ///  {2}
        ///  </summary>  
        {1} {0}{{ get; }}"
                        , field.name.Capitalize(), type, field.description.Safe());
                }
            }

            foreach (var index in message.index)
            {
                Writer.WriteLine(
@"        ///  <summary>
        ///  {2}
        ///  </summary>  
        {1} Get{0}(int index);
        ///  <summary>
        ///  {2}
        ///  </summary>  
        int {0}Count{{ get; }}"
                    , index.name.Capitalize(), "LocalMessageDescriptor", index.description.Safe());
            }

                Writer.WriteLine(
@"    }}    

    public partial class {0}Header : {0}HeaderOrBuilder
    {{
        public partial class Builder: {0}HeaderOrBuilder
        {{
        }}
    }}
}}
"
            , message.name);
        }

        protected override void InitializeAbstractClass(messageType message)
        {
            Writer.WriteLine(
@"namespace generated
{{
    /// <summary>
    /// {1}
    /// </summary>
    public abstract partial class Abstract{0} : ProtoOrBuilder
    {{"
                , message.name, message.description.Safe());
        }

        protected override void EndAbstractClass(messageType message)
        {
            Writer.WriteLine(
@"    }
}
");
        }

        protected override void GenerateClassFields(messageType message)
        {
            if (!message.IsRoot)
            {
                Writer.WriteLine(
@"        private ProtoOrBuilder _parent;
        internal ProtoOrBuilder Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                _parent = value;
                Root = _parent.Root;
            }
        }

        public int FieldId {get; set; } // field's ID as defined in the protozbuf.xml file (=> the .proto file)
        public int Index { get; set; } // instance's index in the parent's list
");
            }
            else
            {
                Writer.WriteLine(@"        protected Stream _contentStream;");
            }

            Writer.WriteLine(
@"        private {0}HeaderOrBuilder _header;
        internal long PositionInContent {{ get; set; }}
        public int SerializedSize 
        {{ 
            get 
            {{ 
                Debug.Assert(IsBuilt, ""Can't compute size of an unbuilt object!"");
                return (_header as {0}Header).SerializedSize; 
            }} 
        }}"
                , message.name);

            var allFields = message.field.OrderBy(_ => _.id);

            var messageFields = from fld in allFields
                                where fld.messageType != null && fld.type != typeType.referenceMessage
                                select fld;

            foreach (var field in messageFields)
            {
                var fieldtype = FieldType(field);
                switch (field.modifier)
                {
                    case modifierType.repeated:
                        Writer.WriteLine(
@"        internal IStretchable<{0}> _{1};"
                            , fieldtype, field.name);
                        break;
                    default:
                        Writer.WriteLine(
@"        private {0} _{1};"
                            , fieldtype, field.name);
                        break;
                }
            }
        }

        protected override void GenerateClassSimpleField(messageType message, fieldType field)
        {
            var fieldType = FieldType(field);
            if (field.modifier == modifierType.repeated)
            {
                Writer.WriteLine(
@"        /// <summary>
        /// {2}
        /// </summary>
        public void Add{0}({1} item)
        {{
            Debug.Assert(!IsBuilt, ""Can't modify an already built object!"");
            Builder.Add{0}(item);
        }}

        /// <summary>
        /// {2}
        /// </summary>
        public void Remove{0}({1} item)
        {{
            Debug.Assert(!IsBuilt, ""Can't modify an already built object!"");
            Builder.{0}List.Remove(item);
        }}

        /// <summary>
        /// {2}
        /// </summary>
        public {1} Get{0}(int index)
        {{
            return _header.Get{0}(index);
        }}

        /// <summary>
        /// {2}
        /// </summary>
        public IEnumerable<{1}> {0}List
        {{
            get
            {{
                return Enumerable.Range(0, {0}Count).Select(Get{0});
            }}
        }}

        /// <summary>
        /// {2}
        /// </summary>
        public int {0}Count
        {{
            get
            {{
                return _header.{0}Count;
            }}
        }}
"
                    , field.name.Capitalize(), fieldType, field.description.Safe());
                return;
            }

            if (field.modifier == modifierType.optional)
            {
                Writer.WriteLine(
@"        /// <summary>
        /// {1}
        /// </summary>
        public bool Has{0}
        {{
            get
            {{
                return _header.Has{0};
            }}
        }}        

        /// <summary>
        /// {1}
        /// </summary>
        public void Clear{0}()
        {{
            Debug.Assert(!IsBuilt, ""Can't modify an already built object!"");
            Builder.Clear{0}();
        }}
"
                    , field.name.Capitalize(), field.description.Safe());
            }

            Writer.WriteLine(
@"        /// <summary>
        /// {2}
        /// </summary>
        public {1} {0}
        {{
            get
            {{
                return _header.{0};
            }}
            set
            {{
                Debug.Assert(!IsBuilt, ""Can't modify an already built object!"");
                Builder.{0} = value;
            }}
        }}
"
                , field.name.Capitalize(), fieldType, field.description);
        }

        protected override void GenerateClassReferenceField(messageType message, fieldType field)
        {
            var fieldType = FieldType(field);

            if (field.modifier == modifierType.repeated)
            {
                Writer.WriteLine(
@"        /// <summary>
        /// {2}
        /// </summary>
        public void Add{0}({1} item)
        {{
            Debug.Assert(!IsBuilt, ""Can't modify an already built object!"");
            Builder.Add{0}(item.LocalMessageDescriptor);
        }}

        /// <summary>
        /// {2}
        /// </summary>
        public void Remove{0}({1} item)
        {{
            Debug.Assert(!IsBuilt, ""Can't modify an already built object!"");
            Builder.{0}List.Remove(item.LocalMessageDescriptor);
        }}

        /// <summary>
        /// {2}
        /// </summary>
        public {1} Get{0}(int index)
        {{
            return ({1})Root.Decode(_header.Get{0}(index));
        }}

        /// <summary>
        /// {2}
        /// </summary>
        public IEnumerable<{1}> {0}List
        {{
            get
            {{
                return Enumerable.Range(0, {0}Count).Select(Get{0});
            }}
        }}

        /// <summary>
        /// {2}
        /// </summary>
        public int {0}Count
        {{
            get
            {{
                return _header.{0}Count;
            }}
        }}
"
                    , field.name.Capitalize(), fieldType, field.description.Safe());
                return;
            }

            if (field.modifier == modifierType.optional)
            {
                Writer.WriteLine(
@"        /// <summary>
        /// {1}
        /// </summary>
        public bool Has{0}
        {{
            get
            {{
                return _header.Has{0};
            }}
        }}

        /// <summary>
        /// {1}
        /// </summary>
        public void Clear{0}()
        {{
            Debug.Assert(!IsBuilt, ""Can't modify an already built object!"");
            Builder.Clear{0}();
        }}
"
                    , field.name.Capitalize(), field.description.Safe());
            }

            Writer.WriteLine(
@"        /// <summary>
        /// {2}
        /// </summary>
        public {1} {0}
        {{
            get
            {{
                return ({1})Root.Decode(_header.{0});
            }}
            set
            {{
                Debug.Assert(!IsBuilt, ""Can't modify an already built object!"");
                Builder.{0} = value.LocalMessageDescriptor;
            }}
        }}
"
                , field.name.Capitalize(), fieldType, field.description.Safe());
        }

        protected override void GenerateClassNestedField(messageType message, fieldType field)
        {
            var fieldType = FieldType(field);
            if (field.modifier == modifierType.repeated)
            {
                Writer.WriteLine(
@"        /// <summary>
        /// {4}
        /// </summary>
        public {2} Add{1}()
        {{
            Debug.Assert(!IsBuilt, ""Can't modify an already built object!"");

            {2} item = new {2}()
            {{
                FieldId = {3}, 
                Index = _{0}.Count, 
                Parent = this
            }};

            _{0}.Add(item);
            return item;
        }}

        /// <summary>
        /// {4}
        /// </summary>
        public IEnumerable<{2}> {1}List
        {{
            get
            {{
                return Enumerable.Range(0, {1}Count).Select(Get{1});
            }}
        }}

        /// <summary>
        /// {4}
        /// </summary>
        public {2} Get{1}(int index)
        {{
            if (index >= {1}Count) return null;

            var {0} = _{0}[index];
            if ({0} == null)
            {{
                {0} = {2}.ParseFrom(ContentStream, _header.Get{1}(index));
                _{0}[index] = {0};
                {0}.FieldId = {3};
                {0}.Index = index;
                {0}.Parent = this;
            }}
            return {0};
        }}

        /// <summary>
        /// {4}
        /// </summary>
        public int {1}Count
        {{
            get
            {{
                return !IsBuilt? _{0}.Count : _header.{1}Count;
            }}
        }}"
                    , field.name, field.name.Capitalize(), fieldType, field.id, field.description.Safe());
                return;
            }

            if (field.modifier == modifierType.optional)
            {
                Writer.WriteLine(
@"        /// <summary>
        /// {3}
        /// </summary>
        public {2} Add{1}()
        {{
            if (_{0} == null)
            {{
                if (IsBuilt && Has{1})
                {{
                    {1}.Equals(null); // decode from body (C# needs a function call)
                }}
                else
                {{
                    Debug.Assert(!IsBuilt, ""Can't modify an already built object!"");
                    _{0} = new {2}()
                    {{
                        FieldId = {3}, 
                        Parent = this
                    }};
                }}
            }}
            return _{0};
        }}

        /// <summary>
        /// {3}
        /// </summary>
        public {2} {1}
        {{
            get
            {{
                if (IsBuilt && Has{1} && _{0} == null)
                {{
                    _{0} = {2}.ParseFrom(ContentStream, _header.{1});
                    _{0}.FieldId = {3};
                    _{0}.Parent = this;
                }}
                return _{0};
            }}
        }}

        /// <summary>
        /// {3}
        /// </summary>
        public bool Has{1}
        {{
            get
            {{
                return (IsBuilt && _header.Has{1}) || (!IsBuilt && _{0} != null);
            }}
        }}

        /// <summary>
        /// {3}
        /// </summary>
        public void Clear{1}()
        {{
            Debug.Assert(!IsBuilt, ""Can't modify an already built object!"");
            _{0} = null;
        }}
"
                    , field.name, field.name.Capitalize(), fieldType, field.id);
                return;
            }

            Writer.WriteLine(
@"        /// <summary>
        /// {4}
        /// </summary>
        public {2} {1}
        {{
            get
            {{
                if (_{0} == null)
                {{
                    if (IsBuilt)
                    {{
                        _{0} = {2}.ParseFrom(ContentStream, _header.{1});
                    }}
                    else
                    {{
                        _{0} = new {2}();
                    }}

                    _{0}.FieldId = {3};
                    _{0}.Parent = this;
                }}
                return _{0};
            }}
        }}
"
                , field.name, field.name.Capitalize(), fieldType, field.id, field.description.Safe());
        }

        protected override void GenerateClassIndex(messageType message, indexType index)
        {
            // indexes are build at build time
            var field = index.ReferenceField;
            var fieldType = FieldType(field);
            var sortByType = FieldType(index.SortingField);

            Writer.WriteLine(
@"        /// <summary>
        /// {4}
        /// </summary>
        public IEnumerable<{1}> {0}List
        {{
            get
            {{
                Debug.Assert(IsBuilt, ""Index is not built yet!"");
                return Enumerable.Range(0, {0}Count).Select(Get{0});
            }}
        }}

        /// <summary>
        /// {4}
        /// </summary>
        public {1} Get{0}(int index)
        {{
            Debug.Assert(IsBuilt, ""Index is not built yet!"");
            return ({1})Root.Decode(_header.Get{0}(index));
        }}

        /// <summary>
        /// {4}
        /// </summary>
        public int {0}Count
        {{
            get
            {{
                Debug.Assert(IsBuilt, ""Index is not built yet!"");
                return _header.{0}Count;
            }}
        }}

        /// <summary>
        /// {4}
        /// </summary>
        public {1} Search{0}({2} item)
        {{
            Debug.Assert(IsBuilt, ""Index is not built yet!"");
            return Search{0}(item, 0, {0}Count - 1);
        }}

        /// <summary>
        /// {4}
        /// </summary>
        protected {1} Search{0}({2} item, int min, int max)
        {{
            Debug.Assert(IsBuilt, ""Index is not built yet!"");

            if (max < min)
                return null;

            int avg = (min + max) >> 1;

            var candidate = Get{0}(avg);
            var candidateKey = candidate.{3};
            if (candidateKey == item)
                return candidate;

            if (candidateKey < item)
                return Search{0}(item, avg + 1, max);

            return Search{0}(item, min, avg - 1);
        }}
", index.name.Capitalize(), fieldType, sortByType, index.sortBy.Capitalize(), index.description.Safe());
        }

        protected override void GenerateClassConstructor(messageType message)
        {
            Writer.WriteLine(
@"        /// <summary>
        /// {1}
        /// </summary>
        protected Abstract{0}()
        {{
            _header = new {0}Header.Builder();
            PositionInContent = -1;
            Flush();
        }}

        /// <summary>
        /// {1}
        /// </summary>
        protected Abstract{0}({0}Header header, long positionInContent)
        {{
            _header = header;
            PositionInContent = positionInContent;
            Flush();
        }}
", message.name, message.description.Safe());
        }

        protected override void GeneratePrivateOrBuilderImpl(messageType message)
        {
            Writer.WriteLine(message.IsRoot
                ?
@"        public ProtoOrBuilder Root { get { return this; } }
"
                :
@"        public ProtoOrBuilder Root { get; private set; }
");
            Writer.WriteLine(
@"        public ProtoOrBuilder Decode(LocalMessageDescriptor field)
        {
            return Decode(field.CoordinateList, 0);
        }

        public ProtoOrBuilder Decode(IList<int> coordinates, int index)
        {
            if (coordinates.Count == 0)
                return null;

            var fieldIdIdx = index;
            var fieldIndexIdx = index + 1;
            var remainderIdx = index + 2;
            switch(coordinates[fieldIdIdx])
            {"
                );
            var allFields = message.field.Where(_ => _.type == typeType.nestedMessage || _.type == typeType.referenceMessage).OrderBy(_ => _.id);
            foreach (var field in allFields)
            {
                Writer.WriteLine(
@"                case {0}:"
                    , field.id);

                if (field.modifier == modifierType.repeated)
                {
                    Writer.WriteLine(
                        field.type != typeType.nestedMessage
                            ?
@"                    return Get{0}(coordinates[fieldIndexIdx]);"
                            :
@"                    return coordinates.Count == remainderIdx ? Get{0}(coordinates[fieldIndexIdx]) : Get{0}(coordinates[fieldIndexIdx]).Decode(coordinates, remainderIdx);"
                            , field.name.Capitalize());
                }
                else if (field.modifier == modifierType.optional)
                {
                    Writer.WriteLine(
                        field.type != typeType.nestedMessage
                            ?
@"                  return {0};"
                            :
@"                  return coordinates.Count == remainderIdx ? Add{0}() : {0}.Decode(coordinates, remainderIdx);",
                        field.name.Capitalize());
                }
                else
                {
                    Writer.WriteLine(
                        field.type != typeType.nestedMessage
                            ?
@"                    return {0};"
                            :
@"                    return coordinates.Count == remainderIdx ? {0} : {0}.Decode(coordinates, remainderIdx);"
                        , field.name.Capitalize());
                }
            }
            Writer.WriteLine(
@"                default:
                    return null;
            }
        }

        public void AddCoordinates(IList<int> coordinates)
        {");

            if (!message.IsRoot)
            {
                Writer.WriteLine(
@"            coordinates.Insert(0, FieldId);
            coordinates.Insert(1, Index);

            if (Parent == null)
                return;

            Parent.AddCoordinates(coordinates);"
                );
            }

            Writer.WriteLine(
@"        }

        public LocalMessageDescriptor LocalMessageDescriptor
        {
            get
            {
                var b = new LocalMessageDescriptor.Builder();
                AddCoordinates(b.CoordinateList);
                return b.Build();
            }
        }
");

            Writer.WriteLine(
@"        private {0}Header.Builder Builder {{ get {{ return _header as {0}Header.Builder; }} }}
        public bool IsBuilt {{ get {{ return !(_header is {0}Header.Builder); }} }}
"
                    , message.name);
        }

        protected override void GenerateBuild(messageType message)
        {
            if (!message.IsRoot)
            {
                Writer.WriteLine(
@"        public Stream ContentStream
        {
            get
            {
                return Root.ContentStream;
            }
        }
");
            }
            else
            {
                Writer.WriteLine(
@"        public Stream ContentStream
        {
            get
            {
                if (_contentStream == null)
                    _contentStream = new MemoryStream();
                return _contentStream;
            }
            set
            {    
                if (_contentStream != null)
                    _contentStream.Dispose();

                _contentStream = value;
            }
        }
");
            }

            Writer.WriteLine(
@"        public virtual void PreBuild()
        {{
            // use this method to customize the build process
        }}

        public void Build()
        {{
            Build(ContentStream, false);
        }}

        internal void Build(Stream content, bool saveToOutput)
        {{
            var alreadyBuilt = IsBuilt;
            if (alreadyBuilt && !saveToOutput)
                return;

            // prebuild hook
            PreBuild();

            var builder = Builder;
            if (builder == null)
                builder = new {0}Header.Builder(({0}Header) _header);", message.name);

            foreach (var field in message.field.Where(_ => _.type == typeType.nestedMessage))
            {
                Writer.WriteLine(@"            builder.Clear{0}();", field.name.Capitalize());
            }

            foreach (var index in message.index)
            {
                Writer.WriteLine(@"            builder.Clear{0}();", index.name.Capitalize());
            }

            Writer.WriteLine(
@"
            // build all nested messages to have their position in the content stream");

            // do nothing for reference messages and pod types: the builder is already the owner of those fields

            foreach (var field in message.field.Where(_ => _.type == typeType.nestedMessage))
            {
                if (field.modifier == modifierType.repeated)
                {
                    Writer.WriteLine(
@"            var tmp_{0}List = {1}List;
            foreach(var {0} in tmp_{0}List) 
            {{
                var oldPos = {0}.PositionInContent;
                {0}.Build(content, saveToOutput);
                builder.Add{1}((uint){0}.PositionInContent);
                if (alreadyBuilt || saveToOutput)
                    {0}.PositionInContent = oldPos;
            }}
", field.name, field.name.Capitalize());
                }
                else
                {
                    Writer.WriteLine(
@"            var tmp_{0} = {1};
            if (tmp_{0} != null) 
            {{ 
                var oldPos = tmp_{0}.PositionInContent;
                tmp_{0}.Build(content, saveToOutput); 
                builder.{1} = (uint)tmp_{0}.PositionInContent;
                if (alreadyBuilt)
                    tmp_{0}.PositionInContent = oldPos;
            }}
", field.name, field.name.Capitalize());
                }
            }

            // create indexes
            foreach (var index in message.index)
            {
                Writer.WriteLine(
@"            
            foreach(var {0} in tmp_{0}List.OrderBy(item => item.{2}))
            {{
                builder.Add{1}({0}.LocalMessageDescriptor);
            }}"
                    , index.ReferenceField.name, index.name.Capitalize(), index.sortBy.Capitalize());
            }

            Writer.WriteLine(
@"            // write the header
            content.Seek(0, SeekOrigin.End);

            // if we write to output, the position in the content stream
            // will be restored when writing the parent header
            // => this is not possible (and not needed) for root message
            var isRoot = {0};
            var dontSavePos = saveToOutput && isRoot;
            if (!dontSavePos)
                PositionInContent = content.Position;

            var builtHeader = builder.Build();
            builtHeader.WriteDelimitedTo(content);
"
                , message.IsRoot ? "true" : "false");

            // write the message length at the end for later decoding
            // Note: the length is fixed
            if (message.IsRoot)
            {
                Writer.WriteLine(
@"            var codedStream = CodedOutputStream.CreateInstance(content, sizeof(uint));
            codedStream.WriteFixed32NoTag((uint)builtHeader.SerializedSize);
            codedStream.Flush();
");
            }

            Writer.WriteLine(
@"
            if (!alreadyBuilt && !saveToOutput)
            {
                _header = builtHeader;
                Flush();
            }

            if (alreadyBuilt && saveToOutput)
            {
                Flush();
            }
        }
");

            if (!message.IsRoot)
                return;

            Writer.WriteLine(
@"        public void WriteDelimitedTo(Stream output)
        {
            Build(output, true);
        }

        public byte[] WriteDelimitedToBytes()
        {
            using (var strm = new MemoryStream())
            {
                WriteDelimitedTo(strm);
                return strm.ToArray();
            }
        }
"
                );
        }

        protected override void GenerateFlush(messageType message)
        {
            Writer.WriteLine(
@"        public void Flush()
        {"
                );

            var allFields = message.field.OrderBy(_ => _.id);

            var messageFields = from fld in allFields
                                where fld.messageType != null
                                && fld.type == typeType.nestedMessage
                                select fld;

            foreach (var field in messageFields)
            {
                if (field.modifier == modifierType.repeated)
                    Writer.WriteLine(
@"            _{0} = !IsBuilt ? (IStretchable<{1}>)new Stretchable<{1}>() : (IStretchable<{1}>)new WeakStretchable<{1}>();"
                        , field.name, field.messageType);
                else
                    Writer.WriteLine(
@"            _{0} = null;"
                        , field.name);
            }

            Writer.WriteLine(
@"        }
"
                );
        }
        
        protected override void GenerateSerialization(messageType message)
        {
            if (message.IsRoot)
            {
                Writer.WriteLine(
    @"        public static {0} ParseFrom(Stream strm)
        {{
            // the root is always at the end of the stream
            var size = sizeof(uint);
            var offset = -size;

            lock(strm)
            {{
                strm.Seek(offset, SeekOrigin.End);

                uint msgSize = 0;
                var success = CodedInputStream.CreateInstance(strm).ReadFixed32(ref msgSize);
                Debug.Assert(success, ""Unable to read message size"");

                offset -= (int) msgSize + CodedOutputStream.ComputeInt32SizeNoTag((int)msgSize);
                var pos = strm.Seek(offset, SeekOrigin.End);

                var header = {0}Header.ParseDelimitedFrom(strm);
                Debug.Assert(header != null, ""Can't decode header!"");

                var parsed = new {0}(header, pos);
                parsed.ContentStream = strm;
                return parsed;
            }}
        }}

        public static {0} ParseFrom(byte[] bytes)
        {{
            return ParseFrom(new MemoryStream(bytes));
        }}
", message.name);
                return;
            }

            Writer.WriteLine(
@"        internal static {0} ParseFrom(Stream strm, uint pos)
        {{
            lock(strm)
            {{
                strm.Seek(pos, SeekOrigin.Begin);

                var header = {0}Header.ParseDelimitedFrom(strm);
                Debug.Assert(header != null, ""Can't decode header!"");

                var parsed = new {0}(header, pos);
                return parsed;
            }}
        }}
", message.name);
        }

        protected override void GenerateEqualsAndHashCode(messageType message)
        {
            if (!message.IsRoot)
            {
                Writer.WriteLine(
@"        public override bool Equals(Object other)
        {{
            var that = other as {0};
            if (that == null)
                return false;

            var result = (FieldId == that.FieldId) && (Index == that.Index);

            if (!result)
                return false;

            if (Parent == null)
                return that.Parent == null;

            return Parent.Equals(that.Parent);
        }}

        public override int GetHashCode()
        {{
            //Fowler Noll Vo hash algorithm, FNV-1a 32bit
            unchecked
            {{
                const int prime = 0x01000193;
                var hash = (int) 0x811C9DC5;

                if (Parent != null)
                    hash = prime * (hash ^ Parent.GetHashCode());
                hash = prime * (hash ^ FieldId);
                hash = prime * (hash ^ Index);

                return hash;
            }}   
        }}
", message.name);
                return;
            }

            var fields = message.field.Where(_ => _.modifier == modifierType.required).OrderBy(_ => _.id).ToList();

            Writer.Write(
@"        public override bool Equals(Object other)
        {{
            if (other == null)
                return false;

            var that = other as {0};

            if (that == null)
                return false;

            return "
, message.name);
            var first = true;
            foreach (var field in fields)
            {
                if (!first) Writer.Write(" && ");

                Writer.Write(
@"({0}.Equals(that.{0}))", field.name.Capitalize());
                first = false;
            }

            if (first)
                Writer.Write("true");

            Writer.WriteLine(";");

            Writer.WriteLine(
@"
        }

        public override int GetHashCode()
        {
            unchecked
            {
                {
                    const int prime = 0x01000193;
                    var hash = (int) 0x811C9DC5;");

            foreach (var field in fields)
            {
                Writer.WriteLine(
@"                  hash = prime*(hash ^ {0}.GetHashCode());", field.name.Capitalize());
            }
            Writer.WriteLine(
@"
                    return hash;
                }
            }
        }
");
        }

        protected override void GenerateToString(messageType message)
        {
            Writer.WriteLine(
@"        public override string ToString()
        {
            return ToString(new BaseFormat() { Indentation = 0, NewLine = System.Environment.NewLine });
        }

        public virtual string ToString(IFormat format)
        {
            var bd = new StringBuilder();");

            Writer.WriteLine(
@"            format.FormatHeader(bd,""{0}"");
", message.name);

            var fields = message.field.OrderBy(_ => _.id);

            foreach (var field in fields)
            {
                switch (field.modifier)
                {
                    case modifierType.repeated:
                        if (field.type == typeType.referenceMessage)
                        {
                            Writer.WriteLine(
    @"            format.FormatField(bd, ""{0}List"", this, _ => _.{0}List.Select(item => ""{0}"" + item.Index)); 
    ", field.name.Capitalize());
                        }
                        else
                        {
                            Writer.WriteLine(
    @"            format.FormatField(bd, ""{0}List"", this, _ => _.{0}List);
    ", field.name.Capitalize());
                        }
                        break;
                    case modifierType.optional:
                        Writer.WriteLine(
@"            format.FormatField(bd, ""{0}"", this, _ => _.{0}, Has{0});
", field.name.Capitalize());
                        break;
                    default:
                        Writer.WriteLine(
@"            format.FormatField(bd, ""{0}"", this, _ => _.{0});", field.name.Capitalize());
                        break;
                }
            }

            Writer.WriteLine(
@"            format.FormatFooter(bd);
            return bd.ToString();
        }
");
        }
        
        private void WriteUsings()
        {
            Writer.WriteLine(
@"using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.IO;
using System.Collections.Generic;
using {1}.Common;
using Google.ProtocolBuffers;
using {0};
", GeneratedNamespace, Namespace);
        }

        private static string FieldType(fieldType node, string suffix = "")
        {
            switch (node.type)
            {
                case typeType.nestedMessage:
                    return node.messageType + suffix;
                case typeType.referenceMessage:
                    return node.messageType + suffix;
                case typeType.@enum:
                    return node.enumType;
                case typeType.uint32:
                    return "uint";
                case typeType.int32:
                    return "int";
                default:
                    return node.type.ToString();
            }
        }

        internal override void GenerateFinalClientClass(messageType message)
        {
            Writer.WriteLine(
@"    public partial class {0} : Abstract{0}
    {{
        public {0}() {{ /* NOP */ }}
        public {0}({0}Header header, long posInContent) : base(header, posInContent) {{ /* NOP */ }}
    }}
"
                , message.name);
        }
    }
}


using System.IO;
using System.Linq;
using ProtoZBuffer.Utils;

namespace ProtoZBuffer.Core.Generators
{
    public class JavaGenerator : AbstractGenerator
    {
        protected override string NamespaceSeparator
        {
            get { return "."; }
        }

        protected override string ResourceFolder
        {
            get { return "java"; }
        }

        protected override string ResourceNamespace
        {
            get
            {
                return Namespace + ".common";
            }
        }

        protected override void InstallResources()
        {
            SafeDirectoryCreation(OutputFolder);

            var assembly = GetType().Assembly;
            CopyResourceToOutput(assembly, "Extensions.java", OutputFolder, ResourceNamespace);
            CopyResourceToOutput(assembly, "IIOStream.java", OutputFolder, ResourceNamespace);
            CopyResourceToOutput(assembly, "ByteArrayIOStream.java", OutputFolder, ResourceNamespace);
            CopyResourceToOutput(assembly, "FileIOStream.java", OutputFolder, ResourceNamespace);
            CopyResourceToOutput(assembly, "IStretchableArray.java", OutputFolder, ResourceNamespace);
            CopyResourceToOutput(assembly, "StretchableArray.java", OutputFolder, ResourceNamespace);
            CopyResourceToOutput(assembly, "LazyArray.java", OutputFolder, ResourceNamespace);
            CopyResourceToOutput(assembly, "IFilter.java", OutputFolder, ResourceNamespace);
            CopyResourceToOutput(assembly, "IProduct.java", OutputFolder, ResourceNamespace);
            CopyResourceToOutput(assembly, "IMapper.java", OutputFolder, ResourceNamespace);
            CopyResourceToOutput(assembly, "IFormat.java", OutputFolder, ResourceNamespace);
        }

        protected override string ProtocCommandLine
        {
            get
            {
                var cmd = Path.Combine(ProtoGenFolder, "protoc.exe"); // path to protoc.exe
                cmd += string.Format(" --proto_path=\"{0}\"", Path.GetDirectoryName(ProtoFile)); // where to search the .proto file
                cmd += string.Format(" --java_out=\"{0}\"", Path.GetDirectoryName(ProtoFile)); // where to output the generated protobuf files
                cmd += string.Format(" \"{0}\"", ProtoFile);
                return cmd;
            }
        }

        protected TextWriter Writer { get; set; }

        internal override void GenerateProtoOrBuilderInterface()
        {
            using (var strm = GetStream(OutputFolder, "ProtoOrBuilder.java", GeneratedNamespace))
            {
                WriteAutoGenerationWarning(strm);

                strm.WriteLine(
    @"package {0};
", GeneratedNamespace);

                strm.WriteLine(
@"import java.util.List;

import {0}.{1}.LocalMessageDescriptor;
import {2}.IFormat;
import {2}.IIOStream;

@SuppressWarnings(""all"")
public interface ProtoOrBuilder
{{
    ProtoOrBuilder getRoot();
    void addCoordinates(List<Integer> coordinates);
    LocalMessageDescriptor getLocalMessageDescriptor();
    ProtoOrBuilder decode(List<Integer> coordinates, int index);
    ProtoOrBuilder decode(LocalMessageDescriptor field);
    IIOStream getContentStream();
    String toString(IFormat format);
}}", GeneratedNamespace, DocumentName, ResourceNamespace);
            }
        }

        internal override void GenerateToStringFormatters()
        {
            using (var strm = GetStream(OutputFolder, "BaseFormat.java", GeneratedNamespace))
            {
                strm.WriteLine(
    @"package {0};
", GeneratedNamespace);

                WriteAutoGenerationWarning(strm);

                strm.WriteLine(
@"import java.util.Arrays;
import java.util.Iterator;
import java.util.List;
import {0}.IFormat;

@SuppressWarnings(""all"")
public class BaseFormat implements IFormat
{{
    private int _indentation;
    private String _newLine;

    public BaseFormat(int indentation, String newLine)
    {{
        _indentation = indentation;
        _newLine = newLine;
    }}

    public static String join(String delimiter, Iterable<?> s) 
    {{
        Iterator<?> iter = s.iterator();
        if (!iter.hasNext()) return """";
        StringBuilder buffer = new StringBuilder(iter.next().toString());
        while (iter.hasNext()) buffer.append(delimiter).append(iter.next());
        return buffer.toString();
    }}

    @Override
    public void setIndentation(int indentation) {{ _indentation = indentation; }}

    @Override
    public int getIndentation() {{ return _indentation; }}

    @Override
    public String getNewLine() {{ return _newLine; }}

    @Override
    public String getTabulations()
    {{
        char[] chars = new char[_indentation];
        Arrays.fill(chars, '\t');
        return new String(chars);
    }}

    @Override
    public void formatHeader(StringBuilder builder, String title)
    {{
        builder.append(getTabulations());
        builder.append(title);
        builder.append(getNewLine());
        builder.append(getTabulations());
        builder.append(""{{"");
        builder.append(getNewLine());
        _indentation++;
    }}

    @Override
    public void formatFooter(StringBuilder builder)
    {{
        _indentation--;
        builder.append(getTabulations());
        builder.append(""}}"");
        builder.append(getNewLine());
    }}

    @Override
    public <T> void formatField(StringBuilder bd, String title, T field)
    {{
        if (field instanceof ProtoOrBuilder)
        {{
            formatComplexField(bd, title, (ProtoOrBuilder)field, field != null);
            return;
        }}

        formatSimpleField(bd, title, field, field != null);
    }}

    private void formatSimpleField(StringBuilder builder, String name, Object field, boolean has)
    {{
        if (!has) return;

        builder.append(getTabulations());
        builder.append(name);
        builder.append("": "");
        builder.append(has ? field : ""not set"");
        builder.append(getNewLine());
    }}

    private void formatComplexField(StringBuilder builder, String name, ProtoOrBuilder field, boolean has)
    {{
        if (!has) return;
        
        formatHeader(builder, name);
        builder.append(has ? field.toString(this) : getTabulations() + ""null"");
        builder.append(getNewLine());
        formatFooter(builder);
    }}

    @SuppressWarnings(""unchecked"")
    @Override
    public <T> void formatField(StringBuilder bd, String name, List<T> list)
    {{
        if (list.isEmpty()) return;

        if (list.get(0) instanceof ProtoOrBuilder)
        {{
            formatComplexField(bd, name, (List<ProtoOrBuilder>)list);
            return;
        }}

        formatListField(bd, name, list);
    }}

    private void formatComplexField(StringBuilder builder, String name, List<ProtoOrBuilder> list)
    {{
        if (list.isEmpty()) return;

        builder.append(getTabulations());
        builder.append(name);
        builder.append("": "");
        builder.append(getNewLine());
        boolean first = true;
        for (ProtoOrBuilder value : list)
        {{
            builder.append(value.toString(this));
            first = false;
        }}
        if (first)
        {{
            builder.append(getTabulations());
            builder.append(""empty"");
        }}
        builder.append(getNewLine());
    }}

    private void formatListField(StringBuilder builder, String name, List<?> list)
    {{
        builder.append(getTabulations());
        builder.append(name);
        builder.append("": "");
        builder.append(!list.isEmpty() ? join("", "", list) : ""empty"");
        builder.append(getNewLine());
    }}

}}
", ResourceNamespace);
            }
        }

        internal override void GenerateHeaderOrBuilderInterface(messageType message)
        {
            // NOP: protoc.exe generates those for Java
        }

        protected override void InitializeAbstractClass(messageType message)
        {
            Writer = GetStream(OutputFolder, "Abstract" + message.name + ".java", GeneratedNamespace);
            WriteAutoGenerationWarning(Writer);

            Writer.WriteLine(
@"package {0};
", GeneratedNamespace);

            GenerateImports(message);

            Writer.WriteLine(
@"@SuppressWarnings(""all"")
public abstract class Abstract{0} implements ProtoOrBuilder
{{"
                    , message.name);
        }

        protected override void EndAbstractClass(messageType message)
        {
            Writer.WriteLine(
@"}"
);
            Writer.Dispose();
            Writer = null;
        }

        protected override void GenerateClassFields(messageType message)
        {
            if (!message.IsRoot)
            {
                Writer.WriteLine(
@"    private ProtoOrBuilder _root;
    private ProtoOrBuilder _parent;
    private int _fieldId = -1; // field's ID as defined in the protozbuf.xml file (=> the .proto file)
    private int _index = -1; // instance's _index in the _parent's list

    public ProtoOrBuilder getParent() { return _parent; }
    public void setParent(ProtoOrBuilder parent)
    {
        _parent = parent;
        _root = _parent.getRoot();
    }

    public int getFieldId() { return _fieldId; }
    public void setFieldId(int fieldId) { _fieldId = fieldId; }
    public int getIndex() { return _index; }
    public void setIndex(int index) { _index = index; }
    
");
            }
            else
            {
                Writer.WriteLine(@"    protected IIOStream _contentStream;");
            }

            Writer.WriteLine(
@"    protected {0}HeaderOrBuilder _header;
    protected int _positionInContent = -1;

    public int getPositionInContent() {{ return _positionInContent; }}
    public void setPositionInContent(int positionInContent) {{ _positionInContent = positionInContent; }}
"
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
@"    private IStretchableArray<{0}> _{1}List;", fieldtype, field.name);
                        break;
                    default:
                        Writer.WriteLine(
@"    private {0} _{1};", fieldtype, field.name);
                        break;
                }
            }
        }

        protected override void GenerateClassSimpleField(messageType message, fieldType field)
        {
            var boxedType = BoxedType(field, "");
            var fieldType = FieldType(field);
            if (field.modifier == modifierType.repeated)
            {
                Writer.WriteLine(
@"    /**
    * {3}
    **/
    public void add{0}({1} item)
    {{
        assert !isBuilt() : ""Can't modify an already built object!"";
        getBuilder().add{0}(item);
    }}

    /**
    * {3}
    **/
    public void remove{0}({1} item)
    {{
        assert !isBuilt() : ""Can't modify an already built object!"";
        getBuilder().get{0}List().remove(item);
    }}

    /**
    * {3}
    **/
    public {1} get{0}(int index)
    {{
        return _header.get{0}(index);
    }}

    /**
    * {3}
    **/
    public List<{2}> get{0}List()
    {{
        List<{2}> l = new ArrayList<{2}>();
        int n = get{0}Count();
        for(int i = 0; i < n; i++)
            l.add(get{0}(i));
        return l;
    }}

    /**
    * {3}
    **/
    public int get{0}Count()
    {{
        return _header.get{0}Count();
    }}
"
                        , field.name.Capitalize(), fieldType, boxedType, field.description.Safe());
                return;
            }

            if (field.modifier == modifierType.optional)
            {
                Writer.WriteLine(
@"    /**
    * {1}
    **/
    public boolean has{0}()
    {{
        return _header.has{0}();
    }}

    /**
    * {1}
    **/
    public void clear{0}()
    {{
        assert !isBuilt() : ""Can't modify an already built object!"";
        getBuilder().clear{0}();
    }}
"
                        , field.name.Capitalize(), field.description.Safe());
            }

            Writer.WriteLine(
@"    /**
    * {2}
    **/
    public {1} get{0}()
    {{
        return _header.get{0}();
    }}

    /**
    * {2}
    **/
    public void set{0}({1} value)
    {{
        assert !isBuilt() : ""Can't modify an already built object!"";
        getBuilder().set{0}(value);
    }}
"
                    , field.name.Capitalize(), fieldType, field.description.Safe());
        }

        protected override void GenerateClassReferenceField(messageType message, fieldType field)
        {
            var fieldType = FieldType(field);
            var boxedType = BoxedType(field, "");

            if (field.modifier == modifierType.repeated)
            {
                Writer.WriteLine(
@"    /**
    * {3}
    **/
    public void add{0}({1} item)
    {{
        assert !isBuilt() : ""Can't modify an already built object!"";
        getBuilder().add{0}(item.getLocalMessageDescriptor());
    }}

    /**
    * {3}
    **/
    public void remove{0}({1} item)
    {{
        assert !isBuilt() : ""Can't modify an already built object!"";
        getBuilder().get{0}BuilderList().remove(item.getLocalMessageDescriptor());
    }}

    /**
    * {3}
    **/
    public {1} get{0}(int index)
    {{
        return ({1})getRoot().decode(_header.get{0}(index));
    }}

    /**
    * {3}
    **/
    public List<{2}> get{0}List()
    {{
        List<{2}> l = new ArrayList<{2}>();
        int n = get{0}Count();
        for(int i = 0; i < n; i++)
            l.add(get{0}(i));
        return l;
    }}

    /**
    * {3}
    **/
    public int get{0}Count()
    {{
        return _header.get{0}Count();
    }}
"
                            , field.name.Capitalize(), fieldType, boxedType, field.description.Safe());
                return;
            }

            if (field.modifier == modifierType.optional)
            {
                Writer.WriteLine(
@"    /**
    * {1}
    **/
    public boolean has{0}()
    {{
        return _header.has{0}();
    }}      

    /**
    * {1}
    **/
    public void clear{0}()
    {{
        assert !isBuilt() : ""Can't modify an already built object!"";
        getBuilder().clear{0}();
    }}
"
                        , field.name.Capitalize(),field.description.Safe());
            }

            Writer.WriteLine(
@"    /**
    * {2}
    **/
    public {1} get{0}()
    {{
        return ({1})getRoot().decode(_header.get{0}());
    }}

    /**
    * {2}
    **/
    public void set{0}({1} value)
    {{
        assert !isBuilt() : ""Can't modify an already built object!"";
        getBuilder().set{0}(value.getLocalMessageDescriptor());
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
@"    /**
    * {4}
    **/
    public {2} add{1}()
    {{
        assert !isBuilt() : ""Can't modify an already built object!"";

        {2} item = new {2}();
        item.setFieldId({3});
        item.setIndex(_{0}List.size());
        item.setParent(this);
        _{0}List.add(item);
        return item;
    }}

    /**
    * {4}
    **/
    public List<{2}> get{1}List()
    {{
        List<{2}> l = new ArrayList<{2}>();
        int n = get{1}Count();
        for(int i = 0; i < n; i++)
            l.add(get{1}(i));
        return l;
    }}

    /**
    * {4}
    **/
    public {2} get{1}(int index)
    {{
        if (index >= get{1}Count()) return null;

        {2} l{1} = _{0}List.get(index);
        if (l{1} == null)
        {{
            l{1} = {2}.parseFrom(getContentStream(), _header.get{1}(index));
            if (l{1}==null) 
                return null;
            l{1}.setFieldId({3});
            l{1}.setIndex(index);
            l{1}.setParent(this);
            _{0}List.set(index, l{1});
        }}
        return l{1};
    }}

    /**
    * {4}
    **/
    public int get{1}Count()
    {{
        return !isBuilt()? _{0}List.size() : _header.get{1}Count();
    }}
"
                                , field.name, field.name.Capitalize(), fieldType, field.id, field.description.Safe());
                return;
            }

            if (field.modifier == modifierType.optional)
            {
                Writer.WriteLine(
@"        /**
        * {4}
        **/
        public {2} add{1}()
        {{
            if (_{0} == null)
            {{
                if (isBuilt() && has{1}())
                {{
                    get{1}(); // decode from body
                }}
                else
                {{
                    assert !isBuilt() : ""Can't modify an already built object!"";
                    _{0} = new {2}();
                    _{0}.setFieldId({3});
                    _{0}.setParent(this);
                }}
            }}
            return _{0};
        }}

        /**
        * {4}
        **/
        public {2} get{1}()
        {{
            if (isBuilt() && has{1}() && _{0} == null)
            {{
                _{0} = {2}.parseFrom(getContentStream(), _header.get{1}());
                if (_{0}==null)
                    return null;
                _{0}.setFieldId({3});
                _{0}.setParent(this);
            }}
            return _{0};
        }}

        /**
        * {4}
        **/
        public boolean has{1}()
        {{
            return (isBuilt() && _header.has{1}()) || (!isBuilt() && _{0} != null);
        }}

        /**
        * {4}
        **/
        public void clear{1}()
        {{
            assert !isBuilt() : ""Can't modify an already built object!"";
            _{0} = null;
        }}
"
                    , field.name, field.name.Capitalize(), fieldType, field.id, field.description.Safe());
                return;
            }

            Writer.WriteLine(
@"        /**
        * {4}
        **/
        public {2} get{1}()
        {{
            if (_{0} == null)
            {{
                if (isBuilt())
                {{
                    _{0} = {2}.parseFrom(getContentStream(), _header.get{1}());
                    if (_{0}==null)
                        return null;
                }}
                else
                {{
                    _{0} = new {2}();
                }}
                _{0}.setFieldId({3});
                _{0}.setParent(this);
            }}
            return _{0};
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
@"        // Note: indexes are built during the build process, and aren't available before
        /**
        * {4}
        **/
        public List<{1}> get{0}List()
        {{
            assert isBuilt() : ""Index is not built yet!"";

            List<{1}> list = new ArrayList<{1}>();
            int count = get{0}Count();
            for(int i = 0; i < count; i++)
            {{
                list.add(get{0}(i));
            }}
            return list;
        }}

        /**
        * {4}
        **/
        public {1} get{0}(int index)
        {{
            assert isBuilt() : ""Index is not built yet!"";
            return ({1})getRoot().decode(_header.get{0}(index));
        }}

        /**
        * {4}
        **/
        public int get{0}Count()
        {{
            assert isBuilt() : ""Index is not built yet!"";
            return _header.get{0}Count();
        }}

        /**
        * {4}
        **/
        public {1} search{0}({2} item)
        {{
            assert isBuilt() : ""Index is not built yet!"";

            // note: we don't use std::find so that 
            // we decode a minimum number of items
            return search{0}(item, 0, get{0}Count() - 1);
        }}

        /**
        * {4}
        **/
        protected {1} search{0}({2} item, int min, int max)
        {{
            assert isBuilt() : ""Index is not built yet!"";

            if (max < min)
                return null;

            int avg = (min + max) >> 1;

            {1} candidate = get{0}(avg);
            {2} candidateKey = candidate.get{3}();
            if (candidateKey == item)
                return candidate;

            if (candidateKey < item)
                return search{0}(item, avg + 1, max);

            return search{0}(item, min, avg - 1);
        }}
"
                , index.name.Capitalize()
                , fieldType
                , sortByType
                , index.sortBy.Capitalize()
                , index.description.Safe()
                );
        }

        protected override void GenerateClassConstructor(messageType message)
        {
            Writer.WriteLine(
@"    
    /**
    * {1}
    **/
    protected Abstract{0}()
    {{
        _header = {0}Header.newBuilder();
        _positionInContent = -1;
        flush();
    }}

    /**
    * {1}
    **/
    protected Abstract{0}({0}Header header, int positionInContent)
    {{
        _header = header;
        _positionInContent = positionInContent;
        flush();
    }}
", message.name, message.description.Safe());
        }

        protected override void GeneratePrivateOrBuilderImpl(messageType message)
        {
            Writer.WriteLine(
@"    
    @Override
    public ProtoOrBuilder getRoot() {{ return {0}; }}
", message.IsRoot ? "this" : "_root");

            Writer.WriteLine(
@"    @Override
    public ProtoOrBuilder decode(LocalMessageDescriptor field)
    {
        return decode(field.getCoordinateList(), 0);
    }

    @Override
    public ProtoOrBuilder decode(List<Integer> coordinates, int index)
    {
        if (coordinates.isEmpty())
            return null;

        int fieldIdIdx = index;
        int fieldIndexIdx = index + 1;
        int remainderIdx = index + 2;
        switch(coordinates.get(fieldIdIdx))
        {");
            var allFields = message.field.Where(_ => _.type == typeType.nestedMessage || _.type == typeType.referenceMessage).OrderBy(_ => _.id);
            foreach (var field in allFields)
            {
                Writer.WriteLine(
@"            case {0}:", field.id);

                if (field.modifier == modifierType.repeated)
                {
                    Writer.WriteLine(
                        field.type != typeType.nestedMessage
                            ?
@"                return get{0}(coordinates.get(fieldIndexIdx));"
                            :
@"                return coordinates.size() == remainderIdx ? get{0}(coordinates.get(fieldIndexIdx)) : get{0}(coordinates.get(fieldIndexIdx)).decode(coordinates, remainderIdx);", 
                        field.name.Capitalize());
                }
                else if (field.modifier == modifierType.optional)
                {
                    Writer.WriteLine(
                        field.type != typeType.nestedMessage
                            ?
@"                  return get{0}();"
                            :
@"                  return coordinates.size() == remainderIdx ? add{0}() : get{0}().decode(coordinates, remainderIdx);", 
                        field.name.Capitalize());
                }
                else
                {
                    Writer.WriteLine(
                        field.type != typeType.nestedMessage
                            ?
@"                return get{0}();"
                            :
@"                return coordinates.size() == remainderIdx ? get{0}() : get{0}().decode(coordinates, remainderIdx);",
                        field.name.Capitalize());
                }
            }
            Writer.WriteLine(
@"            default:
                return null;
        }
    }

    @Override
    public void addCoordinates(List<Integer> coordinates)
    {");

            if (!message.IsRoot)
            {
                Writer.WriteLine(
@"        coordinates.add(0, getFieldId());
        coordinates.add(1, getIndex());

        if (getParent() == null)
            return;

        getParent().addCoordinates(coordinates);"
                );
            }

            Writer.WriteLine(
@"    }

    @Override
    public LocalMessageDescriptor getLocalMessageDescriptor()
    {
        List<Integer> coordinates = new ArrayList<Integer>();
        addCoordinates(coordinates);

        LocalMessageDescriptor.Builder b = LocalMessageDescriptor.newBuilder();
        b.addAllCoordinate(coordinates);
        return b.build();
    }
");
            Writer.WriteLine(
@"    protected {0}Header.Builder getBuilder() {{ return ({0}Header.Builder)_header; }}

    // ESCA-JAVA0029:
    public boolean isBuilt() {{ return !(_header instanceof {0}Header.Builder); }}
", message.name);
        }
        
        protected override void GenerateBuild(messageType message)
        {
            Writer.WriteLine(
@"
    public void preBuild()
    {
        // use this method to customize the build process
    }
");

            if (!message.IsRoot)
            {
                Writer.WriteLine(
@"    @Override
    public IIOStream getContentStream() 
    {
        return getRoot().getContentStream();
    }
");
            }
            else
            {
                Writer.WriteLine(
@"    @Override
    public IIOStream getContentStream() 
    {
        if (_contentStream == null)
            _contentStream = new ByteArrayIOStream();
        return _contentStream;
    }

    public void setContentStream(IIOStream contentStream)
    {
        assert _contentStream == null;
        _contentStream = contentStream;
    }
");
            }

            Writer.WriteLine(
@"    public void build() throws IOException
    {{
        build(getContentStream(), false);
    }}

    public void build(IIOStream content, boolean saveToOutput) throws IOException
    {{
        boolean alreadyBuilt = isBuilt();
        if (alreadyBuilt && !saveToOutput)
            return;

        // prebuild hook
        preBuild();

        {0}Header.Builder builder = null;
        if (alreadyBuilt)
        {{
            builder = {0}Header.newBuilder(({0}Header) _header);
        }}
        else
        {{
            builder = getBuilder();
        }}", message.name);

            foreach (var field in message.field.Where(_ => _.type == typeType.nestedMessage))
            {
                Writer.WriteLine(@"        builder.clear{0}();", field.name.Capitalize());
            }

            foreach (var index in message.index)
            {
                Writer.WriteLine(@"        builder.clear{0}();", index.name.Capitalize());
            }

            // do nothing for reference messages and pod types: the builder is already the owner of those fields

            foreach (var field in message.field.Where(_ => _.type == typeType.nestedMessage))
            {
                if (field.modifier == modifierType.repeated)
                {
                    Writer.WriteLine(
@"        
        List<{2}> tmp_{0}List = get{1}List();
        for ({2} l{0} : tmp_{0}List)
        {{
            int oldPos = l{0}.getPositionInContent();
            l{0}.build(content, saveToOutput);
            builder.add{1}(l{0}.getPositionInContent());
            if (alreadyBuilt || saveToOutput)
                l{0}.setPositionInContent(oldPos);
        }}"
                        , field.name, field.name.Capitalize(), FieldType(field));
                }
                else
                {
                    Writer.WriteLine(
@"
        {2} tmp_{0} = get{1}();
        if (tmp_{0} != null) 
        {{ 
            int oldPos = tmp_{0}.getPositionInContent();
            tmp_{0}.build(content, saveToOutput); 
            builder.set{1}(tmp_{0}.getPositionInContent());
            if (alreadyBuilt)
                tmp_{0}.setPositionInContent(oldPos);
        }}"
                        , field.name, field.name.Capitalize(), FieldType(field));
                }
            }

            // create indexes
            foreach (var index in message.index)
            {
                Writer.WriteLine(
@"
        Collections.sort(tmp_{0}List, new Comparator<{1}>() {{
			@Override
			public int compare({1} o1, {1} o2) {{
				if (o1.get{3}() == o2.get{3}())
					return 0;
				return o1.get{3}() < o2.get{3}() ? -1 : 1;
			}}
		}});
		
        for({1} l{0} : tmp_{0}List)
        {{
            builder.add{2}(l{0}.getLocalMessageDescriptor());
        }}"
                    , index.ReferenceField.name, index.ReferenceField.name.Capitalize(), index.name.Capitalize(), index.sortBy.Capitalize());
            }

            Writer.WriteLine(
@"
        // write the header
        OutputStream output = content.getOutputStream();

        // if we write to output, the position in the content stream
        // will be restored when writing the parent header
        // => this is not possible (and not needed) for root message
        boolean isRoot = {1};
        boolean dontSavePos = saveToOutput && isRoot;
        if (!dontSavePos)
            setPositionInContent(content.getPosition());

        {0}Header builtHeader = builder.build();
        builtHeader.writeDelimitedTo(output);
"
                , message.name, message.IsRoot ? "true" : "false");

            // write the message length at the end for later decoding
            // Note: the length is fixed
            if (message.IsRoot)
            {
                Writer.WriteLine(
@"        CodedOutputStream codedStream = CodedOutputStream.newInstance(output, Integer.SIZE/Byte.SIZE);
        codedStream.writeFixed32NoTag(builtHeader.getSerializedSize());
        codedStream.flush();
");
            }

            Writer.WriteLine(
@"        if (!alreadyBuilt && !saveToOutput)
        {
            _header = builtHeader;
            flush();
        }

        if (alreadyBuilt && saveToOutput)
        {
            flush();
        }
    }
");

            if (!message.IsRoot)
                return;

            Writer.WriteLine(
@"    public void writeDelimitedTo(IIOStream output) throws IOException
    {
        build(output, true);
    }

    public byte[] writeDelimitedToBytes()
    {
        try
        {
            ByteArrayIOStream stream = new ByteArrayIOStream();
            writeDelimitedTo(stream);
            return stream.toByteArray();
        } 
        catch (IOException e)
        {
            return new byte[0];
        }
    }
");
        }
        
        protected override void GenerateFlush(messageType message)
        {
            Writer.WriteLine(
@"    public void flush()
    {");

            var allFields = message.field.OrderBy(_ => _.id);

            var messageFields = from fld in allFields
                                where fld.messageType != null
                                && fld.type == typeType.nestedMessage
                                select fld;

            var has = false;
            foreach (var field in messageFields)
            {
                has = true;
                if (field.modifier == modifierType.repeated)
                    Writer.WriteLine(
@"        _{0}List = !isBuilt() ? new StretchableArray<{1}>() : new LazyArray<{1}>();", field.name, field.messageType);
                else
                    Writer.WriteLine(
@"        _{0} = null;", field.name);
            }

            if (!has)
                Writer.WriteLine(
@"        // NOP");

            Writer.WriteLine(
@"    }");
            Writer.WriteLine();
        }

        protected override void GenerateSerialization(messageType message)
        {
            if (message.IsRoot)
            {
                Writer.WriteLine(
@"    public static {0} parseFrom(IIOStream strm) throws IOException
    {{
        // the root is always at the end of the stream
        int size = Integer.SIZE/Byte.SIZE;
        int offset = -size;

        InputStream input = strm.getInputStreamAt(offset, IIOStream.E_SeekOrigin.End);

        int msgSize = CodedInputStream.newInstance(input).readFixed32();
        offset -= msgSize + CodedOutputStream.computeInt32SizeNoTag(msgSize);
            
        input = strm.getInputStreamAt(offset, IIOStream.E_SeekOrigin.End);
        int pos = strm.getPosition();

        {0} new{0} = new {0}();
        new{0}.setContentStream(strm);
        new{0}.setPositionInContent(pos);
        new{0}._header = {0}Header.parseDelimitedFrom(input);
        return new{0};
    }}

    public static {0} parseFrom(byte[] bytes)
    {{
        try
        {{
            return parseFrom(new ByteArrayIOStream(bytes));
        }}
        catch(IOException e)
        {{
            return null;
        }}
    }}
", message.name);
                return;
            }

            Writer.WriteLine(
@"    public static {0} parseFrom(IIOStream strm, int pos)
    {{
        try
        {{
            InputStream input = strm.getInputStreamAt(pos);

            {0}Header header = {0}Header.parseDelimitedFrom(input);
            assert header != null : ""Can't decode header!"";

            {0} parsed = new {0}(header, pos);
            return parsed;
        }} 
        catch (IOException ex)
        {{
            assert false : ""Can't decode header!"";
            return null;
        }}
    }}
", message.name);
        }
        
        protected override void GenerateEqualsAndHashCode(messageType message)
        {
            if (!message.IsRoot)
            {
                Writer.WriteLine(
@"    @Override
    public boolean equals(Object other)
    {{
        if (other == null)
            return false;

        if (!other.getClass().equals(getClass()))
            return false;

        Abstract{0} that = (Abstract{0})other;

        boolean result = (_fieldId == that._fieldId) && (_index == that._index);

        if(!result)
            return false;

        if (_parent == null)
            return that._parent == null;

        return _parent.equals(that._parent);
    }}

    @Override
    public int hashCode()
    {{
        int hashCode = 17;
        if (_parent != null)
            hashCode = 31 * hashCode + _parent.hashCode();
        hashCode = 31 * hashCode + _fieldId;
        hashCode = 31 * hashCode + _index;
        return hashCode;
    }}
", message.name);
                return;
            }

            var fields = message.field.Where(_ => _.modifier == modifierType.required).OrderBy(_ => _.id).ToList();

            Writer.Write(
@"    @Override
    public boolean equals(Object other)
    {{
        if (other == null)
            return false;

        if (!other.getClass().equals(getClass()))
            return false;

        Abstract{0} that = (Abstract{0})other;

        return "
, message.name);
            var first = true;
            foreach (var field in fields)
            {
                if (!first) Writer.Write(" && ");

                if (HasBoxedType(field))
                    Writer.Write(
@"((({1})get{0}()).equals((({1})that.get{0}())))", field.name.Capitalize(), BoxedType(field, ""));
                else
                {
                    Writer.Write(
@"(get{0}().equals(that.get{0}()))", field.name.Capitalize());
                }
                first = false;
            }

            if (first)
                Writer.Write("true");

            Writer.WriteLine(";");

            Writer.WriteLine(
@"
    }

    @Override
    public int hashCode()
    {
        int hashCode = 17;");

            foreach (var field in fields)
            {
                if (HasBoxedType(field))
                    Writer.WriteLine(
@"        hashCode = 31 * hashCode + (({1})get{0}()).hashCode();", field.name.Capitalize(), BoxedType(field, ""));
                else
                {
                    Writer.WriteLine(
@"        hashCode = 31 * hashCode + get{0}().hashCode();", field.name.Capitalize());
                }
            }
            Writer.WriteLine(
@"
        return hashCode;
    }
");
        }

        protected override void GenerateToString(messageType message)
        {
            Writer.WriteLine(
@"    @Override
    public String toString()
    {
        String ret = System.getProperty(""line.separator"");
        return toString(new BaseFormat(0,ret));
    }

    @Override
    public String toString(IFormat format)
    {
        StringBuilder bd = new StringBuilder();");
            Writer.WriteLine(
@"        format.formatHeader(bd,""{0}"");
", message.name);

            var fields = message.field.OrderBy(_ => _.id);

            foreach (var field in fields)
            {
                switch (field.modifier)
                {
                    case modifierType.repeated:
                        Writer.WriteLine(
@"        format.formatField(bd,""{0}"", get{1}List());
", field.name, field.name.Capitalize());
                        break;
                    case modifierType.optional:
                        Writer.WriteLine(
@"        format.formatField(bd,""{0}"", has{0}() ? get{0}() : null);
", field.name.Capitalize());
                        break;
                    default:
                        Writer.WriteLine(
@"        format.formatField(bd,""{0}"", get{0}());", field.name.Capitalize());
                        break;
                }
            }

            Writer.WriteLine(
@"        format.formatFooter(bd);
        return bd.toString();
    }
");
        }
        
        private void GenerateImports(messageType message)
        {
            Writer.WriteLine(
@"import java.util.List;
import java.util.ArrayList;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;");

            if (message.field.Any(_ => _.modifier == modifierType.repeated))
            {
                Writer.WriteLine(
@"import java.util.ArrayList;
import java.util.List;
import {0}.IStretchableArray;
import {0}.StretchableArray;", ResourceNamespace);
            }

            if (message.IsRoot)
            {
                Writer.WriteLine(@"import com.google.protobuf.CodedInputStream;
import com.google.protobuf.CodedOutputStream;
import {0}.ByteArrayIOStream;", ResourceNamespace);
            }

            if (message.index.Any())
            {
                Writer.WriteLine(@"import java.util.Collections;
import java.util.Comparator;");
            }

            Writer.WriteLine(
@"import {0}.{1}.LocalMessageDescriptor;
import {0}.{1}.{2}Header;
import {0}.{1}.{2}HeaderOrBuilder;
import {3}.{2};
import {4}.IFormat;
import {4}.IIOStream;
import {4}.LazyArray;
", GeneratedNamespace, DocumentName, message.name, Namespace, ResourceNamespace);

            var enums = (from fieldType f in message.field
                         where f.type == typeType.@enum
                         select f.enumType).Distinct();

            foreach (var e in enums)
                Writer.WriteLine(
@"import {0}.{1}.{2};", GeneratedNamespace, DocumentName, e);

            var nesteds = (from fieldType f in message.field
                           where f.type == typeType.nestedMessage && f.messageType != message.name
                           select f.messageType).Distinct();

            var references = (from fieldType f in message.field
                              where f.type == typeType.referenceMessage && f.messageType != message.name
                              select f.messageType).Distinct();

            foreach (var e in nesteds.Union(references))
                Writer.WriteLine(
@"import {0}.{1};", Namespace, e);

            Writer.WriteLine();
        }
        
        private static string FieldType(fieldType node)
        {
            switch (node.type)
            {
                case typeType.nestedMessage:
                    return node.messageType;
                case typeType.referenceMessage:
                    return node.messageType;
                case typeType.@enum:
                    return node.enumType;
                case typeType.uint32:
                    return "int";
                case typeType.int32:
                    return "int";
                case typeType.@string:
                    return "String";
                case typeType.@bool:
                    return "boolean";
                default:
                    return node.type.ToString();
            }
        }

        private static string BoxedType(fieldType node, string suffix)
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
                    return "Integer";
                case typeType.int32:
                    return "Integer";
                case typeType.@string:
                    return "String";
                case typeType.@bool:
                    return "Boolean";
                default:
                    return node.type.ToString();
            }
        }

        private static bool HasBoxedType(fieldType node)
        {
            switch (node.type)
            {
                case typeType.uint32:
                case typeType.int32:
                case typeType.@bool:
                    return true;
                default:
                    return false;
            }
        }

        internal override void GenerateFinalClientClass(messageType message)
        {
            var filePath = GetFilePath(OutputFolder, message.name + ".java", Namespace);

            if (File.Exists(filePath)) return;

            using (var strm = GetStreamFromPath(filePath))
            {
                strm.WriteLine(
@"package {2};

import {1}.Abstract{0};
import {1}.{3}.{0}Header;

/**
*
*/
public class {0} extends Abstract{0}
{{
    /**
     * 
     */
    public {0}() 
    {{
        // NOP
    }}

    /**
     * @param header
     *            header
     * @param posInContent
     *            position in content
     */
    public {0}({0}Header header, int posInContent) 
    {{ 
        super(header, posInContent); 
    }}
}}"
                        , message.name, GeneratedNamespace, Namespace, DocumentName);
            }
        }
    }
}

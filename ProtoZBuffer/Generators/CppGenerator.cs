using System;
using System.IO;
using System.Linq;

namespace ProtoZBuffer.Generators
{
    class CppGenerator : AbstractGenerator
    {
        protected override string NamespaceSeparator
        {
            get { return "::"; }
        }

        protected override string ResourceFolder
        {
            get { return "cpp"; }
        }

        protected override string ResourceNamespace
        {
            get { return Namespace + "::Common"; }
        }

        private string IncludesFolder
        {
            get { return Path.Combine(CppFolder, "include"); }
        }

        private string CppFolder
        {
            get { return OutputFolder; }
        }

        private TextWriter IncludeWriter { get; set; }
        private TextWriter CppWriter { get; set; }

        protected override void InstallResources()
        {
            SafeDirectoryCreation(CppFolder);
            SafeDirectoryCreation(IncludesFolder);

            var assembly = GetType().Assembly;
            CopyResourceToOutput(assembly, "ArrayList.h", IncludesFolder, ResourceNamespace);
            CopyResourceToOutput(assembly, "Util.h", IncludesFolder, ResourceNamespace);
            CopyResourceToOutput(assembly, "Util.cpp", CppFolder, ResourceNamespace);
        }

        protected override string ProtocCommandLine
        {
            get
            {
                var outputFolder = Path.Combine(IncludesFolder, GetNamespacePath(GeneratedNamespace));
                SafeDirectoryCreation(outputFolder);

                var cmd = Path.Combine(ProtoGenFolder, "protoc.exe"); // path to protoc.exe
                cmd += string.Format(" --proto_path=\"{0}\"", Path.GetDirectoryName(ProtoFile)); // where to search the .proto file
                cmd += string.Format(" --cpp_out=\"{0}\"", outputFolder); // where to output the generated protobuf files
                cmd += string.Format(" \"{0}\"", ProtoFile);
                return cmd;
            }
        }

        protected override string ReplaceNamespaceInContent(string content, string nspace)
        {
            return content
                .Replace("%NAMESPACE_BEGIN%", GetNamespaceBegin(nspace))
                .Replace("%NAMESPACE_END%", GetNamespaceEnd(nspace))
                .Replace("%NAMESPACE_PATH%", GetNamespacePath(nspace))
                ;
        }

        // from my::name::space, return namespace my { namespace name { namespace space {
        private string GetNamespaceBegin(string nspace)
        {
            var nspaces = nspace.Split(new [] { NamespaceSeparator }, StringSplitOptions.RemoveEmptyEntries);
            if (nspaces.Length == 0)
                return "";
            return "namespace " + string.Join(" { namespace ", nspaces) + Environment.NewLine + "{";
        }

        // returns as many closing curly braces as nested namespaces
        private string GetNamespaceEnd(string nspace)
        {
            var nspaces = nspace.Split(new [] { NamespaceSeparator }, StringSplitOptions.RemoveEmptyEntries);
            return new string('}', nspaces.Length);
        }

        // from my::name::space, return  my<s>name<s>space where <s> is the platform specific directory separator
        private string GetNamespacePath(string nspace)
        {
            return InternalGetNamespacePath(nspace, Path.DirectorySeparatorChar);
        }
        // from my::name::space, return  my/name/space
        private string GetNamespacePathSlash(string nspace)
        {
            return InternalGetNamespacePath(nspace, '/');
        }
        // from my::name::space, return  my<s>name<s>space where <s> is the provided directory separator
        private string InternalGetNamespacePath(string nspace, char directorySeparator)
        {
            return nspace.Replace(NamespaceSeparator, directorySeparator.ToString());
        }

        protected override bool GenerateLazyImplementation(protozbuffType p)
        {
            var baseName = DocumentName + ".lazy";
            IncludeWriter = GetStream(IncludesFolder, baseName + ".h", GeneratedNamespace);
            CppWriter = GetStream(CppFolder, baseName + ".cpp", GeneratedNamespace);

            WriteAutoGenerationWarning(IncludeWriter);
            WriteAutoGenerationWarning(CppWriter);

            IncludeWriter.WriteLine(@"
#pragma once
#include <memory>
#include <sstream>
#include <string>
#include ""{1}.pb.h""
#include ""{0}/ArrayList.h""
#include ""{0}/Util.h""
", GetNamespacePathSlash(ResourceNamespace), DocumentName);

            // forward declaration for final client classes
            IncludeWriter.WriteLine(GetNamespaceBegin(Namespace));
            foreach (var message in p.Items.OfType<messageType>())
            {
                IncludeWriter.WriteLine(@"class {0};", message.name.Capitalize());
            }
            IncludeWriter.WriteLine(GetNamespaceEnd(Namespace));
            IncludeWriter.WriteLine();
            IncludeWriter.WriteLine(GetNamespaceBegin(GeneratedNamespace));

            CppWriter.WriteLine("#include <stdafx.h>");
            CppWriter.WriteLine("#include <sstream>");
            CppWriter.WriteLine("#include <{0}/{1}.h>", GetNamespacePathSlash(GeneratedNamespace), baseName);
            CppWriter.WriteLine("#include <{0}/Util.h>", GetNamespacePathSlash(ResourceNamespace));
            foreach (var message in p.Items.OfType<messageType>())
            {
                CppWriter.WriteLine(@"#include <{0}/{1}.h>", GetNamespacePathSlash(Namespace), message.name.Capitalize());
            }

            CppWriter.WriteLine(@"using namespace {0};", ResourceNamespace);
            CppWriter.WriteLine(GetNamespaceBegin(GeneratedNamespace));

            return base.GenerateLazyImplementation(p);
        }

        internal override void GenerateProtoOrBuilderInterface()
        {
            IncludeWriter.WriteLine(
@"    typedef ::google::protobuf::RepeatedField< ::google::protobuf::int32 > CoordinateList;
    class BaseFormat; 
    class ProtoOrBuilder
    {
    public:
        virtual ProtoOrBuilder* getRoot() = 0;
        virtual void addCoordinates(CoordinateList& coordinates) = 0;
        virtual LocalMessageDescriptor getLocalMessageDescriptor() = 0;
        virtual ProtoOrBuilder* decode(const CoordinateList& coordinates, int index) = 0;
        virtual ProtoOrBuilder* decode(const LocalMessageDescriptor& field) = 0;
        virtual std::iostream& contentStream() = 0;
        virtual std::string toString();
        virtual std::string toString(BaseFormat& format) = 0;
        virtual ~ProtoOrBuilder();
    };

    std::ostream& operator<<(std::ostream& out, ProtoOrBuilder& proto);
");

            CppWriter.WriteLine(
@"        std::string ProtoOrBuilder::toString()
        {
            auto format = BaseFormat();
            return toString(format);
        }
        
        ProtoOrBuilder::~ProtoOrBuilder()
        {
        }

        std::ostream& operator<<(std::ostream& out, ProtoOrBuilder& proto)
        {
            out << proto.toString();
            return out;
        }
");
        }

        internal override void GenerateToStringFormatters()
        {
            IncludeWriter.WriteLine(
@"    class BaseFormat
    {
    public:
        int m_indentation;
        int getIndentation() const { return m_indentation; }
        void setIndentation(int indentation) { m_indentation = indentation; }

        std::string getTabulations() const { return std::string(m_indentation, '\t'); }

        void formatHeader(std::ostringstream& builder, const std::string& title)
        {
            builder << getTabulations() << title << std::endl;
            builder << getTabulations() << ""{"" << std::endl;
            m_indentation++;
        }

        void formatFooter(std::ostringstream& builder)
        {
            m_indentation--;
            builder << getTabulations() << ""}"" << std::endl;
        }
    
        template<typename T>
        void formatField(std::ostringstream& builder, const std::string& name, T&& obj, bool has = true)
        {
            if (!has) return;
            builder << getTabulations() << name << "": "";
            if (has)
                builder << obj;
            else
                builder << ""not set"";
            builder << std::endl;
        }

        void formatProtoField(std::ostringstream& builder, const std::string& name, ProtoOrBuilder& obj, bool has = true)
        {
            if (!has) return;
            formatHeader(builder, name);
            builder << (has ? obj.toString(*this) : getTabulations() + ""null"") << std::endl;
            formatFooter(builder);
        }

        void formatProtoField(std::ostringstream& builder, const std::string& name, ProtoOrBuilder* obj, bool has = true)
        {
            if (!has || obj == nullptr) return;
            formatProtoField(builder, name, * obj, has && obj != nullptr);
        }
    
        template<typename T>
        void formatListField(std::ostringstream& builder, const std::string& name, const std::vector<T>& objs)
        {
            if (objs.empty()) return;

            builder << getTabulations() << name << "": "";
            std::copy(objs.begin(), objs.end(), std::ostream_iterator<T>(builder, "", ""));
            builder << std::endl;
        }

        template<typename T>
        void formatProtoField(std::ostringstream& builder, const std::string& name, const std::vector<T*>& objs)
        {
            if (objs.empty()) return;

            builder << getTabulations() << name << "": "" << std::endl;
            for (auto* obj : objs)
            {
                builder << obj->toString(*this);
            }
            builder << std::endl;
        }
    };
");
        }

        internal override void GenerateHeaderOrBuilderInterface(messageType message)
        {
            // NOP: C++ doesn't have interfaces 
        }

        protected override void InitializeAbstractClass(messageType message)
        {
            IncludeWriter.WriteLine(
@"    class Abstract{0} : public ProtoOrBuilder
    {{"
                , message.name);
        }

        protected override void EndAbstractClass(messageType message)
        {
            IncludeWriter.WriteLine(
@"    };
");
        }

        protected override void GenerateClassFields(messageType message)
        {
            if (!message.IsRoot)
            {
                IncludeWriter.WriteLine(
@"    private: 
        ProtoOrBuilder* m_root;
        ProtoOrBuilder* m_parent;
        int m_fieldId = -1; // field's ID as defined in the protozbuf.xml file (=> the .proto file)
        int m_index = -1; // instance's index in the parent's list

    public: 
        ProtoOrBuilder* getParent() { return m_parent; }
        void setParent(ProtoOrBuilder* parent);

        int fieldId() const { return m_fieldId; }
        void setFieldId(int fieldId) { m_fieldId = fieldId; }
        int index() const { return m_index; }
        void setIndex(int index) { m_index = index; }
");

            CppWriter.WriteLine(
@"        void Abstract{0}::setParent(ProtoOrBuilder* parent)
        {{
            m_parent = parent;
            m_root = parent->getRoot();
        }}", message.name);
            }
            else
            {
                IncludeWriter.WriteLine(
    @"    protected: 
        std::shared_ptr<std::iostream> m_contentStream;");
            }

            IncludeWriter.WriteLine(
@"    protected: 
        {0}Header m_header;
        long m_positionInContent;

    public:
        int positionInContent() const {{ return m_positionInContent; }}
        void setPositionInContent(int positionInContent) {{ m_positionInContent = positionInContent; }}

    protected: "
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
                        IncludeWriter.WriteLine(
@"        {2}::ArrayList<{0}> m_{1}List;", fieldtype, field.name, ResourceNamespace);
                        break;
                    default:
                        IncludeWriter.WriteLine(
@"        std::unique_ptr<{0}> m_{1};", fieldtype, field.name);
                        break;
                }
            }
        }

        protected override void GenerateClassSimpleField(messageType message, fieldType field)
        {
            var fieldTypeForList = FieldType(field);
            var fieldType = fieldTypeForList;
            if (field.type == typeType.@string)
                fieldType = "const " + fieldType + "&";

            if (field.modifier == modifierType.repeated)
            {
                IncludeWriter.WriteLine(
@"        void add{1}({2} item);
        void remove{1}({2} item);
        std::vector<{3}> {0}List();
        {2} get{1}(int index);
        int {0}Count() const;
"
                    , field.name, field.name.Capitalize(), fieldType, fieldTypeForList);

                CppWriter.WriteLine(
@"        void Abstract{4}::add{1}({3} item)
        {{
            assert((""Can't modify an already built object!"", !isBuilt()));
            m_header.add_{2}(item);
        }}

        void Abstract{4}::remove{1}({3} item)
        {{
            assert((""Can't modify an already built object!"", !isBuilt()));
            auto kepts = std::vector<{5}>();
            auto& originals = m_header.{2}();
            std::copy_if(originals.begin(), originals.end(), std::back_inserter(kepts), [&item]({3} check) {{ return check != item; }});
            m_header.clear_{2}();
            std::for_each(kepts.begin(), kepts.end(), [this]({3} item) {{ add{1}(item); }});
        }}

        std::vector<{5}> Abstract{4}::{0}List()
        {{
            auto& originals = m_header.{2}();
            return std::vector<{5}>(originals.begin(), originals.end());
        }}

        {3} Abstract{4}::get{1}(int index)
        {{
            return m_header.{2}(index);
        }}

        int Abstract{4}::{0}Count() const
        {{
            return m_header.{2}_size();
        }}
"
                    , field.name, field.name.Capitalize(), field.name.ToLowerInvariant(), fieldType, message.name, fieldTypeForList);
                return;
            }

            if (field.modifier == modifierType.optional)
            {
                IncludeWriter.WriteLine(
@"        bool has{0}() const {{ return m_header.has_{1}(); }}        
        void clear{0}() {{ m_header.clear_{1}(); }}
"
                    , field.name.Capitalize(), field.name.ToLowerInvariant());
            }

            IncludeWriter.WriteLine(
@"        {3} {0}() const {{ return m_header.{2}(); }}
        void set{1}({3} value) {{ m_header.set_{2}(value); }}
"
                , field.name, field.name.Capitalize(), field.name.ToLowerInvariant(), fieldType);
        }

        protected override void GenerateClassReferenceField(messageType message, fieldType field)
        {
            var fieldType = FieldType(field);

            if (field.modifier == modifierType.repeated)
            {
                IncludeWriter.WriteLine(
@"        void add{1}({2}& item);
        void remove{1}({2}& item);
        std::vector<{2}*> {0}List();
        {2}& get{1}(int index);
        int {0}Count() const;
"
                    , field.name, field.name.Capitalize(), fieldType);

                CppWriter.WriteLine(
@"        void Abstract{4}::add{1}({3}& item)
        {{
            assert((""Can't modify an already built object!"", !isBuilt()));
            m_header.add_{2}()->CopyFrom(item.getLocalMessageDescriptor());
        }}

        void Abstract{4}::remove{1}({3}& item)
        {{
            assert((""Can't modify an already built object!"", !isBuilt()));
            auto kepts = std::vector<LocalMessageDescriptor>();
            auto& originals = m_header.{2}();
            std::copy_if(originals.begin(), originals.end(), std::back_inserter(kepts), [this, &item](const LocalMessageDescriptor& check) 
            {{ 
                return getRoot()->decode(check) != &item; 
            }});
            m_header.clear_{2}();
            std::for_each(kepts.begin(), kepts.end(), [this](LocalMessageDescriptor& descriptor) {{ m_header.add_{2}()->CopyFrom(descriptor); }});
        }}

        std::vector<{3}*> Abstract{4}::{0}List()
        {{
            auto count = {0}Count();
            auto list = std::vector<{3}*>();
            list.reserve(count);
            for (auto i = 0; i < count; ++i)
            {{
                list.push_back(&get{1}(i));
            }}
            return list;
        }}

        {3}& Abstract{4}::get{1}(int index)
        {{
            return *dynamic_cast<{3}*>(getRoot()->decode(m_header.{2}(index)));
        }}

        int Abstract{4}::{0}Count() const
        {{
            return m_header.{2}_size();
        }}
"
                    , field.name, field.name.Capitalize(), field.name.ToLowerInvariant(), fieldType, message.name);
                return;
            }

            if (field.modifier == modifierType.optional)
            {
                IncludeWriter.WriteLine(
@"        bool has{0}() const {{ return m_header.has_{1}(); }}
        void clear{0}() {{ m_header.clear_{1}(); }}
"
                    , field.name.Capitalize(), field.name.ToLowerInvariant());
            }

            IncludeWriter.WriteLine(
@"        void set{1}({2}& value);
        {2}* {0}();
"
                , field.name, field.name.Capitalize(), fieldType);

            CppWriter.WriteLine(
@"        void Abstract{4}::set{1}({3}& value)
        {{
            m_header.clear_{2}();
            m_header.mutable_{2}()->CopyFrom(value.getLocalMessageDescriptor());
            // TODO see if that works // m_header.set_allocated_{2}(value.getLocalMessageDescriptor());
        }}

        {3}* Abstract{4}::{0}()
        {{
            return dynamic_cast<{3}*>(getRoot()->decode(m_header.{2}()));
        }}
"
                , field.name, field.name.Capitalize(), field.name.ToLowerInvariant(), fieldType, message.name);
        }

        protected override void GenerateClassNestedField(messageType message, fieldType field)
        {
            var fieldType = FieldType(field);
            if (field.modifier == modifierType.repeated)
            {
                IncludeWriter.WriteLine(
@"        {2}& add{1}();
        std::vector<{2}*> {0}List();
        {2}& get{1}(int index);
        int {0}Count() const;
"
                    , field.name, field.name.Capitalize(), fieldType);

                CppWriter.WriteLine(
@"        {3}& Abstract{4}::add{1}()
        {{
            assert((""Can't modify an already built object!"", !isBuilt()));
            auto index = m_{0}List.size();
            auto {0} = std::make_unique<{3}>();
            {0}->setFieldId({5});
            {0}->setIndex(index);
            {0}->setParent(this);
            m_{0}List.set(index, std::move({0}));
            return *m_{0}List.get(index);
        }}

        std::vector<{3}*> Abstract{4}::{0}List()
        {{
            auto count = {0}Count();
            auto list = std::vector<{3}*>();
            list.reserve(count);
            for (auto i = 0; i < count; ++i)
            {{
                list.push_back(&get{1}(i));
            }}
            return list;
        }}

        {3}& Abstract{4}::get{1}(int index)
        {{
            if (!m_{0}List.get(index))
            {{
                auto {0} = {3}::ParseFrom(contentStream(), m_header.{2}(index));
                {0}->setFieldId({5});
                {0}->setIndex(index);
                {0}->setParent(this);
                m_{0}List.set(index, std::move({0}));
            }}
            return *m_{0}List.get(index);
        }}

        int Abstract{4}::{0}Count() const
        {{
            return !isBuilt() ? m_{0}List.size() : m_header.{2}_size();
        }}"
                , field.name, field.name.Capitalize(), field.name.ToLowerInvariant(), fieldType, message.name, field.id);
                return;
            }

            if (field.modifier == modifierType.optional)
            {
                IncludeWriter.WriteLine(
@"        {2}& add{1}(); // adds the optional {1}
        {2}* {0}();
        bool has{1}() const;
        void clear{1}();
        
"
                    , field.name, field.name.Capitalize(), fieldType);

                CppWriter.WriteLine(
@"        {3}& Abstract{4}::add{1}()
        {{
            if (m_{0} == nullptr)
            {{
                if (isBuilt() && has{1}())
                {{
                    {0}(); // decode from body
                }}
                else
                {{
                    assert((""Can't modify an already built object!"", !isBuilt()));
                    m_{0} = std::make_unique<{6}::{3}>();
                    m_{0}->setFieldId({5});
                    m_{0}->setParent(this);
                }}
            }}
            return *m_{0};
        }}

        {3}* Abstract{4}::{0}()
        {{
            if (isBuilt() && has{1}() && m_{0} == nullptr)
            {{
                m_{0} = {3}::ParseFrom(contentStream(), m_header.{2}());
                m_{0}->setFieldId({5});
                m_{0}->setParent(this);
            }}
            return m_{0}.get();
        }}

        bool Abstract{4}::has{1}() const
        {{
            return (isBuilt() && m_header.has_{2}()) || (!isBuilt() && m_{0} != nullptr);
        }}

        void Abstract{4}::clear{1}()
        {{
            assert((""Can't modify an already built object!"", !isBuilt()));
            m_{0}.reset();
        }}
"
                    , field.name, field.name.Capitalize(), field.name.ToLowerInvariant(), fieldType, message.name, field.id, Namespace);
                return;
            }

            IncludeWriter.WriteLine(@"        {1}& {0}();", field.name, fieldType);
            CppWriter.WriteLine(
@"        {2}& Abstract{3}::{0}()
        {{
            if (m_{0} == nullptr)
            {{
                if (isBuilt())
                {{
                    m_{0} = {2}::ParseFrom(contentStream(), m_header.{1}());
                }}
                else
                {{
                    m_{0} = std::make_unique<{2}>();
                }}
                m_{0}->setFieldId({4});
                m_{0}->setParent(this);
            }}
            return *m_{0};
        }}
"
                , field.name, field.name.ToLowerInvariant(), fieldType, message.name, field.id);
        }

        protected override void GenerateClassIndex(messageType message, indexType index)
        {
            // indexes are build at build time
            var field = index.ReferenceField;
            var fieldType = FieldType(field);
            var sortByType = FieldType(index.SortingField);
            if (index.SortingField.type == typeType.@string)
                sortByType = "const " + sortByType + "&";

            IncludeWriter.WriteLine(
@"        // Note: indexes are built during the build process, and aren't available before
        std::vector<{2}*> {0}List();
        {2}& get{1}(int index);
        int {0}Count() const;
        {2}* search{1}({3} item);
        {2}* search{1}({3} item, int min, int max);
"
                , index.name, index.name.Capitalize(), fieldType, sortByType);

            CppWriter.WriteLine(
@"        std::vector<{3}*> Abstract{4}::{0}List()
        {{
            assert((""Index is not built yet!"", isBuilt()));

            auto count = {0}Count();
            auto list = std::vector<{3}*>();
            list.reserve(count);
            for (auto i = 0; i < count; ++i)
            {{
                list.push_back(&get{1}(i));
            }}
            return list;
        }}

        {3}& Abstract{4}::get{1}(int index)
        {{
            assert((""Index is not built yet!"", isBuilt()));
            return *dynamic_cast<{3}*>(getRoot()->decode(m_header.{2}(index)));
        }}

        int Abstract{4}::{0}Count() const
        {{
            assert((""Index is not built yet!"", isBuilt()));
            return m_header.{2}_size();
        }}

        {3}* Abstract{4}::search{1}({5} item)
        {{
            assert((""Index is not built yet!"", isBuilt()));

            // note: we don't use std::find so that 
            // we decode a minimum number of items
            return search{1}(item, 0, {0}Count() - 1);
        }}

        {3}* Abstract{4}::search{1}({5} item, int min, int max)
        {{
            assert((""Index is not built yet!"", isBuilt()));

            if (max < min)
                return nullptr;

            int avg = (min + max) >> 1;

            auto& candidate = get{1}(avg);
            {5} candidateKey = candidate.{6}();
            if (candidateKey == item)
                return &candidate;

            if (candidateKey < item)
                return search{1}(item, avg + 1, max);

            return search{1}(item, min, avg - 1);
        }}
"
                , index.name, index.name.Capitalize(), index.name.ToLowerInvariant(), fieldType, message.name, sortByType, index.sortBy);
        }

        protected override void GenerateClassConstructor(messageType message)
        {
            IncludeWriter.WriteLine(
@"    protected: 
        Abstract{0}();
        Abstract{0}(const {0}Header& header, int posInContent);

        Abstract{0}(const Abstract{0}&) = delete;
        Abstract{0}& operator=(const Abstract{0}&) = delete;
", message.name);

            CppWriter.WriteLine(
@"        
        Abstract{0}::Abstract{0}() : {1} m_positionInContent(-1)
        {{
        }}        

        Abstract{0}::Abstract{0}(const {0}Header& header, int pos) : {1} m_header(header), m_positionInContent(pos)
        {{
        }}

        ", message.name, message.IsRoot ? "" : "m_parent(nullptr), m_root(nullptr),");
        }

        protected override void GeneratePrivateOrBuilderImpl(messageType message)
        {
            IncludeWriter.WriteLine(@"    public:
");

            IncludeWriter.Write("        ProtoOrBuilder* getRoot() override { return ");
            IncludeWriter.Write(message.IsRoot ? @"this" : "m_root");
            IncludeWriter.WriteLine("; }");

            IncludeWriter.WriteLine(@"        ProtoOrBuilder* decode(const LocalMessageDescriptor& field) override;
        ProtoOrBuilder* decode(const CoordinateList& coordinates, int index) override;");
            CppWriter.WriteLine(
@"
        ProtoOrBuilder* Abstract{0}::decode(const LocalMessageDescriptor& field)
        {{
            return decode(field.coordinate(), 0);
        }}

        ProtoOrBuilder* Abstract{0}::decode(const CoordinateList& coordinates, int index)
        {{
            if (coordinates.size() == 0)
                return nullptr;
            
            auto fieldIdIdx = index;
            auto fieldIndexIdx = index + 1;
            auto remainderIdx = index + 2;
            switch(coordinates.Get(fieldIdIdx))
            {{", message.name);
            var allFields = message.field.Where(_ => _.type == typeType.nestedMessage || _.type == typeType.referenceMessage).OrderBy(_ => _.id);
            foreach (var field in allFields)
            {
                CppWriter.WriteLine(
@"              case {0}:", field.id);

                if (field.modifier == modifierType.repeated)
                {
                    CppWriter.WriteLine(
                        field.type != typeType.nestedMessage
                            ?
@"                  return &this->get{0}(coordinates.Get(fieldIndexIdx));"
                            :
@"                  return coordinates.size() == remainderIdx ? &this->get{0}(coordinates.Get(fieldIndexIdx)) : this->get{0}(coordinates.Get(fieldIndexIdx)).decode(coordinates, remainderIdx);",
                        field.name.Capitalize());
                }
                else if (field.modifier == modifierType.optional)
                {
                    CppWriter.WriteLine(
                        field.type != typeType.nestedMessage
                            ?
@"                  return this->{0}();"
                            :
@"                  return coordinates.size() == remainderIdx ? &this->add{1}() : this->{0}()->decode(coordinates, remainderIdx);",
                        field.name, field.name.Capitalize());
                }
                else
                {
                    CppWriter.WriteLine(
                        field.type != typeType.nestedMessage
                            ?
@"                  return this->{0}();"
                            :
@"                  return coordinates.size() == remainderIdx ? &this->{0}() : this->{0}().decode(coordinates, remainderIdx);",
                        field.name);
                }
            }
            CppWriter.WriteLine(
@"              default:
                  return nullptr;
            }
        }
");
            IncludeWriter.WriteLine(@"        void addCoordinates(CoordinateList& coordinates) override;");
            CppWriter.WriteLine(
@"        void Abstract{0}::addCoordinates(CoordinateList& coordinates)
        {{", message.name);

            if (!message.IsRoot)
            {
                CppWriter.WriteLine(
@"            
            coordinates.Add(0);
            coordinates.Add(0);
            for (auto i = coordinates.size() - 1; i >= 2; --i)
            {
                coordinates.Set(i, coordinates.Get(i - 2));
            }

            coordinates.Set(0, fieldId());
            coordinates.Set(1, index());

            if (getParent() == nullptr)
                return;

            getParent()->addCoordinates(coordinates);");
            }

            CppWriter.WriteLine(@"        }
");

            IncludeWriter.WriteLine(@"        LocalMessageDescriptor getLocalMessageDescriptor() override;");

            CppWriter.WriteLine(
@"        LocalMessageDescriptor Abstract{0}::getLocalMessageDescriptor()
        {{
            auto b = LocalMessageDescriptor();
            addCoordinates(*b.mutable_coordinate());
            return b;
        }}", message.name);

            IncludeWriter.WriteLine(@"        bool isBuilt() const { return m_positionInContent != -1; }");
        }

        protected override void GenerateBuild(messageType message)
        {
            IncludeWriter.WriteLine(
@"        virtual void preBuild() {} // use this method to customize the build process
        std::iostream& contentStream() override;
        void build();
        void build(std::ostream& output, bool saveToOutput);
");

            if(!message.IsRoot)
            {
                CppWriter.WriteLine(
        @"        
        std::iostream& Abstract{0}::contentStream() 
        {{
            return getRoot()->contentStream();
        }}", message.name);
            }
            else
            {
                IncludeWriter.WriteLine(@"        void setContentStream(std::shared_ptr<std::iostream> contentStream);");
                CppWriter.WriteLine(
        @"
        std::iostream& Abstract{0}::contentStream() 
        {{
            if (m_contentStream == nullptr)
                m_contentStream.reset(new std::stringstream());
            return *m_contentStream;
        }}

        void Abstract{0}::setContentStream(std::shared_ptr<std::iostream> contentStream)
        {{
            assert(m_contentStream == nullptr);
            m_contentStream = contentStream;
        }}
", message.name);
            }

        CppWriter.WriteLine(
@"
        void Abstract{0}::build()
        {{
            build(contentStream(), false);
        }}

        void Abstract{0}::build(std::ostream& content, bool saveToOutput)
        {{
            auto alreadyBuilt = isBuilt();
            if (alreadyBuilt && !saveToOutput)
                return;

            // prebuild hook
            preBuild();

            std::unique_ptr<{0}Header> headerCopy;
            auto* builder = &m_header;
            if (alreadyBuilt)
            {{
                headerCopy = std::make_unique<{0}Header>(m_header);
                builder = headerCopy.get();
            }}", message.name);

            foreach (var field in message.field.Where(_ => _.type == typeType.nestedMessage))
            {
                CppWriter.WriteLine(@"            builder->clear_{0}();", field.name.ToLowerInvariant());
            }

            foreach (var index in message.index)
            {
                CppWriter.WriteLine(@"            builder->clear_{0}();", index.name.ToLowerInvariant());
            }

            CppWriter.WriteLine(
@"
            // build all nested messages to have their position in the content stream");

            // do nothing for reference messages and pod types: the builder is already the owner of those fields
            
            foreach (var field in message.field.Where(_ => _.type == typeType.nestedMessage))
            {
                if (field.modifier == modifierType.repeated)
                {
                    CppWriter.WriteLine(
@"            auto tmp_{0}List = {0}List();
            for (auto* {0} : tmp_{0}List)
            {{
                auto oldPos = {0}->positionInContent();
                {0}->build(content, saveToOutput);
                builder->add_{1}({0}->positionInContent());
                if (alreadyBuilt || saveToOutput)
                    {0}->setPositionInContent(oldPos);
            }}"
                        , field.name, field.name.ToLowerInvariant());
                }
                else
                {
                    CppWriter.WriteLine(
@"
            auto* tmp_{0} = {0}();
            if (tmp_{0} != nullptr) 
            {{ 
                auto oldPos = tmp_{0}->positionInContent();
                tmp_{0}->build(content, saveToOutput); 
                builder->set_{1}(tmp_{0}->positionInContent());
                if (alreadyBuilt)
                    tmp_{0}->setPositionInContent(oldPos);
            }}"
                        , field.name, field.name.ToLowerInvariant());
                }
            }

            // create indexes
            foreach (var index in message.index)
            {
                CppWriter.WriteLine(
@"
            std::sort(tmp_{0}List.begin(), tmp_{0}List.end(), []({1}* left, {1}* right) {{ return left->{2}() < right->{2}(); }});
            for (auto* {0} : tmp_{0}List)
            {{
                builder->add_{3}()->CopyFrom({0}->getLocalMessageDescriptor());
            }}
", index.ReferenceField.name, index.ReferenceField.messageType, index.SortingField.name, index.name.ToLowerInvariant());
            }

            CppWriter.WriteLine(
@"
            // write the header
            content.seekp(0, std::ios::end);

            // if we write to output, the position in the content stream
            // will be restored when writing the parent header
            // => this is not possible (and not needed) for root message
            auto isRoot = {1};
            auto dontSavePos = saveToOutput && isRoot;
            if (!dontSavePos)
                setPositionInContent((long)content.tellp());

            {0}::Util::writeDelimited(*builder, content, isRoot);

            if ((!alreadyBuilt && !saveToOutput) || (alreadyBuilt && saveToOutput))
            {{
                flush();
            }}
        }}
", ResourceNamespace, message.IsRoot ? "true" : "false");

            if (!message.IsRoot)
                return;

            IncludeWriter.WriteLine(@"        void writeDelimitedTo(std::ostream& output);");

            CppWriter.WriteLine(
@"        void Abstract{0}::writeDelimitedTo(std::ostream& output)
        {{
            build(output, true);
        }}
"
                , message.name);
        }

        protected override void GenerateFlush(messageType message)
        {
            IncludeWriter.WriteLine(@"        void flush();");
            CppWriter.WriteLine(
@"
        void Abstract{0}::flush()
        {{", message.name.Capitalize());

            var allFields = message.field.OrderBy(_ => _.id);

            var messageFields = from fld in allFields
                                where fld.messageType != null
                                && fld.type == typeType.nestedMessage
                                select fld;

            var has = false;
            foreach (var field in messageFields)
            {
                has = true;
                CppWriter.WriteLine(
                    field.modifier == modifierType.repeated
                        ? @"            m_{0}List.clear();"
                        : @"            m_{0}.reset();", field.name);
            }

            if (!has)
                CppWriter.WriteLine(@"            // NOP");

            CppWriter.WriteLine(@"        }");
            CppWriter.WriteLine();
        }

        protected override void GenerateSerialization(messageType message)
        {
            if (message.IsRoot)
            {
                IncludeWriter.WriteLine(
    @"        static std::unique_ptr<{0}> ParseFrom(std::shared_ptr<std::iostream> s);
"
                    , message.name);

                CppWriter.WriteLine(
@"        std::unique_ptr<{0}> Abstract{0}::ParseFrom(std::shared_ptr<std::iostream> s)
        {{
            {0}Header header;
            auto success = Util::readDelimited(*s, header, true); // the root is always at the end of the stream
            assert((""Can't decode header!"", success));
            
            auto parsed = std::make_unique<{0}>(header, (long)s->tellg());
            parsed->setContentStream(s);
            return parsed;
        }}
"
                    , message.name);
                return;
            }

            IncludeWriter.WriteLine(
@"        static std::unique_ptr<{0}> ParseFrom(std::iostream& s, int pos);
"
                , message.name);

            CppWriter.WriteLine(
@"        std::unique_ptr<{0}> Abstract{0}::ParseFrom(std::iostream& s, int pos)
        {{
            s.seekg(pos, std::ios::beg);

            {0}Header header;
            auto success = Util::readDelimited(s, header);
            assert((""Can't decode header!"", success));

            auto parsed = std::make_unique<{0}>(header, pos);
            return parsed;
        }}
"
                , message.name);
        }

        protected override void GenerateEqualsAndHashCode(messageType message)
        {
            // throw new NotImplementedException();
        }

        protected override void GenerateToString(messageType message)
        {
            IncludeWriter.WriteLine(
@"        using ProtoOrBuilder::toString;
        std::string toString(BaseFormat& format) override;
");
            CppWriter.WriteLine(
@"        std::string Abstract{0}::toString(BaseFormat& format)
        {{
            auto bd = std::ostringstream();
", message.name);

            CppWriter.WriteLine(
@"            format.formatHeader(bd,""{0}"");
", message.name);

            var fields = message.field.OrderBy(_ => _.id);

            foreach (var field in fields)
            {
                switch (field.modifier)
                {
                    case modifierType.repeated:
                        if (field.type == typeType.referenceMessage)
                        {
                            CppWriter.WriteLine(
    @"            format.formatField(bd, ""{1}List"", {0}Count()); 
    ", field.name, field.name.Capitalize());
                        }
                        else if (field.type == typeType.nestedMessage)
                        {
                            CppWriter.WriteLine(
    @"            format.formatProtoField(bd, ""{1}List"", {0}List()); 
    ", field.name, field.name.Capitalize());
                        }
                        else
                        {
                            CppWriter.WriteLine(
    @"            format.formatListField(bd, ""{1}List"", {0}List());
    ", field.name, field.name.Capitalize());
                        }
                        break;
                    case modifierType.optional:
                        if (field.type == typeType.referenceMessage || field.type == typeType.nestedMessage)
                        {
                            CppWriter.WriteLine(
@"            format.formatProtoField(bd, ""{1}"", {0}(), has{1}());
", field.name, field.name.Capitalize());
                        }
                        else
                        {
                            CppWriter.WriteLine(
@"            format.formatField(bd, ""{1}"", {0}(), has{1}());
    ", field.name, field.name.Capitalize());
                        }
                        break;
                    default:
                        if (field.type == typeType.referenceMessage || field.type == typeType.nestedMessage)
                        {
                            CppWriter.WriteLine(
@"            format.formatProtoField(bd, ""{0}"", {0}());", field.name);
                        }
                        else
                        {
                            CppWriter.WriteLine(
@"            format.formatField(bd, ""{0}"", {0}());", field.name);
                        }
                        break;
                }
            }

            CppWriter.WriteLine(
@"            format.formatFooter(bd);
            return bd.str();
        }
");
        }

        internal override void GenerateFinalClientClasses(protozbuffType node)
        {
            // close file .lazy.h
            IncludeWriter.WriteLine(GetNamespaceEnd(GeneratedNamespace));
            IncludeWriter.Dispose();
            IncludeWriter = null;

            // close file .lazy.cpp
            CppWriter.WriteLine(GetNamespaceEnd(GeneratedNamespace));
            CppWriter.Dispose();
            CppWriter = null;

            base.GenerateFinalClientClasses(node);
        }

        internal override void GenerateFinalClientClass(messageType message)
        {
            var filePathInclude = GetFilePath(IncludesFolder, message.name + ".h", Namespace);
            if (File.Exists(filePathInclude)) return;

            using (var strm = GetStreamFromPath(filePathInclude))
            {
                strm.WriteLine(
@"#pragma once
#include ""{0}/{1}.h""

{2}
    class {3} : public {4}::Abstract{3}
    {{
    public:
        {3}();
        {3}(const generated::{3}Header& header, int posInContent);
    }};
{5}
"
, GetNamespacePathSlash(GeneratedNamespace), // 0
DocumentName + ".lazy",                 // 1
GetNamespaceBegin(Namespace),           // 2
message.name.Capitalize(),              // 3
GeneratedNamespace,                     // 4
GetNamespaceEnd(Namespace));            // 5
            }

            var filePathCpp = GetFilePath(CppFolder, message.name + ".cpp", "");
            if (File.Exists(filePathCpp)) return;

            using (var strm = GetStreamFromPath(filePathCpp))
            {
                strm.WriteLine(
@"#include <stdafx.h>
#include ""include/{0}/{1}.h"""
, GetNamespacePathSlash(Namespace),  // 0
message.name.Capitalize()            // 1
);
                foreach (var field in message.field.Where(_ => _.type == typeType.nestedMessage))
                {
                strm.WriteLine(
@"#include ""include/{0}/{1}.h"""
, GetNamespacePathSlash(Namespace),  // 0
field.messageType.Capitalize()       // 1
);
                }

                strm.WriteLine(
@"{1}

    {0}::{0}()
    {{
        // NOP
    }}

    {0}::{0}(const generated::{0}Header& header, int posInContent) : Abstract{0}(header, posInContent)
    {{
        // NOP
    }}

{2}
"
, message.name.Capitalize(),            // 0
GetNamespaceBegin(Namespace),           // 1
GetNamespaceEnd(Namespace));            // 2);
            }
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
                case typeType.uint64:
                    return "unsigned long";
                case typeType.uint32:
                    return "unsigned int";
                case typeType.int64:
                    return "long";
                case typeType.int32:
                    return "int";
                case typeType.@string:
                    return "std::string";
                default:
                    return node.type.ToString();
            }
        }

        // we only take namespaces into account for headers
        protected override string GetFilePath(string folder, string name, string nspace)
        {
            if (Path.GetExtension(name) == ".h")
                return base.GetFilePath(folder, name, nspace);

            SafeDirectoryCreation(folder);
            return Path.Combine(folder, name);
        }
    }
}

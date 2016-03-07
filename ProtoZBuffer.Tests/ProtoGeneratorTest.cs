using System.IO;
using NUnit.Framework;
using protozbuffer;

namespace ProtoZBuffer.Tests
{
    [TestFixture]
    class ProtoGeneratorTest
    {
        [Test]
        public void MessageWith2Fields()
        {
            var foo = ProtozbuffLoader.Load(new StringReader(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<protozbuff xmlns=""http://tempuri.org/protoZ.xsd"">
  <message name=""Folder"" description=""Document definition"">
    <field id=""1"" modifier=""required"" name=""name"" type=""string""
           description=""Folder Name."" />
    <field id=""42"" modifier=""optional"" name=""size"" type=""int64""
           description=""Folder size."" />
  </message>
</protozbuff>"));

            const string result = @"package bar;

message FolderHeader
{
    required string name= 1;
    optional int64 size= 42;
}

message LocalMessageDescriptor
{
    repeated int32 coordinate = 1 [packed=true];
}
";
            var writer = new StringWriter();
            ProtoGenerator.Generate(foo, writer, "bar");
            Assert.That(writer.ToString(), Is.EqualTo(result));
        }

        [Test]
        public void NoModifierMeansRequired()
        {
            var foo = ProtozbuffLoader.Load(new StringReader(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<protozbuff xmlns=""http://tempuri.org/protoZ.xsd"">
  <message name=""Folder"" description=""Document definition"">
    <field id=""1"" name=""size"" type=""double""
           description=""Folder size."" />
  </message>
</protozbuff>"));

            const string result = @"package bar;

message FolderHeader
{
    required double size= 1;
}

message LocalMessageDescriptor
{
    repeated int32 coordinate = 1 [packed=true];
}
";
            var writer = new StringWriter();
            ProtoGenerator.Generate(foo, writer, "bar");
            Assert.That(writer.ToString(), Is.EqualTo(result));
        }

        [Test]
        public void EnumTypeForField()
        {
            var foo = ProtozbuffLoader.Load(new StringReader(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<protozbuff xmlns=""http://tempuri.org/protoZ.xsd"">
  <message name=""Folder"" description=""Document definition"">
    <field id=""1"" modifier=""required"" name=""name"" type=""enum"" enumType=""my_enum""
           description=""Folder Name."" />
  </message>
  <enum name=""my_enum"" description=""description of my enum"">
     <enumItem name=""item1"" description=""first item"" value=""42"" />
     <enumItem name=""item2"" description=""second item"" value=""21"" />
  </enum>
</protozbuff>"));

            const string result = @"package bar;

message FolderHeader
{
    required my_enum name= 1;
}

enum my_enum
{
    item1=42;
    item2=21;
}

message LocalMessageDescriptor
{
    repeated int32 coordinate = 1 [packed=true];
}
";
            var writer = new StringWriter();
            ProtoGenerator.Generate(foo, writer, "bar");
            Assert.That(writer.ToString(), Is.EqualTo(result));
        }
    }
}

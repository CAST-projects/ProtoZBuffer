using System.IO;
using NUnit.Framework;
using protozbuffer;

namespace ProtoZBuffer.tests
{
    [TestFixture]
    public class ProtozbuffLoarderTest
    {
        [Test]
        public void TestMethod1()
        {
            var stringReader = new StringReader(@"<?xml version=""1.0"" encoding=""utf-8"" ?>

<protozbuff xmlns=""http://tempuri.org/protoZ.xsd"">
  <message name=""Folder"" description=""Document definition"">
    <field id=""1"" modifier=""required"" name=""name"" type=""string""
           description=""Folder Name."" />
    <field id=""2"" modifier=""repeated"" name=""files"" messageType=""File""
           description=""Files"" />
    <field id=""3"" modifier=""repeated"" name=""folders"" type=""nestedMessage"" messageType=""Folder""
           description=""Folders embedded"" />
  </message>
  <message name=""File"" description=""File"">
    <field id=""1"" modifier=""required"" name=""name"" type=""string""
           description=""File Name."" />
  </message>
</protozbuff>");
            var syntaxTree = ProtozbuffLoader.Load(stringReader);
            Assert.That(syntaxTree, Is.Not.Null);

        }
    }
}

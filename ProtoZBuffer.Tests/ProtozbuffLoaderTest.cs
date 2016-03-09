using System.IO;
using NUnit.Framework;
using ProtoZBuffer;
using ProtoZBuffer.Core;

namespace ProtoZBuffer.Tests
{
    [TestFixture]
    public class ProtozbuffLoarderTest
    {
        private string _tempFilePath = null;

        [SetUp]
        public void Init()
        {
            _tempFilePath = Path.GetTempFileName();
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_tempFilePath))
            {
                File.Delete(_tempFilePath);
            }
        }

        private void CreateFile(string content)
        {
            using (var file = new StreamWriter(_tempFilePath))
            {
                file.Write(content);
            }
        }

        [Test]
        public void ValidProtoZBufferFile()
        {
            CreateFile(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<protozbuff xmlns=""http://tempuri.org/protoZ.xsd"">
  <message name=""Folder"" description=""Document definition"">
    <field id=""1"" modifier=""required"" name=""name"" type=""string""
           description=""Folder Name."" />
    <field id=""2"" modifier=""repeated"" name=""files"" type=""referenceMessage"" messageType=""File""
           description=""Files"" />
    <field id=""3"" modifier=""repeated"" name=""folders"" type=""nestedMessage"" messageType=""Folder""
           description=""Folders embedded"" />
  </message>
  <message name=""File"" description=""File"">
    <field id=""1"" modifier=""required"" name=""name"" type=""string""
           description=""File Name."" />
  </message>
</protozbuff>");

            protozbuffType protoTree = null;
            Assert.DoesNotThrow(() => protoTree = ProtozbuffLoader.Load(_tempFilePath));
            Assert.That(protoTree, Is.Not.Null);
        }

        [Test]
        public void EmptyFilePath()
        {
            protozbuffType protoTree = null;
            Assert.DoesNotThrow(() => protoTree = ProtozbuffLoader.Load(""));
            Assert.That(protoTree, Is.Null);
        }

        [Test]
        public void InvalidProtoZBufferFile()
        {
            CreateFile(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<protozbuff xmlns=""http://tempuri.org/protoZ.xsd"">
  <message name=""Folder"" description=""Document definition"">
    <field id=""1"" modifier=""required"" name=""name"" type=""string""
           description=""Folder Name."" />
    </message>
  </message>
</protozbuff>");

            protozbuffType protoTree = null;
            Assert.DoesNotThrow(() => protoTree = ProtozbuffLoader.Load(_tempFilePath));
            Assert.That(protoTree, Is.Null);
        }

    }
}

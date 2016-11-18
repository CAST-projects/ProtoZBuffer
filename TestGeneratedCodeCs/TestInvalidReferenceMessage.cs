using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileStructure;

namespace TestGeneratedCodeCs
{
    [TestFixture]
    public sealed class TestInvalidReferenceMessage
    {
        [Test]
        public void TestAddInvalidExternalMessage()
        {
            var externalDoc = new Document();
            var externalData = externalDoc.AddData();
            var externalIdentity = externalData.AddSecretIdentity();
            externalIdentity.FirstName = "Toto";
            externalIdentity.BirthYear = 2013;

            var currentDoc = new Document();
            var data = currentDoc.AddData();
            Assert.Throws<ArgumentException>(() => data.AddReviewers(externalIdentity));
            Assert.Throws<ArgumentException>(() => data.Approver = externalIdentity);
        }
    }
}

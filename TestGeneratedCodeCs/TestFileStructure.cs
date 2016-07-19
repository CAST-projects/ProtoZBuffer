using FileStructure;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGeneratedCodeCs
{
    [TestFixture]
    public class TestFileStructure
    {
        [TestCase]
        public void TestSinglesSave()
        {
            var doc = new FileStructure.Document();
            {
                var d1 = doc.AddData();
                var id1 = d1.Identification;
                id1.FirstName = "UniverseAndEverything";

                var d2 = doc.AddData();
                var id2 = d2.Identification;
                id2.BirthYear = 1791;
                id2.FirstName = "Babbage";
            }
            var bytes = doc.WriteDelimitedToBytes();
            var resultDoc = Document.ParseFrom(bytes);
            Assert.AreEqual(2, resultDoc.DataCount);
            {
                var d1 = resultDoc.GetData(0);
                Assert.AreEqual("UniverseAndEverything", d1.Identification.FirstName);
                Assert.IsFalse(d1.Identification.HasBirthYear);

                var d2 = resultDoc.GetData(1);
                Assert.AreEqual("Babbage", d2.Identification.FirstName);
                Assert.AreEqual(1791, d2.Identification.BirthYear);
            }
        }

        [TestCase]
        public void TestBuildsAndSave()
        {
            var doc = new FileStructure.Document();
            {
                var d1 = doc.AddData();
                var id1 = d1.Identification;
                id1.FirstName = "UniverseAndEverything";

                var d2 = doc.AddData();
                var id2 = d2.Identification;
                id2.BirthYear = 1791;
                id2.FirstName = "Babbage";
            }
            doc.Build();
            doc.Build();
            doc.Build();
            var bytes = doc.WriteDelimitedToBytes();
            var resultDoc = Document.ParseFrom(bytes);
            Assert.AreEqual(2, resultDoc.DataCount);
            {
                var d1 = resultDoc.GetData(0);
                Assert.AreEqual("UniverseAndEverything", d1.Identification.FirstName);
                Assert.IsFalse(d1.Identification.HasBirthYear);

                var d2 = resultDoc.GetData(1);
                Assert.AreEqual("Babbage", d2.Identification.FirstName);
                Assert.AreEqual(1791, d2.Identification.BirthYear);
            }
        }

    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.Tests
{
    [TestClass()]
    public class ExifManagerDotNetTests
    {
        [TestMethod()]
        public void SetExifToImageTest()
        {
            //Assert.Fail();
        }

        // テスト後Private化
        //[TestMethod()]
        //public void IsValidFileTest()
        //{
        //    ExifManagerDotNet em = new ExifManagerDotNet();
        //    Assert.AreEqual(em.IsValidFile(null), false);
        //    Assert.AreEqual(em.IsValidFile(""), false);
        //    Assert.AreEqual(em.IsValidFile(string.Empty), false);
        //    Assert.AreEqual(em.IsValidFile("ABC"), false);
        //    Assert.AreEqual(em.IsValidFile("test.txt"), false);
        //    Assert.AreEqual(em.IsValidFile("tools.dll"), true);
        //    Assert.AreEqual(em.IsValidFile(@"C:\Windows\regedit.exe"), true);
        //}
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InperStudio.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.ViewModels.Tests
{
    [TestClass()]
    public class MainWindowViewModelTests
    {
        [TestMethod()]
        public void TestTest()
        {
            PrivateObject po = new PrivateObject(new MainWindowViewModel(null));
            Assert.AreEqual(po.Invoke("Test", 1, 1),2);
        }
    }
}
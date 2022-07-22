using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InperStudio.ViewModels.Tests
{
    [TestClass()]
    public class MainWindowViewModelTests
    {
        [TestMethod()]
        public void TestTest()
        {
            PrivateObject po = new PrivateObject(new MainWindowViewModel(null));
            Assert.AreEqual(po.Invoke("Test", 1, 1), 2);
        }
    }
}
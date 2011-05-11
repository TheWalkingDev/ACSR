using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ACSR.PythonScripting;


namespace ACSR.Code.PythonScripting.UnitTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        public UnitTest1()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        [DeploymentItem("Test01.py")]
        public void TestPyEngine()
        {
            //var ctx = engine.CreateScriptContextFromString("def test(): return 10 + globalAdd");
            //var fn = @"z:\Dev\Andre\Libraries\AndresCSharpResource\UnitTests\ACSR.Code.PythonScripting.UnitTests\Test01.py";
            var fn = "test01.py";

            var engine = new ScriptController(true);
            var ctx = engine.CreateScriptContextFromFile(fn);
            ctx.Scope.globalAdd = 10;
            var code = ctx.Execute();
            var retVal = ctx.Scope.test(10);
            Assert.AreEqual(ctx.Scope.globalAdd + 10, retVal);
        }

        IScriptContext getTest1Context()
        {
            var fn = "test01.py";

            var engine = new ScriptController(true);
            var ctx = engine.CreateScriptContextFromFile(fn);
            return ctx;
        }

        [TestMethod]
        [DeploymentItem("Test01.py")]
        public void TestByRef()
        {

            var ctx = getTest1Context();
            var code = ctx.Execute();
            int refInt = 0;
            Action t = () => refInt = 1;

            
            var retVal = ctx.Scope.TestByRef(t, ref refInt); 

            Assert.AreEqual(10, refInt);
        }
        


    }
}

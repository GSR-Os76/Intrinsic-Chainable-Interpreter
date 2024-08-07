﻿using GSR.IntrinsicChainable;

namespace GSR.Tests.IntrinsicChainable
{
    [TestClass]
    public class TestSessionContext
    {

        [TestMethod]
        [ExpectedException(typeof(UndefinedMemberException))]
        [DataRow("")]
        [DataRow("fjisoroeiwq ")]
        [DataRow("_e,l_ dlf")]
        [DataRow("ʡ")]
        public void TestGetValueNonExistant(string name) => new SessionContext().GetValue(name);

    } // end class
} // end namespace
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpgradeSubstrateTargetVersion.Tests
{
    [TestClass]
    public class XmlHelperTest
    {

        [TestMethod]
        public void T1()
        {
            var result = MatchParam.Load("Match/wxs.xml");

        }
    }
}

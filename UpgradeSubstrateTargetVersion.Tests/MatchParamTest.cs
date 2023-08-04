using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpgradeSubstrateTargetVersion.Tests
{
    [TestClass]
    public class MatchParamTest
    {

        [TestMethod]
        public void T1()
        {
            var result = MatchParam.Load("Match/csproj.replace.xml");
        }
    }
}

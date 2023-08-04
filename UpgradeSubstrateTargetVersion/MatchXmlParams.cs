namespace UpgradeSubstrateTargetVersion
{
    using System.Xml;
    using System.Xml.Linq;

    public class MatchXmlParams
    {
        public class MatchType
        {
            public string NodeName { get; set; }

            public string Value { get; set; }
        }

        public List<string> Path { get; set; } = new List<string>();

        public List<string> PathNot { get; set; } = new List<string>();

        public List<XElement> When { get; set; } = new List<XElement>();

        public bool IsWhenAll { get; set; } = true;

        public List<XElement> WhenNot { get; set; } = new List<XElement>();

        public List<XElement> value { get; set; } = new List<XElement>();

        public XElement Where { get; set; }

        public static List<MatchXmlParams> Load(string path)
        {
            List<MatchXmlParams> matchParamsList = new List<MatchXmlParams>();

            XDocument xDocument = XDocument.Load(path);

            foreach (XElement item in xDocument.Element("xml").Elements("Case"))
            {
                MatchXmlParams matchXmlParams = new MatchXmlParams();
                matchParamsList.Add(matchXmlParams);
                matchXmlParams.When.AddRange(item.Elements("When").Elements());
                matchXmlParams.WhenNot.AddRange(item.Elements("WhenNot").Elements());
                matchXmlParams.value.AddRange(item.Elements("Value").Elements());
                matchXmlParams.Where = item.Elements("Where").Elements().FirstOrDefault();
                matchXmlParams.IsWhenAll = item.Attribute("IsWhenAll") == null ? true : bool.Parse(item.Attribute("IsWhenAll").Value);
                matchXmlParams.Path.AddRange(item.Elements("Path").SelectMany(x => x.Value.Split(Environment.NewLine).Where(y => !string.IsNullOrWhiteSpace(y)).Select(y => y.Trim())));
                matchXmlParams.PathNot.AddRange(item.Elements("PathNot").SelectMany(x => x.Value.Split(Environment.NewLine).Where(y => !string.IsNullOrWhiteSpace(y)).Select(y => y.Trim())));

                if (matchXmlParams.Where == null)
                {
                    matchXmlParams.Where = matchXmlParams.When.FirstOrDefault();
                }
            };

            return matchParamsList;
        }
    }
}

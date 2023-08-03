namespace UpgradeSubstrateTargetVersion
{
    using System.Xml;

    public class MatchParam
    {
        public List<string> Path { get; set; } = new List<string>();

        public List<string> PathNot { get; set; } = new List<string>();

        public List<string> When { get; set; } = new List<string>();

        public bool IsWhenAll { get; set; } = true;

        public List<string> WhenNot { get; set; } = new List<string>();

        public List<string> value { get; set; } = new List<string>();

        public string Where { get; set; }

        public static List<MatchParam> Load(string path)
        {
            List<MatchParam> matchParamsList = new List<MatchParam>();
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.Load(path);

            foreach (XmlNode xmlNodeList in xmlDocument.ChildNodes)
            {
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    if (xmlNode.NodeType == XmlNodeType.Whitespace)
                    {
                        continue;
                    }

                    MatchParam matchParam = new MatchParam();
                    foreach (XmlNode node in xmlNode.ChildNodes)
                    {
                        if (node.NodeType == XmlNodeType.Whitespace)
                        {
                            continue;
                        }
                        if (node.Name == "Path")
                        {
                            foreach (var item in node.InnerXml.Split(Environment.NewLine))
                            {
                                if (!string.IsNullOrWhiteSpace(item))
                                {
                                    matchParam.Path.Add(item.TrimStart());
                                }
                            }
                        }
                        if (node.Name == "PathNot")
                        {
                            foreach (var item in node.InnerXml.Split(Environment.NewLine))
                            {
                                if (!string.IsNullOrWhiteSpace(item))
                                {
                                    matchParam.PathNot.Add(item.TrimStart());
                                }
                            }
                        }
                        else if (node.Name == "WhenAll")
                        {
                            matchParam.IsWhenAll = !node.InnerText.Contains("false", StringComparison.OrdinalIgnoreCase);
                        }
                        else if (node.Name == "When")
                        {
                            foreach (XmlNode item in node.ChildNodes)
                            {
                                if (item.NodeType == XmlNodeType.Element || item.NodeType == XmlNodeType.Text)
                                {
                                    string regex = GetRegexValue(node, item);
                                    matchParam.When.Add(regex);
                                }
                            }
                        }
                        else if (node.Name == "WhenNot")
                        {
                            foreach (XmlNode item in node.ChildNodes)
                            {
                                if (item.NodeType == XmlNodeType.Element || item.NodeType == XmlNodeType.Text)
                                {
                                    string regex = GetRegexValue(node, item);
                                    matchParam.WhenNot.Add(regex);
                                }
                            }
                        }
                        else if (node.Name == "Where")
                        {
                            foreach (XmlNode item in node.ChildNodes)
                            {
                                if (item.NodeType != XmlNodeType.Whitespace)
                                {
                                    string regex = item.OuterXml.ToRegexString();
                                    matchParam.Where = regex;
                                    break;
                                }
                            }
                        }
                        else if (node.Name == "Value")
                        {
                            int spaceCount = 0;
                            foreach (var item in node.InnerXml.Split(Environment.NewLine))
                            {
                                if (!string.IsNullOrWhiteSpace(item))
                                {
                                    if (spaceCount == 0)
                                    {
                                        spaceCount = item.TakeWhile(c => c == ' ').Count();
                                    }

                                    matchParam.value.Add(item.Substring(spaceCount));
                                }
                            }
                        }
                    }

                    matchParamsList.Add(matchParam);
                }
            }

            return matchParamsList;
        }

        private static string GetRegexValue(XmlNode node, XmlNode item)
        {
            var noprefixspace = node?.Attributes != null && string.Equals(node.Attributes["NoPrefixSpace"]?.Value, "true", StringComparison.OrdinalIgnoreCase);
            var nopostfixspace = node?.Attributes != null && string.Equals(node.Attributes["NoPostfixSpace"]?.Value, "true", StringComparison.OrdinalIgnoreCase);

            string regex = item.OuterXml.ToRegexString(noprefixspace, nopostfixspace);
            return regex;
        }
    }
}

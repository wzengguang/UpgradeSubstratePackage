using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace UpgradeSubstrateTargetVersion
{
    public class MatchParam
    {
        public List<string> Path { get; set; } = new List<string>();

        public List<string> When { get; set; } = new List<string>();

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
                        else if (node.Name == "When")
                        {
                            foreach (XmlNode item in node.ChildNodes)
                            {
                                if (item.NodeType == XmlNodeType.Whitespace)
                                {
                                    continue;
                                }

                                string regex = item.OuterXml.ToRegexString();
                                matchParam.When.Add(regex);
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
    }
}

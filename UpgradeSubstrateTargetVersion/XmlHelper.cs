namespace UpgradeSubstrateTargetVersion
{
    using System.Xml.Linq;

    public static class XmlHelper
    {

        public static XElement Find(this XElement aElement, XElement bElement)
        {
            var all = aElement.Descendants(aElement.GetDefaultNamespace() + bElement.Name.LocalName);
            foreach (var item in all)
            {
                if (item.EqualContain(bElement))
                {
                    return item;
                }
            }

            return null;
        }

        public static XElement Clone(this XElement aElement, XElement bElement)
        {
            XElement xElement = new XElement(aElement);

            xElement.Name = bElement.Name.Namespace + aElement.Name.LocalName;

            return xElement;
        }

        public static bool EqualContain(this XElement aElement, XElement bElement)
        {
            if (aElement == null || bElement == null)
            {
                return false;
            }

            if (aElement.Name.LocalName != bElement.Name.LocalName)
            {
                return false;
            }

            if (string.IsNullOrEmpty(bElement.Value.Trim()) && !aElement.Value.Trim().Equals(bElement.Value.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            foreach (XAttribute bAttribute in bElement.Attributes())
            {
                XAttribute aAttribute = aElement.Attribute(bAttribute.Name);
                if (aAttribute == null || aAttribute.Value.Equals(bAttribute.Value, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            foreach (XElement bElementChild in bElement.Elements())
            {
                bool equal = false;

                foreach (var aElementChild in aElement.Elements(bElementChild.Name))
                {
                    if (aElementChild.EqualContain(bElementChild))
                    {
                        equal = true;
                        break;
                    }
                }
                if (!equal)
                {
                    return false;
                }
            }

            return true;
        }
    }
}

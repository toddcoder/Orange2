using Orange.Library.Managers;
using System.Xml.Linq;
using System.Linq;

namespace Orange.Library.Values
{
	public class XMLElement : Value
	{
		XElement element;

		public XMLElement(XElement element)
		{
			this.element = element;
		}

		public override int Compare(Value value)
		{
			return 0;
		}

		public override string Text
		{
			get
			{
				return element.ToString();
			}
			set
			{
			}
		}

		public override double Number
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public override ValueType Type
		{
			get
			{
				return ValueType.XMLElement;
			}
		}

		public override bool IsTrue
		{
			get
			{
				return false;
			}
		}

		public override Value Clone()
		{
			return new XMLElement(element);
		}

		public Value this[string name]
		{
			get
			{
				if (name == null)
					return "";
				return name.StartsWith("@") ? GetAttributeValue(name) : GetNodeValue(name);
			}
			set
			{
				if (name.StartsWith("@"))
					SetAttributeValue(name, value.Text);
				else
					SetNodeValue(name, value.Text);
			}
		}

		protected override void registerMessages(MessageManager manager)
		{
			manager.RegisterMessage(this, "name", v => ((XMLElement)v).Name());
			manager.RegisterMessage(this, "nodes", v => ((XMLElement)v).Nodes());
			manager.RegisterMessage(this, "attr", v => ((XMLElement)v).Attributes());
			manager.RegisterMessage(this, "value", v => ((XMLElement)v).InnerText());
		}

		public Value Name()
		{
			return element.Name.LocalName;
		}

		public Value Nodes()
		{
			return new Array(element.Elements().Select(e => new XMLElement(e)).ToArray());
		}

		public Value Attributes()
		{
			return new Array(element.Attributes().Select(a => new KeyedValue(a.Name.LocalName, a.Value)).ToArray());
		}

		public Value InnerText()
		{
			return element.Value;
		}

		public bool ContainsNode(string nodeName)
		{
			return element.Elements(nodeName).Any();
		}

		public string GetNodeValue(string nodeName)
		{
			XElement result = element.Elements(nodeName).FirstOrDefault();
			return result == null ? "" : result.Value;
		}

		public void SetNodeValue(string nodeName, string value)
		{
			XElement result = element.Elements(nodeName).FirstOrDefault();
			if (result != null)
				result.Value = value;
		}

		public string GetAttributeValue(string attributeName)
		{
			if (attributeName.StartsWith("@"))
				attributeName = attributeName.Substring(1);
			var result = element.Attribute(attributeName);
			return result == null ? "" : result.Value;
		}

		public void SetAttributeValue(string attributeName, string value)
		{
			if (attributeName.StartsWith("@"))
				attributeName = attributeName.Substring(1);
			var result = element.Attribute(attributeName);
			if (result != null)
				result.Value = value;
		}

		public override string ToString()
		{
			return element.ToString();
		}
	}
}
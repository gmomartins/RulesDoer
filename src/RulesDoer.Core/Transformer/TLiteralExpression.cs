using System;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace RulesDoer.Core.Transformer {

    [Serializable]
    [XmlType ("tLiteralExpression", Namespace = "http://www.omg.org/spec/DMN/20151101/dmn.xsd")]
    [XmlRoot ("literalExpression", Namespace = "http://www.omg.org/spec/DMN/20151101/dmn.xsd")]
    public partial class TLiteralExpression : TExpression {

        [XmlElement ("text", Namespace = "http://www.omg.org/spec/DMN/20151101/dmn.xsd", DataType = "string")]
        public string Text { get; set; }

        [XmlElement ("importedValues", Namespace = "http://www.omg.org/spec/DMN/20151101/dmn.xsd")]
        public TImportedValues ImportedValues { get; set; }

        [XmlAttribute ("expressionLanguage", Form = XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string ExpressionLanguage { get; set; }
    }
}
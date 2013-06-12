using System;
using System.Xml.Serialization;

namespace AnyParser
{
    [Serializable]
    public class PostProcessInstruction
    {
        [XmlAttribute]
        public string IfFound { get; set; }
        [XmlAttribute]
        public string ReplaceBy { get; set; }
    }
}
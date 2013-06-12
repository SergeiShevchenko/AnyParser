using System.Collections.Generic;
using System.Xml.Serialization;

namespace AnyParser
{
    /// <summary>
    /// Правило синтаксиса
    /// </summary>
    public class Rule
    {
        /// <summary>
        /// Левая часть правила
        /// </summary>
        [XmlAttribute]
        public string NonTerminal { get; set; }

        [XmlAttribute]
        public string CanBe { get; set; }

        /// <summary>
        /// Правая часть правила (содержит различные варианты)
        /// </summary>
        [XmlIgnore]
        public List<RuleVariant> Right { get; set; }

        public override string ToString()
        {
            return NonTerminal + "::=" + string.Join(" | ", Right);
        }
    }
}

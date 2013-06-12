using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace AnyParser
{
    /// <summary>
    /// Описание грамматики
    /// </summary>
    [Serializable]
    public class SyntaxGrammar
    {
        /// <summary>
        /// Название главного правила
        /// </summary>
        [XmlElement("MainRule")]
        public string MainRuleName { get; set; }

        /// <summary>
        /// Главное правило
        /// </summary>
        [XmlIgnore]
        public Rule MainRule { get; set; }

        /// <summary>
        /// Список правил из файла
        /// </summary>
        public List<Rule> Rules { get; set; }

        /// <summary>
        /// Производит инициализацию описания грамматики из файла
        /// </summary>
        public static SyntaxGrammar Read(string fileName)
        {
            SyntaxGrammar result = (SyntaxGrammar)new XmlSerializer(typeof(SyntaxGrammar)).Deserialize(new StreamReader(fileName));
            for (int i = 0; i < result.Rules.Count; i++)
            {
                Rule rule = result.Rules[i];
                rule.NonTerminal = rule.NonTerminal.Trim();
                rule.Right = new List<RuleVariant>();
                foreach (string variant in rule.CanBe.Replace("{", " {").Replace("}", "} ").Replace("   ", " ").Replace("  ", " ").Split('|'))
                {
                    RuleVariant ss = new RuleVariant
                    {
                        Text = variant.Trim()
                    };
                    foreach (string part in variant.Trim().Split(' '))
                    {
                        if (part.Length == 0)
                            continue;
                        if (part.StartsWith("{") && part.EndsWith("}"))
                            ss.Add(new SyntaxItem(SyntaxItemType.NonTerminal, part.Substring(1, part.Length - 2)));
                        else
                            ss.Add(new SyntaxItem(SyntaxItemType.Terminal, part));
                    }
                    rule.Right.Add(ss);
                }
                if (rule.NonTerminal == result.MainRuleName.Trim())
                    result.MainRule = rule;
            }
            if (result.MainRule == null)
                throw new Exception("Main rule not found");
            return result;
        }

        /// <summary>
        /// Производит поиск правила с необходимой левой частью
        /// </summary>
        /// <param name="nonTerminalName">Левая часть правила</param>
        public Rule Find(string nonTerminalName)
        {
            foreach (Rule syntaxRule in Rules)
                if (syntaxRule.NonTerminal == nonTerminalName)
                    return syntaxRule;
            return null;
        }
    }
}

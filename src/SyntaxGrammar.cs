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
        /// Название главного правила (Name of the main rule)
        /// </summary>
        [XmlElement("MainRule")]
        public string MainRuleName { get; set; }

        /// <summary>
        /// Главное правило (Main rule)
        /// </summary>
        [XmlIgnore]
        public Rule MainRule { get; set; }

        /// <summary>
        /// Список правил из файла (List of rules from file)
        /// </summary>
        public List<Rule> Rules { get; set; }

        /// <summary>
        /// Производит инициализацию описания грамматики из файла (Initializes grammar description from file)
        /// </summary>
        public static SyntaxGrammar Read(string fileName, List<Table> tables)
        {
            SyntaxGrammar result = (SyntaxGrammar)new XmlSerializer(typeof(SyntaxGrammar)).Deserialize(new StreamReader(fileName));
            for (int i = 0; i < result.Rules.Count; i++)
            {
                Rule rule = result.Rules[i];
                rule.NonTerminal = rule.NonTerminal.Trim();
                rule.Right = new List<RuleVariant>();
                foreach (string variant in rule.CanBe.Replace("{", " {").Replace("}", "} ")
                    .Replace("[", " [ ").Replace("]", " ] ").Split('|'))
                {
                    RuleVariant rv = new RuleVariant();
                    ParseRuleVariant(rule.NonTerminal, rv, variant.Trim().Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries), tables);
                    rv.Text = string.Join(" ", rv);
                    rule.Right.Add(rv);
                }
                if (rule.NonTerminal == result.MainRuleName.Trim())
                    result.MainRule = rule;
            }
            if (result.MainRule == null)
                throw new Exception("Main rule not found");
            return result;
        }

        /// <summary>
        /// Разбирает вариант правила (Parses the variant of the rule)
        /// </summary>
        private static void ParseRuleVariant(string ruleName, IList<SyntaxItem> items, string[] parts, List<Table> tables, int start = 0, int end = -1)
        {
            if (end == -1)
                end += parts.Length;
            for (int partNumber = start; partNumber <= end; partNumber++)
            {
                string part = parts[partNumber];
                if (part == "[")
                {
                    int cycleOpensAt = partNumber;
                    int balance = 1;
                    partNumber++;    
                    while (balance != 0)
                    {
                        if (parts[partNumber] == "]")
                            balance--;
                        if (parts[partNumber] == "[")
                            balance++;
                        if (partNumber == end && balance != 0)
                            throw new Exception(string.Format("Rule {0} is incorrect", ruleName));
                        partNumber++;                        
                    }
                    partNumber--;
                    SyntaxCycleItem nSyntaxItem = new SyntaxCycleItem();
                    ParseRuleVariant(ruleName, nSyntaxItem.List, parts, tables, cycleOpensAt + 1, partNumber - 1);
                    nSyntaxItem.Text = "["+string.Join(" ", nSyntaxItem.List)+"]";
                    items.Add(nSyntaxItem);
                }
                else if (part.StartsWith("{") && part.EndsWith("}"))
                    items.Add(new SyntaxItem(SyntaxItemType.NonTerminal, part.Substring(1, part.Length - 2)));
                else
                {
                    if (!tables[LexicAnalysis.SEPARATORS].Contains(part))
                        if (!tables[LexicAnalysis.WORDS].Contains(part))
                            throw new Exception("Unknown terminal: " + part);
                    items.Add(new SyntaxItem(SyntaxItemType.Terminal, part));
                }
            }
        }

        /// <summary>
        /// Производит поиск правила с необходимой левой частью (Searches for the rule with the fitting left side)
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

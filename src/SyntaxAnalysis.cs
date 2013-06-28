using System;
using System.Linq;
using System.Collections.Generic;

namespace AnyParser
{
    /// <summary>
    /// Синтаксический анализатор
    /// </summary>
    public class SyntaxAnalysis
    {
        private LexicAnalysis lexems;
        private SyntaxGrammar _syntaxGrammar;
        private int position;
        private Lexem currentLexem;
        public SyntaxNode MainNode = new SyntaxNode(string.Empty);

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="lexems">Готовый список лексем в нужном порядке</param>
        /// <param name="syntaxGrammarFileName">Файл с описанием грамматики</param>
        public SyntaxAnalysis(LexicAnalysis lexems, string syntaxGrammarFileName)
        {
            this.lexems = lexems;
            try
            {
                _syntaxGrammar = SyntaxGrammar.Read(syntaxGrammarFileName, lexems.Tables);
                next();
                MainNode.Desc = _syntaxGrammar.MainRuleName;
                Inspect(_syntaxGrammar.MainRule, MainNode);
                if (currentLexem != null)
                {
                    SyntaxNode ignored = new SyntaxNode("Ignored text");
                    ignored.BeginLexem = position - 1;
                    ignored.EndLexem = lexems.Output.Count - 1;
                    ignored.SyntaxNodeType = SyntaxNodeType.Failure;
                    MainNode.Children.Add(ignored);
                }
            }
            catch
            {
            }
            if (lexems.ErrorMsg != null)
                MainNode.ErrorMsg = lexems.ErrorMsg;
        }

        /// <summary>
        /// Получение следующей лексемы
        /// </summary>
        private void next()
        {
            if (position == lexems.Output.Count)
            {
                currentLexem = null;
                position++;
                return;
            }
            currentLexem = lexems.Output[position];
            position++;
        }

        /// <summary>
        /// Проверка правила с текущей позиции в списке лексем.
        /// Производит анализ всех возможных раскрытий правила, которые удовлетворяют по первой лексеме.
        /// </summary>
        /// <param name="rule">Правило</param>
        /// <param name="ruleNode">Узел, в который пишем результат разбора правила</param>
        private void Inspect(Rule rule, SyntaxNode ruleNode)
        {
            // Переменные в стеке, для запоминания с какой позиции начали смотреть, чтобы в нее вернуться при неудаче разбора
            int i;
            Exception lastException = null;
            ruleNode.BeginLexem = position - 1;
            for (i = 0; i < rule.Right.Count; i++)
            {
               // один из вариантов раскрытия правила
                RuleVariant variant = rule.Right[i];
                SyntaxNode variantNode = new SyntaxNode(variant.ToString());               
                try
                {
                    position = ruleNode.BeginLexem + 1;
                    currentLexem = lexems.Output[ruleNode.BeginLexem];
                    // проверяем первую лексему, сравнивая ее с currentLexem:
                    if (FastMismatchCheck(variant.GetFirst()) != null)
                        continue;
                    ruleNode.Children.Add(variantNode);
                    // первый тест пройден, запускаем полный анализ варианта, с возможной рекурсией:
                    ParseSyntaxItemsList(variant, variantNode);
                    // вариант проработал до последней лексемы, правило выполнено
                    // не рассматриваем другие варианты этого же правила
                    while (ruleNode.Children.Count > 1)
                        ruleNode.Children.RemoveAt(0);
                    ruleNode.SyntaxNodeType = SyntaxNodeType.Success;
                    if (variant.Count > 1 || variant[0].Type != SyntaxItemType.Terminal)
                        variantNode.SyntaxNodeType = SyntaxNodeType.Success;
                    ruleNode.EndLexem = position - 2;
                    variantNode.BeginLexem = ruleNode.BeginLexem;
                    variantNode.EndLexem = ruleNode.EndLexem;
                    return;
                }
                catch (Exception e)
                {
                    variantNode.ErrorMsg = e.Message;
                    variantNode.SyntaxNodeType = SyntaxNodeType.Failure;
                    variantNode.BeginLexem = ruleNode.BeginLexem;
                    variantNode.EndLexem = position - 1;
                    if (variantNode.EndLexem >= lexems.Output.Count)
                        variantNode.EndLexem = lexems.Output.Count - 1;
                    lastException = e;
                    ruleNode.EndLexem = ruleNode.EndLexem > position - 1 ? ruleNode.EndLexem : position - 1;
                    if (ruleNode.EndLexem >= lexems.Output.Count)
                        ruleNode.EndLexem = lexems.Output.Count - 1;
                }
            }
            ruleNode.SyntaxNodeType = SyntaxNodeType.Failure;
            ruleNode.ErrorMsg = lastException.Message;
            throw lastException;
        }

        /// <summary>
        /// Быстрая проверка на несовпадение с правилом (Checking for mistakes)
        /// </summary>
        private string FastMismatchCheck(SyntaxItem syntaxItem)
        {
            if (syntaxItem.Type == SyntaxItemType.Terminal)
            {
                if (!equalsTerminal(syntaxItem, currentLexem))
                    return string.Format("{0} expected but {1} found", syntaxItem.Text, currentLexem.Display);
            }
            if (syntaxItem.Text == "Number")
            {
                if (!isNumber(currentLexem))
                    return string.Format("{0} expected but {1} found", syntaxItem.Text, currentLexem.Display);
            }
            if (syntaxItem.Text == "Identifier")
            {
                if (!isIdentifier(currentLexem))
                    return string.Format("{0} expected but {1} found", syntaxItem.Text, currentLexem.Display);
            }
            return null;
        }

        /// <summary>
        /// Разбор списка синтаксических элементов (Parsing of the list of syntax items)
        /// </summary>
        private void ParseSyntaxItemsList(IList<SyntaxItem> list, SyntaxNode variantNode)
        {
            foreach (SyntaxItem syntaxItem in list)
            {
                if (currentLexem == null)
                    throw new Exception(string.Format("{0} expected but EOF found", syntaxItem.Text));
                if (syntaxItem.Type == SyntaxItemType.Cycle)
                {
                    SyntaxCycleItem cycle = (SyntaxCycleItem)syntaxItem;
                    while (currentLexem != null)
                    {
                        if (FastMismatchCheck(cycle.GetFirst()) != null)
                            break;
                        ParseSyntaxItemsList(cycle.List, variantNode);                        
                    }
                    continue;
                }
                string mismatchDescription = FastMismatchCheck(syntaxItem);
                if (mismatchDescription != null)
                    throw new Exception(mismatchDescription);
                SyntaxNode innerNode = variantNode;
                // только для случая, когда несколько элементов в правой части правила, создаем под них отдельные узлы:
                if (list.Count > 1)
                {
                    innerNode = new SyntaxNode(syntaxItem.Text);
                    variantNode.Children.Add(innerNode);
                }
                if (syntaxItem.Type == SyntaxItemType.Terminal)
                {
                    // терминал успешно распознан
                    innerNode.BeginLexem = position - 1;
                    innerNode.EndLexem = position - 1;
                    next();
                }
                else if (syntaxItem.Text == "Number" || syntaxItem.Text == "Identifier")
                {
                    // число или идентификатор успешно распознаны
                    innerNode.BeginLexem = position - 1;
                    innerNode.EndLexem = position - 1;
                    innerNode.SyntaxNodeType = SyntaxNodeType.Success;
                    innerNode.Children.Add(new SyntaxNode(currentLexem.Display)
                    {
                        BeginLexem = innerNode.BeginLexem,
                        EndLexem = innerNode.EndLexem
                    });
                    next();
                }
                else // запускаем рекурсивный анализ вложенного нетерминала по его правилу
                    Inspect(_syntaxGrammar.Find(syntaxItem.Text), innerNode);
            }
        }

        /// <summary>
        /// Проверка на совпадение терминального элемента
        /// </summary>
        /// <param name="syntaxItem">Лексема из описания правила</param>
        /// <param name="lexem">Лексема в коде, сопоставляемая с ней</param>
        /// <returns>True, если лексемы совпадают</returns>
        private bool equalsTerminal(SyntaxItem syntaxItem, Lexem lexem)
        {
            return lexems.Tables[lexem.Table][lexem.Index].Equals(syntaxItem.Text);
        }

        /// <summary>
        /// Проверка на численность лексемы
        /// </summary>
        /// <returns>True, если число</returns>
        private bool isNumber(Lexem lexem)
        {
            return lexem.Table == LexicAnalysis.INTEGERS || lexem.Table == LexicAnalysis.REALS;
        }

        /// <summary>
        /// Проверка на идентификатор
        /// </summary>
        /// <returns>True, если идентификатор</returns>
        private bool isIdentifier(Lexem lexem)
        {
            return lexem.Table == LexicAnalysis.IDENTIFIERS;
        }
    }
}
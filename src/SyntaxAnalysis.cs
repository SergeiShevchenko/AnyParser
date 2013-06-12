using System;
using System.Linq;

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
        public SyntaxNode MainNode = new SyntaxNode("");

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="lexems">Готовый список лексем в нужном порядке</param>
        /// <param name="_syntaxGrammar">Описание грамматики</param>
        public SyntaxAnalysis(LexicAnalysis lexems, SyntaxGrammar _syntaxGrammar)
        {
            this.lexems = lexems;
            this._syntaxGrammar = _syntaxGrammar;
        }

        /// <summary>
        /// Производит синтаксический разбор
        /// </summary>
        public void Work()
        {
            try
            {
                next();
                MainNode.Desc = _syntaxGrammar.MainRuleName;
                Inspect(_syntaxGrammar.MainRule, MainNode);
            }
            catch (Exception e)
            {
            }            
        }

        /// <summary>
        /// Получение следующей лексемы
        /// </summary>
        private void next()
        {
            if (position > lexems.Output.Count)
                throw new Exception("End of file too early");
            if (position == lexems.Output.Count)
            {
                position++;
                currentLexem = null;
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
                    var first = variant.First();
                    if (first.Type == SyntaxItemType.Terminal)
                    {
                        if (!equalsTerminal(first, currentLexem))
                            continue;
                    }
                    else if (first.Text == "Number")
                    {
                        if (!isNumber(currentLexem))
                            continue;
                    }
                    else if (first.Text == "Identifier")
                    {
                        if (!isIdentifier(currentLexem))
                            continue;
                    }
                    ruleNode.Children.Add(variantNode);
                    // первый тест пройден, запускаем полный анализ варианта, с возможной рекурсией:
                    foreach (SyntaxItem syntaxItem in variant)
                    {
                        if (syntaxItem.Type == SyntaxItemType.Terminal)
                        {
                            if (!equalsTerminal(syntaxItem, currentLexem))
                                throw error(currentLexem);
                            if (variant.Count > 1)
                                variantNode.Children.Add(new SyntaxNode(currentLexem.Display) 
                                { 
                                    BeginLexem = position - 1,
                                    EndLexem = position - 1
                                });
                            next();
                        }
                        else if (syntaxItem.Text == "Number")
                        {
                            if (!isNumber(currentLexem))
                                throw error(currentLexem);
                            variantNode.Children.Add(new SyntaxNode(currentLexem.Display)
                            {                                
                                BeginLexem = position - 1,
                                EndLexem = position - 1
                            });
                            next();
                        }
                        else if (syntaxItem.Text == "Identifier")
                        {
                            if (!isIdentifier(currentLexem))
                                throw error(currentLexem);
                            variantNode.Children.Add(new SyntaxNode(currentLexem.Display)
                            {
                                BeginLexem = position - 1,
                                EndLexem = position - 1
                            });
                            next();
                        }
                        else // рекурсивный вызов для вложенных нетерминалов:
                        {
                            SyntaxNode innerRule;
                            if (variant.Count > 1)
                            {
                                innerRule = new SyntaxNode(syntaxItem.Text);
                                variantNode.Children.Add(innerRule);
                            }
                            else
                                innerRule = variantNode;
                            Inspect(_syntaxGrammar.Find(syntaxItem.Text), innerRule);
                        }
                    }
                    // вариант проработал до последней лексемы, правило выполнено
                    // не рассматриваем другие варианты этого же правила
                    while (ruleNode.Children.Count > 1)
                        ruleNode.Children.RemoveAt(0);
                    ruleNode.SyntaxNodeType = SyntaxNodeType.Success;
                    variantNode.SyntaxNodeType = SyntaxNodeType.Success;
                    if (variant.Count == 1 && variant[0].Type == SyntaxItemType.Terminal)
                        variantNode.SyntaxNodeType = SyntaxNodeType.None;
                    ruleNode.EndLexem = position - 2;
                    variantNode.BeginLexem = ruleNode.BeginLexem;
                    variantNode.EndLexem = ruleNode.EndLexem;
                    return;
                }
                catch (Exception e)
                {
                    variantNode.Desc = String.Format("{0} ::= {1}", rule.NonTerminal, variant);
                    variantNode.SyntaxNodeType = SyntaxNodeType.Failure;
                    variantNode.BeginLexem = ruleNode.BeginLexem;
                    variantNode.EndLexem = position - 1;
                    lastException = e;
                    ruleNode.EndLexem = ruleNode.EndLexem > position - 1 ? ruleNode.EndLexem : position - 1;
                }
            }
            ruleNode.SyntaxNodeType = SyntaxNodeType.Failure;
            throw lastException;
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

        /// <summary>
        /// Генерация ошибок
        /// </summary>
        private Exception error(Lexem lexem)
        {
            //TODO: как генерировать новые ошибки пока непонятно
            return new Exception(string.Format("oops {0} {1} {2}", lexem.Display, lexem.LineNumber, lexem.EndColumnNumber));
        }
    }
}
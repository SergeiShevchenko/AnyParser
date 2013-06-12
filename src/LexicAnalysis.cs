using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace AnyParser
{
    /// <summary>
    /// Лексический анализатор
    /// </summary>
    public class LexicAnalysis
    {
        // Идентификаторы таблиц, совпадающие с их индексами в таблице таблиц
        public const int WORDS = 0;
        public const int SEPARATORS = 1;
        public const int IDENTIFIERS = 2;
        public const int INTEGERS = 3;
        public const int REALS = 4;

        /// <summary>
        /// Таблица таблиц лексем
        /// </summary>
        public List<Table> Tables = new List<Table> { new Table(), new Table(), new Table(), new Table(), new Table() };

        /// <summary>
        /// Результат анализа - полученные лексемы
        /// </summary>
        public List<Lexem> Output = new List<Lexem>();
  
        /// <summary>
        /// Строки разбираемого кода
        /// </summary>
        private string source;

        /// <summary>
        /// Текущий символ
        /// </summary>
        private char ch;

        /// <summary>
        /// Номер текущей строки
        /// </summary>
        private int lineNum;

        /// <summary>
        /// Номер текущей колонки в строке
        /// </summary>
        private int symNum;

        /// <summary>
        /// Номер текущей строки без учета последних пробелов
        /// </summary>
        private int lineNumWW;

        /// <summary>
        /// Номер текущей колонки в строке без учета последних пробелов
        /// </summary>
        private int symNumWW;

        private int charNum;

        private List<PostProcessInstruction> postProcessList;
        private List<Comment> commentsList;

        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="fileName">Имя файла с кодом</param>
        /// <param name="grammar">Настройки лексического анализа</param>
        public LexicAnalysis(string fileName, LexicalGrammar grammar)
        {
            Tables[WORDS].AddRange(grammar.ReservedWords);
            Tables[SEPARATORS].AddRange(grammar.Separators);
            postProcessList = grammar.PostProcessList;
            commentsList = grammar.CommentsList;
            source = string.Join("\n", File.ReadAllLines(fileName));
            charNum = 0;
            lineNum = symNum = lineNumWW = symNumWW = 1;
        }

        /// <summary>
        /// Начать анализ
        /// </summary>
        public void Work()
        {
            PreProcess();
            getChar();
            while (ch != '\0')
            {
                while (char.IsWhiteSpace(ch))
                    getChar();
                if (char.IsLetter(ch))
                {
                    StringBuilder lettersAccumulator = new StringBuilder();
                    lettersAccumulator.Append(ch);
                    int tempSymNumWW = symNumWW;
                    int tempLineNumWW = lineNumWW;
                    getChar();
                    while (char.IsLetterOrDigit(ch))
                    {
                        lettersAccumulator.Append(ch);
                        tempSymNumWW = symNumWW;
                        tempLineNumWW = lineNumWW;
                        getChar();
                    }
                    string buf = lettersAccumulator.ToString();
                    int z = Tables[WORDS].FindIndex(buf);
                    Output.Add(z != -1 ? createLexem(WORDS, z, buf, tempLineNumWW, tempSymNumWW) : registerLexem(IDENTIFIERS, buf, buf, tempLineNumWW, tempSymNumWW));
                    continue;
                }
                if (ch >= '0' && ch <= '9')
                {
                    parseNumber();
                    continue;
                }
                if (char.IsWhiteSpace(ch) || ch == '\0')
                    continue;
                //TODO: что делать с многосимвольными разделителями?
                Output.Add(findLexem(SEPARATORS, ch.ToString(), 11, lineNumWW, symNumWW));
                getChar();
            }
            PostProcess();
            for (int i = 0; i < Output.Count; i++)
                Console.WriteLine(String.Format("{0} at {1}", Output[i].Display, Output[i].LineNumber));
        }

        /// <summary>
        /// Препроцессинг исходного кода
        /// Заменяет комментарии на пробелы
        /// </summary>
        internal void PreProcess()
        {
            int beg, i, nd, j;
            int minbeg = 0, mini = -1;
            StringBuilder sb = new StringBuilder(source);
            while (true)
            {                
                for (i = 0; i < commentsList.Count; i++)
                {
                    beg = source.IndexOf(commentsList[i].Begin);
                    if (beg < 0)
                        continue;
                    if (mini == -1 || minbeg < beg)
                    {
                        mini = i;
                        minbeg = beg;
                    }
                }
                if (mini == -1)
                    break;
                nd = source.IndexOf(commentsList[mini].End, minbeg+1);
                if (nd < 0)
                    error(12);
                for (j = minbeg; j <= nd; j++)
                    if (!char.IsWhiteSpace(sb[j]))
                        sb[j] = ' ';
            }
            source = sb.ToString();
        }

        /// <summary>
        /// Создает запись о лексеме
        /// </summary>
        /// <param name="table">ИД таблицы</param>
        /// <param name="number">ИД в таблице</param>
        /// <param name="display">Строковое представление</param>
        private Lexem createLexem(int table, int number, string display, int lineNum, int symNum)
        {
            return new Lexem(table, number, display, lineNum, symNum-display.Length, symNum-1);
        }

        /// <summary>
        /// Производит поиск информации о лексеме
        /// </summary>
        /// <param name="tableId">ИД таблицы</param>
        /// <param name="toFind">Искомый объект</param>
        /// <param name="errorCode">Код генерируемой ошибки, если не найдено</param>
        private Lexem findLexem(int tableId, string toFind, int errorCode, int lineNum, int symNum)
        {
            int index = Tables[tableId].FindIndex(toFind);
            if (index < 0)
                error(errorCode);
            return createLexem(tableId, index, toFind, lineNum, symNum);
        }

        /// <summary>
        /// Регистрация новой лексемы
        /// </summary>
        /// <param name="tableId">ИД таблицы</param>
        /// <param name="objectToAdd">Добавляемый объект</param>
        private Lexem registerLexem(int tableId, object objectToAdd, string buf, int lineNum, int symNum)
        {
            return createLexem(tableId, Tables[tableId].SafeAdd(objectToAdd), buf, lineNum, symNum);
        }

        /// <summary>
        /// Чтение следущего символа
        /// </summary>
        /// <returns>\0 если нечего больше читать</returns>
        private void getChar()
        {
            if (charNum == source.Length)
            {
                ch = '\0';
                return;
            }
            if (source[charNum] == '\n')
            {
                lineNum++;
                symNum = 0;
            }
            ch = source[charNum];
            charNum++;
            symNum++;
            if (!char.IsWhiteSpace(ch) && ch != '\n')
            {
                lineNumWW = lineNum;
                symNumWW = symNum;
            }
        }

        /// <summary>
        /// Обработка числовых данных
        /// </summary>
        private void parseNumber()
        {
            StringBuilder digitsAccumulator = new StringBuilder();
            int tempSymNumWW = 0, tempLineNumWW = 0;
            while (char.IsDigit(ch))
            {
                digitsAccumulator.Append(ch);
                tempLineNumWW = lineNumWW;
                tempSymNumWW = symNumWW;
                getChar();
            }
            if (ch == '.' || ch == 'E' || ch == 'e')
            {
                digitsAccumulator.Append(ch);
                parseRealNumber(digitsAccumulator);
                return;
            }
            long n;
            if (!long.TryParse(digitsAccumulator.ToString(), out n))
                error(10);
            Output.Add(registerLexem(INTEGERS, n, digitsAccumulator.ToString(), tempLineNumWW, tempSymNumWW));
            //TODO: что делать с многосимвольными разделителями?
            if (Tables[SEPARATORS].FindIndex(ch.ToString()) < 0 && ch != ' ')
                error(12);
        }

        /// <summary>
        /// Обработка числа с признаками дробного
        /// </summary>
        /// <param name="digitsAccumulator">Уже накопленные символы числа</param>
        private void parseRealNumber(StringBuilder digitsAccumulator)
        {
            int tempSymNumWW = lineNumWW, tempLineNumWW = symNumWW;
            double d;
            if (ch == '.')
            {
                getChar();
                while (char.IsDigit(ch))
                {
                    digitsAccumulator.Append(ch);
                    tempLineNumWW = lineNumWW;
                    tempSymNumWW = symNumWW;
                    getChar();
                }
            }
            if (ch != 'E' && ch != 'e')
            {
                if (!double.TryParse(digitsAccumulator.ToString(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out d))
                    error(10);
                Output.Add(registerLexem(REALS, d, digitsAccumulator.ToString(), tempLineNumWW, tempSymNumWW));
                //TODO: что делать с многосимвольными разделителями?
                if (Tables[SEPARATORS].FindIndex(ch.ToString()) < 0 && ch != ' ')
                    error(12);
                return;
            }
            digitsAccumulator.Append(ch);
            tempLineNumWW = lineNumWW;
            tempSymNumWW = symNumWW;
            getChar();
            if (ch == '+' || ch == '-')
            {
                digitsAccumulator.Append(ch);
                tempLineNumWW = lineNumWW;
                tempSymNumWW = symNumWW;
                getChar();
            }
            while (char.IsDigit(ch))
            {
                digitsAccumulator.Append(ch);
                tempLineNumWW = lineNumWW;
                tempSymNumWW = symNumWW;
                getChar();
            }
            if (!double.TryParse(digitsAccumulator.ToString(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out d))
                error(10);
            Output.Add(registerLexem(REALS, d, digitsAccumulator.ToString(), tempLineNumWW, tempSymNumWW));
            //TODO: что делать с многосимвольными разделителями?
            if (Tables[SEPARATORS].FindIndex(ch.ToString()) < 0 && ch != ' ')
                error(12);
        }

        /// <summary>
        /// Постпроцессинг полученных лексем
        /// </summary>
        internal void PostProcess()
        {
            foreach (PostProcessInstruction pp in postProcessList)
            {
                string[] toFind = pp.IfFound.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string[] toReplace = pp.ReplaceBy.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                int limit = Output.Count - toFind.Length, i, j;
                bool theSame = toFind.Length == toReplace.Length;
                if (theSame)
                    for (j = 0; j < toFind.Length; j++)
                        if (toFind[j] != toReplace[j])
                            theSame = false;
                if (theSame)
                    continue;
                for (i = 0; i <= limit; i++)
                {
                    bool matches = true;
                    for (j = 0; j < toFind.Length; j++)
                    {
                        if (Output[i + j].Display != toFind[j])
                        {
                            matches = false;
                            break;
                        }
                    }
                    if (matches)
                    {
                        int commonBeginning = 0;
                        for (j = 0; j < toFind.Length; j++)
                        {
                            if (j == toReplace.Length || toFind[j] != toReplace[j])
                                break;
                            commonBeginning++;
                        }
                        int commonEnding = 0;
                        for (j = 0; j < toFind.Length; j++)
                        {
                            if (j == toReplace.Length || toFind[toFind.Length - j - 1] != toReplace[toReplace.Length - j - 1])
                                break;
                            commonEnding++;
                        }
                        int lineNumber = Output[i + toFind.Length - commonEnding - 1].LineNumber;
                        int columnNumber = Output[i + toFind.Length - commonEnding - 1].BeginColumnNumber;
                        for (j = commonBeginning; j < toFind.Length - commonEnding; j++)
                            Output.RemoveAt(i + commonBeginning);
                        for (j = commonBeginning; j < toReplace.Length - commonEnding; j++)
                        {
                            int tableId = WORDS;
                            int index = Tables[tableId].FindIndex(toReplace[j]);
                            if (index < 0)
                            {
                                tableId = SEPARATORS;
                                index = Tables[tableId].FindIndex(toReplace[j]);
                                if (index < 0)
                                    throw new Exception("Preprocessing failed.");
                            }
                            Output.Insert(i + j, new Lexem(tableId, index, toReplace[j], lineNumber, columnNumber, columnNumber));
                        }
                        limit += toReplace.Length - toFind.Length;
                        i += toReplace.Length - 1;
                    }
                }
            }
        }

        /// <summary>
        /// Генерирует ошибку
        /// </summary>
        private void error(int number)
        {
            string detail = "Неизвестная ошибка";
            switch (number)
            {
                case 10:
                    detail = "Найдено неправильное число";
                    break;
                case 11:
                    detail = "Найден неизвестный символ";
                    break;
                case 12:
                    detail = "Незакрытый комментарий";
                    break;
            }
            throw new Exception(String.Format("Лексическая ошибка в строке {0}, позиция {1}: {2}", lineNum, symNum, detail));
        }
    }
}
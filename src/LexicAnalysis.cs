using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
        /// Ошибка лексического анализа
        /// </summary>
        public string ErrorMsg { get; set; }

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
        /// Номер текущего символа в разбираемом коде
        /// </summary>
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
            try
            {
                // заполняем таблицы
                Tables[WORDS].AddRange(grammar.ReservedWords);
                Tables[SEPARATORS].AddRange(grammar.Separators);
                postProcessList = grammar.PostProcessList;
                commentsList = grammar.CommentsList;

                // считываем исходный код
                source = string.Join("\n", File.ReadAllLines(fileName));
                charNum = 0;
                lineNum = symNum = 1;

                // пре-обработка (в частности, удаление комментариев)
                PreProcess();

                // запуск основного цикла
                getChar();
                while (ch != '\0')
                    LexicalLoop();

                // пост-обработка полученных лексем
                PostProcess();
            }
            catch (Exception e)
            {
                ErrorMsg = e.Message;
            }
        }

        /// <summary>
        /// Цикл считывания лексем
        /// </summary>
        private void LexicalLoop()
        {
            while (char.IsWhiteSpace(ch))
                getChar();
            if (char.IsLetter(ch))
            {
                // лексема начинается с буквы - накапливаем буквы, цифры и проверяем в конце что вышло
                StringBuilder lettersAccumulator = new StringBuilder();
                lettersAccumulator.Append(ch);
                int prevSymNum = symNum;
                int prevLineNum = lineNum;
                getChar();
                while (char.IsLetterOrDigit(ch))
                {
                    lettersAccumulator.Append(ch);
                    prevSymNum = symNum;
                    prevLineNum = lineNum;
                    getChar();
                }
                string buf = lettersAccumulator.ToString();
                int z = Tables[WORDS].FindIndex(buf);
                Output.Add(z != -1 ? createLexem(WORDS, z, buf, prevLineNum, prevSymNum)
                                   : registerLexem(IDENTIFIERS, buf, buf, prevLineNum, prevSymNum));
                return;
            }
            if (ch >= '0' && ch <= '9')
            {
                // начинается с цифры - парсим число
                parseNumber();
                return;
            }
            if (ch == '\0') // конец файла
                return;
            // нашли разделитель
            Output.Add(findLexem(SEPARATORS, ch.ToString(), 11, ": " + ch, lineNum, symNum));
            getChar();
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
                    error(12, string.Empty);
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
        /// <param name="endLineNum">Номер строки (где кончилась лексема)</param>
        /// <param name="endColNum">Номер колонки (где кончилась лексема)</param>
        private Lexem createLexem(int table, int number, string display, int endLineNum, int endColNum)
        {
            return new Lexem(table, number, display, endLineNum, endColNum-display.Length, endColNum-1);
        }

        /// <summary>
        /// Производит поиск информации о лексеме
        /// </summary>
        /// <param name="tableId">ИД таблицы</param>
        /// <param name="toFind">Искомый объект</param>
        /// <param name="errorCode">Код генерируемой ошибки, если не найдено</param>
        /// <param name="endLineNum">Номер строки (где кончилась лексема)</param>
        /// <param name="endColNum">Номер колонки (где кончилась лексема)</param>
        private Lexem findLexem(int tableId, string toFind, int errorCode, string errorDesc, int endLineNum, int endColNum)
        {
            int index = Tables[tableId].FindIndex(toFind);
            if (index < 0)
                error(errorCode, errorDesc);
            return createLexem(tableId, index, toFind, endLineNum, endColNum);
        }

        /// <summary>
        /// Регистрация новой лексемы
        /// </summary>
        /// <param name="tableId">ИД таблицы</param>
        /// <param name="objectToAdd">Добавляемый объект</param>
        /// <param name="buf">Строковое представление лексемы</param>
        /// <param name="endLineNum">Номер строки (где кончилась лексема)</param>
        /// <param name="endColNum">Номер колонки (где кончилась лексема)</param>
        private Lexem registerLexem(int tableId, object objectToAdd, string buf, int endLineNum, int endColNum)
        {
            return createLexem(tableId, Tables[tableId].SafeAdd(objectToAdd), buf, endLineNum, endColNum);
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
        }

        /// <summary>
        /// Обработка числовых данных
        /// </summary>
        private void parseNumber()
        {
            StringBuilder digitsAccumulator = new StringBuilder();
            int prevSymNum = 0, prevLineNum = 0;
            while (char.IsDigit(ch))
            {
                digitsAccumulator.Append(ch);
                prevLineNum = lineNum;
                prevSymNum = symNum;
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
                error(10, ": " + digitsAccumulator);
            Output.Add(registerLexem(INTEGERS, n, digitsAccumulator.ToString(), prevLineNum, prevSymNum));
            //TODO: что делать с многосимвольными разделителями?
            if (Tables[SEPARATORS].FindIndex(ch.ToString()) < 0 && ch != ' ')
                error(11, ": " + ch);
        }

        /// <summary>
        /// Обработка числа с признаками дробного
        /// </summary>
        /// <param name="digitsAccumulator">Уже накопленные символы числа</param>
        private void parseRealNumber(StringBuilder digitsAccumulator)
        {
            int prevSymNum = lineNum, prevLineNum = symNum;
            double d;
            if (ch == '.')
            {
                getChar();
                while (char.IsDigit(ch))
                {
                    digitsAccumulator.Append(ch);
                    prevLineNum = lineNum;
                    prevSymNum = symNum;
                    getChar();
                }
            }
            if (ch != 'E' && ch != 'e')
            {
                if (!double.TryParse(digitsAccumulator.ToString(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out d))
                    error(10, ": " + digitsAccumulator);
                Output.Add(registerLexem(REALS, d, digitsAccumulator.ToString(), prevLineNum, prevSymNum));
                //TODO: что делать с многосимвольными разделителями?
                if (Tables[SEPARATORS].FindIndex(ch.ToString()) < 0 && ch != ' ')
                    error(11, ": " + ch);
                return;
            }
            digitsAccumulator.Append(ch);
            prevLineNum = lineNum;
            prevSymNum = symNum;
            getChar();
            if (ch == '+' || ch == '-')
            {
                digitsAccumulator.Append(ch);
                prevLineNum = lineNum;
                prevSymNum = symNum;
                getChar();
            }
            while (char.IsDigit(ch))
            {
                digitsAccumulator.Append(ch);
                prevLineNum = lineNum;
                prevSymNum = symNum;
                getChar();
            }
            if (!double.TryParse(digitsAccumulator.ToString(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out d))
                error(10, ": " + digitsAccumulator);
            Output.Add(registerLexem(REALS, d, digitsAccumulator.ToString(), prevLineNum, prevSymNum));
            //TODO: что делать с многосимвольными разделителями?
            if (Tables[SEPARATORS].FindIndex(ch.ToString()) < 0 && ch != ' ')
                error(11, ": " + ch);
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
        private void error(int number, string description)
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
            throw new Exception(String.Format("Лексическая ошибка в строке {0}, позиция {1}: {2} {3}", lineNum, symNum-1, detail, description));
        }
    }
}
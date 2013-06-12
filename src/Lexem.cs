namespace AnyParser
{
    /// <summary>
    /// Лексема
    /// </summary>
    public class Lexem
    {
        /// <summary>
        /// ИД таблицы
        /// </summary>
        public readonly int Table;

        /// <summary>
        /// Номер в таблице
        /// </summary>
        public readonly int Index;

        /// <summary>
        /// Номер строки в исходном коде
        /// </summary>
        public readonly int LineNumber;

        /// <summary>
        /// Позиция в строке (начало лексемы)
        /// </summary>
        public readonly int BeginColumnNumber;

        /// <summary>
        /// Позиция в строке (конец лексемы)
        /// </summary>
        public readonly int EndColumnNumber;

        /// <summary>
        /// Текстовое представление
        /// </summary>
        public readonly string Display;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="table">ИД таблицы</param>
        /// <param name="index">Номер в таблице</param>
        /// <param name="display">Текстовое представление</param>
        /// <param name="lineNumber">Номер строки в исходном коде</param>
        public Lexem(int table, int index, string display, int lineNumber, int beginColumnNumber, int endColumnNumber)
        {
            Table = table;
            Index = index;
            Display = display;
            LineNumber = lineNumber;
            BeginColumnNumber = beginColumnNumber;
            EndColumnNumber = endColumnNumber;
        }
    }
}
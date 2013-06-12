namespace AnyParser
{
    /// <summary>
    /// �������
    /// </summary>
    public class Lexem
    {
        /// <summary>
        /// �� �������
        /// </summary>
        public readonly int Table;

        /// <summary>
        /// ����� � �������
        /// </summary>
        public readonly int Index;

        /// <summary>
        /// ����� ������ � �������� ����
        /// </summary>
        public readonly int LineNumber;

        /// <summary>
        /// ������� � ������ (������ �������)
        /// </summary>
        public readonly int BeginColumnNumber;

        /// <summary>
        /// ������� � ������ (����� �������)
        /// </summary>
        public readonly int EndColumnNumber;

        /// <summary>
        /// ��������� �������������
        /// </summary>
        public readonly string Display;

        /// <summary>
        /// �����������
        /// </summary>
        /// <param name="table">�� �������</param>
        /// <param name="index">����� � �������</param>
        /// <param name="display">��������� �������������</param>
        /// <param name="lineNumber">����� ������ � �������� ����</param>
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
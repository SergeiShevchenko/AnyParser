using System.Collections.Generic;

namespace AnyParser
{
    /// <summary>
    /// ������� ��������� ������� (������������ ������)
    /// </summary>
    public class RuleVariant : List<SyntaxItem>
    {
        /// <summary>
        /// ��������� ������������� ��������
        /// </summary>
        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}
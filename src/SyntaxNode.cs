using System.Collections.Generic;

namespace AnyParser
{
    /// <summary>
    /// Узел синтаксического разбора
    /// </summary>
    public class SyntaxNode
    {
        /// <summary>
        /// Описание
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string ErrorMsg { get; set; }

        /// <summary>
        /// Дочерние узлы
        /// </summary>
        public List<SyntaxNode> Children { get; set; }

        /// <summary>
        /// Тип узла
        /// </summary>
        public SyntaxNodeType SyntaxNodeType { get; set; }

        /// <summary>
        /// Номер начальной лексемы
        /// </summary>
        public int BeginLexem { get; set; }

        /// <summary>
        /// Номер конечной лексемы
        /// </summary>
        public int EndLexem { get; set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="desc">Описание узла</param>
        public SyntaxNode(string desc)
        {
            Desc = desc;
            Children = new System.Collections.Generic.List<SyntaxNode>();
            SyntaxNodeType = SyntaxNodeType.None;
            BeginLexem = EndLexem = -1;
        }
    }
}

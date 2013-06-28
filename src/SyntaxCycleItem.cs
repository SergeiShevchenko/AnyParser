using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnyParser
{
    public class SyntaxCycleItem : SyntaxItem
    {
        /// <summary>
        /// Список однородных синтаксических элементов
        /// </summary>
        public List<SyntaxItem> List { get; set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        public SyntaxCycleItem()
            : base(SyntaxItemType.Cycle, "Cycle")
        {
            List = new List<SyntaxItem>();
        }

        /// <summary>
        /// Возвращает первый элемент списка
        /// </summary>
        public override SyntaxItem GetFirst()
        {
            return List.GetFirst();
        }
    }
}

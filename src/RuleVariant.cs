using System.Collections.Generic;

namespace AnyParser
{
    /// <summary>
    /// ¬ариант раскрыти€ правила (совокупность лексем)
    /// </summary>
    public class RuleVariant : List<SyntaxItem>
    {
        /// <summary>
        /// “екстовое представление варианта
        /// </summary>
        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}
namespace AnyParser
{
    /// <summary>
    /// Лексема в пределах правой части правила
    /// </summary>
    public class SyntaxItem
    {
        private SyntaxItemType type;
        private string text;

        /// <summary>
        /// Тип (терминал, нетерминал, циклический список)
        /// </summary>
        public SyntaxItemType Type
        {
            get { return type; }
            set { type = value; }
        }

        /// <summary>
        /// Текст лексемы
        /// </summary>
        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        public SyntaxItem()
        {
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="type">Тип (терминал или нетерминал)</param>
        /// <param name="text">Текст лексемы</param>
        public SyntaxItem(SyntaxItemType type, string text)
        {
            this.type = type;
            this.text = text;
        }

        public override string ToString()
        {
            return Text;
        }

        public virtual SyntaxItem GetFirst()
        {
            return this;
        }
    }
}

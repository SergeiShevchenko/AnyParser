namespace AnyParser
{
    /// <summary>
    /// “ип (терминал, нетерминал, циклический список)
    /// </summary>
    public enum SyntaxItemType
    {
        Terminal = 1,
        NonTerminal,
        Cycle
    }
}
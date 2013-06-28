using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnyParser
{
    public static class SyntaxItemListExtension
    {
        public static SyntaxItem GetFirst(this IList<SyntaxItem> list)
        {
            return list[0].GetFirst();
        }
    }
}

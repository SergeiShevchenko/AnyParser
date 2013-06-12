using System.Collections.Generic;

namespace AnyParser
{
    /// <summary>
    /// Таблица элементов (идентификаторов, чисел и т.п.)
    /// </summary>
    public class Table : List<object>
    {
        /// <summary>
        /// Поиск индекса, с которым входит заданный объект
        /// </summary>
        /// <param name="item">Объект для поиска</param>
        /// <returns>-1, если объект не найден</returns>
        public int FindIndex(object item)
        {
            for (int i = 0; i < Count; i++)
                if (this[i].Equals(item))
                    return i;
            return -1;
        }

        /// <summary>
        /// Добавление элемента с проверкой, если он уже существует, тогда не добавляется.
        /// </summary>
        /// <param name="item">Элемент для добавления</param>
        /// <returns>Индекс элемента</returns>
        public int SafeAdd(object item)
        {
            int t = FindIndex(item);
            if (t != -1) 
                return t;
            Add(item);
            return Count-1;
        }
    }
}

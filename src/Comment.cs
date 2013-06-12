using System;
using System.Xml.Serialization;

namespace AnyParser
{
    [Serializable]
    /// Информация о типе комментариев
    public class Comment
    {
        /// <summary>
        /// Открытие комментария
        /// </summary>
        [XmlAttribute]
        public string Begin { get; set; }
        /// <summary>
        /// Закрытие комментария
        /// </summary>
        [XmlAttribute]
        public string End { get; set; }
    }
}
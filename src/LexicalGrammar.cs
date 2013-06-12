using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace AnyParser
{
    [Serializable]
    public class LexicalGrammar
    {
        /// <summary>
        /// Правила построения комментариев
        /// </summary>
        public List<Comment> CommentsList;

        /// <summary>
        /// Зарезервированные слова
        /// </summary>
        public string[] ReservedWords;

        /// <summary>
        /// Разделители
        /// </summary>
        public string[] Separators;

        /// <summary>
        /// Правила постпроцессинга полученных лексем
        /// </summary>
        public List<PostProcessInstruction> PostProcessList;

        /// <summary>
        /// Производит инициализацию описания грамматики из файла
        /// </summary>
        public static LexicalGrammar Read(string fileName)
        {
            return (LexicalGrammar)new XmlSerializer(typeof(LexicalGrammar)).Deserialize(new StreamReader(fileName));
        }
    }
}

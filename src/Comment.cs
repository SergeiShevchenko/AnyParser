using System;
using System.Xml.Serialization;

namespace AnyParser
{
    [Serializable]
    /// ���������� � ���� ������������
    public class Comment
    {
        /// <summary>
        /// �������� �����������
        /// </summary>
        [XmlAttribute]
        public string Begin { get; set; }
        /// <summary>
        /// �������� �����������
        /// </summary>
        [XmlAttribute]
        public string End { get; set; }
    }
}
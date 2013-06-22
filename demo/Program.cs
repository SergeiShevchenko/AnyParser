using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AnyParser;

namespace AnyParserDemo
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            LexicAnalysis LA = new LexicAnalysis("prog.txt", LexicalGrammar.Read("lexic.xml"));
            SyntaxAnalysis SA = new SyntaxAnalysis(LA, SyntaxGrammar.Read("syntax.xml"));
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FirstForm(SA.MainNode, LA));
        }
    }
}

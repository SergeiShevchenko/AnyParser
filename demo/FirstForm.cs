using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AnyParser;

namespace AnyParserDemo
{
    public partial class FirstForm : Form
    {
        LexicAnalysis lexic;

        public FirstForm(SyntaxNode sn, LexicAnalysis lexic)
        {
            InitializeComponent();
            this.lexic = lexic;
            addNodes(sn, treeView1.Nodes.Add(""));
        }

        void addNodes(SyntaxNode sn, TreeNode tn)
        {
            tn.Text = sn.Desc;
            if (sn.SyntaxNodeType == SyntaxNodeType.Success)
            {
                tn.ForeColor = Color.Green;
                tn.Text = "✔" + tn.Text;
            }
            else if (sn.SyntaxNodeType == SyntaxNodeType.Failure)
            {
                tn.ForeColor = Color.Red;
            }
            if (sn.EndLexem >= 0)
            {
                StringBuilder sb = new StringBuilder();
               
                    sb.AppendFormat("Begins at: line {0}, column {1}; ends at line {2}, column {3}.\n",
                        lexic.Output[sn.BeginLexem].LineNumber, lexic.Output[sn.BeginLexem].BeginColumnNumber,
                        lexic.Output[sn.EndLexem].LineNumber, lexic.Output[sn.EndLexem].EndColumnNumber);
                
                for (int i = sn.BeginLexem; i <= sn.EndLexem; i++)
                    sb.AppendFormat("{0} ", lexic.Output[i].Display);
                tn.Tag = sb.ToString();
            }
            foreach (var a in sn.Children)
                addNodes(a, tn.Nodes.Add(""));
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var n = treeView1.SelectedNode;
            if (n == null)
                label1.Text = "";
            var t = n.Tag;
            if (t == null)
                label1.Text = "";
            label1.Text = (string)t;
        }
    }
}

﻿using ScintillaNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;


namespace XMLLexerRead
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        XDocument document;
        XDocument documentModel;

        private void mnuOpen_Click(object sender, EventArgs e)
        {
            if (odXML.ShowDialog() == DialogResult.OK)
            {
                document = XDocument.Load(odXML.FileName);

                documentModel = XDocument.Load(Path.Combine(Path.GetDirectoryName(odXML.FileName), "langs.model.xml"));

                foreach (XElement element in document.Descendants("LexerType"))
                {
                    cmbLanguage.Items.Add(element.Attribute("name").Value);
                }
                cmbLanguage.SelectedIndex = 0;
            }
        }

        private void cmbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            tbScintillaRead.Clear();


            tbScintillaRead.Text +=
                "scintilla.StyleResetDefault();" + Environment.NewLine +
                "scintilla.Styles[Style.Default].Font = \"Consolas\";" + Environment.NewLine +
                "scintilla.Styles[Style.Default].Size = 10;" + Environment.NewLine +
                "scintilla.StyleClearAll();" + Environment.NewLine + Environment.NewLine;

            ComboBox comboBox = (ComboBox)sender;
            string item = comboBox.SelectedItem.ToString();

            List < Color > colorsColors = new List<Color>(new Color[] { });

            string colorArrayName = $"{item}Colors";
            string colorsCs = $"List<Color> {colorArrayName} = new List<Color>(new Color[]{Environment.NewLine}" +  " { " + Environment.NewLine;

            int colorIndex = 0;

            var nodes = document.Descendants("LexerType").Where(f => f.Attribute("name").Value == item);
            foreach (var node in nodes)
            {
                foreach (XElement element in node.Descendants("WordsStyle"))
                {
                    string fgColor = "#" + element.Attribute("fgColor").Value;
                    string bgColor = "#" + element.Attribute("bgColor").Value;
                    Color colorForeground = ColorTranslator.FromHtml(fgColor);
                    Color colorBackground = ColorTranslator.FromHtml(bgColor);

                    string fontStyle = "0";
                    if (element.Attribute("fontStyle") != null)
                    {
                        fontStyle = element.Attribute("fontStyle").Value;
                    }


                    colorsCs +=
                        $"Color.FromArgb({colorForeground.R}, {colorForeground.G}, {colorForeground.B}), // {fgColor} " + Environment.NewLine +
                        $"Color.FromArgb({colorBackground.R}, {colorBackground.G}, {colorBackground.B}), // {fgColor} " + Environment.NewLine;

                    tbScintillaRead.Text +=
                        $"// {element.Attribute("name").Value}, fontStyle = {fontStyle} " + Environment.NewLine +
                    $"scintilla.Styles[{item}.{element.Attribute("name").Value}].ForeColor = {colorArrayName}[{colorIndex++}];" + Environment.NewLine +
                    $"scintilla.Styles[{item}.{element.Attribute("name").Value}].BackColor = {colorArrayName}[{colorIndex++}];" + Environment.NewLine;
                }
            }
            colorsCs += "});" + Environment.NewLine + Environment.NewLine;

            tbScintillaRead.Text = colorsCs + tbScintillaRead.Text;

            tbScintillaRead.Text += "scintilla.Lexer = Lexer." + item + ";" + Environment.NewLine + Environment.NewLine;

            nodes = documentModel.Descendants("Language").Where(f => f.Attribute("name").Value == item);

            int keywordIndex = 0;
            foreach(var node in nodes)
            {
                var subNodes = node.Descendants("Keywords");
                foreach (var subNode in subNodes)
                {
                    tbScintillaRead.Text += "// Name: " + subNode.Attribute("name").Value + Environment.NewLine;
                    tbScintillaRead.Text += $"scintilla.SetKeywords({keywordIndex++}, \"" + subNode.Value + "\");" + Environment.NewLine;
                }
            }
        }

        private void describeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Scintilla scintilla = new Scintilla();
            scintilla.Lexer = Lexer.Cpp;
            MessageBox.Show(scintilla.DescribeKeywordSets());
        }
    }
}

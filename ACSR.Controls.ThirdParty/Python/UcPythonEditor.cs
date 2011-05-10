using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ACSR.Controls.ThirdParty.Python
{
    public partial class UcPythonEditor : UserControl
    {
        public string CodeAsString
        {
            get
            {
                return scintilla1.Text;
            }
            set
            {
                scintilla1.Text = value;
            }
        }
        public UcPythonEditor()
        {
            InitializeComponent();
            scintilla1.TextInserted += new EventHandler<ScintillaNet.TextModifiedEventArgs>(scintilla1_TextInserted);
            scintilla1.KeyUp += new KeyEventHandler(scintilla1_KeyUp);
            scintilla1.KeyDown += new KeyEventHandler(scintilla1_KeyDown);

        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Return)
            {
                return false;
            }
            return true;
        }
        const int VK_SEMICOLON = 186;

        void HandleKeyDown(KeyEventArgs e)
        {
            switch ((int)e.KeyCode)
            {
                case (VK_SEMICOLON):
                    {

                        if (e.Modifiers == Keys.Shift)
                        {
                            _indentNextLine = true;
                        }

                    }
                    break;
                default:
                    if (e.Modifiers != Keys.Shift && e.KeyCode != Keys.Return)
                        _indentNextLine = false;
                    break;
            }
         
        }

        void HandleKeyPress(KeyEventArgs e)
        {
            
            switch ((int)e.KeyCode)
            {
                case (int) Keys.Return:
                    {
                           if (_indentNextLine)
                            {
                                scintilla1.NativeInterface.Tab();
                                _indentNextLine = false;
                            }
                    }
                    break;
               
            }
        }

        protected override bool ProcessKeyPreview(ref Message m)
        {
            return base.ProcessKeyPreview(ref m);

            const int WM_KEYDOWN = 0x100;
            const int WM_SYSKEYDOWN = 0x104;
            int ikey = (int)((long)m.WParam);
            Keys pressedkey = (Keys)ikey;
            var e = new KeyEventArgs(pressedkey | ModifierKeys);
            var result = base.ProcessKeyPreview(ref m);
            if ((m.Msg == WM_KEYDOWN) || (m.Msg == WM_SYSKEYDOWN))
            {
                HandleKeyPress(e);    
            }
            return result;
            
        }

        void scintilla1_KeyDown(object sender, KeyEventArgs e)
        {
            HandleKeyDown(e);
        }


        void scintilla1_KeyUp(object sender, KeyEventArgs e)
        {
            HandleKeyPress(e);
            return;
            if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Enter)
            {
                if (_indentNextLine)
                {
                    //scintilla1.InsertText("<TAB>");
                    scintilla1.NativeInterface.Tab();
                    _indentNextLine = false;
                    
                }
            }
            e.Handled = false;
        }

        void scintilla1_TextInserted(object sender, ScintillaNet.TextModifiedEventArgs e)
        {
            return;

            if (e.Text.Length > 0)
            {
                char lastChar = e.Text[e.Text.Length - 1];
                if (lastChar != '\n' && lastChar != ' ')
                {

                    _indentNextLine = lastChar == ':';
                }
            }
        }
        bool _indentNextLine = false;
      


       
    }
}

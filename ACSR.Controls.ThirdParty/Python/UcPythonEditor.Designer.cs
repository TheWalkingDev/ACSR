namespace ACSR.Controls.ThirdParty.Python
{
    partial class UcPythonEditor
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.scintilla1 = new ScintillaNet.Scintilla();
            ((System.ComponentModel.ISupportInitialize)(this.scintilla1)).BeginInit();
            this.SuspendLayout();
            // 
            // scintilla1
            // 
            this.scintilla1.ConfigurationManager.Language = "python";
            this.scintilla1.Indentation.ShowGuides = true;
            this.scintilla1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scintilla1.Location = new System.Drawing.Point(0, 0);
            this.scintilla1.Name = "scintilla1";
            this.scintilla1.Size = new System.Drawing.Size(550, 268);
            this.scintilla1.Styles.BraceBad.FontName = "Verdana";
            this.scintilla1.Styles.BraceLight.FontName = "Verdana";
            this.scintilla1.Styles.ControlChar.FontName = "Verdana";
            this.scintilla1.Styles.Default.FontName = "Verdana";
            this.scintilla1.Styles.IndentGuide.FontName = "Verdana";
            this.scintilla1.Styles.LastPredefined.FontName = "Verdana";
            this.scintilla1.Styles.LineNumber.FontName = "Verdana";
            this.scintilla1.Styles.Max.FontName = "Verdana";
            this.scintilla1.Margins.Margin0.Width = 20;
            this.scintilla1.Margins.Margin2.Width = 20;
            this.scintilla1.TabIndex = 4;
            this.scintilla1.Whitespace.Mode = ScintillaNet.WhitespaceMode.VisibleAfterIndent;
            this.scintilla1.Indentation.SmartIndentType = ScintillaNet.SmartIndent.Simple;
            // 
            // UcPythonEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.scintilla1);
            this.Name = "UcPythonEditor";
            this.Size = new System.Drawing.Size(550, 268);
            ((System.ComponentModel.ISupportInitialize)(this.scintilla1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private ScintillaNet.Scintilla scintilla1;

    }
}

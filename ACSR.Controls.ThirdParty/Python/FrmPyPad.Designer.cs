namespace ACSR.Controls.ThirdParty.Python
{
    partial class FrmPyPad
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ucPyPad1 = new ACSR.Controls.ThirdParty.Python.UcPyPad();
            this.SuspendLayout();
            // 
            // ucPyPad1
            // 
            this.ucPyPad1.CodeAsString = "";
            this.ucPyPad1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucPyPad1.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ucPyPad1.Location = new System.Drawing.Point(0, 0);
            this.ucPyPad1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ucPyPad1.Name = "ucPyPad1";
            this.ucPyPad1.Size = new System.Drawing.Size(763, 347);
            this.ucPyPad1.TabIndex = 0;
            // 
            // FrmPyPad
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(763, 347);
            this.Controls.Add(this.ucPyPad1);
            this.Name = "FrmPyPad";
            this.Text = "PyPad";
            this.ResumeLayout(false);

        }

        #endregion

        private UcPyPad ucPyPad1;
    }
}
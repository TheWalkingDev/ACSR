using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ACSR.Controls.ThirdParty.Python
{
    public partial class FrmPyPad : Form
    {
        public UcPyPad Control
        {
            get
            {
                return ucPyPad1;
            }
        }
        public FrmPyPad()
        {
            InitializeComponent();
        }
        
            
    }
}

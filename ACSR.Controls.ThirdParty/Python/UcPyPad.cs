using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ACSR.PythonScripting;
using System.Runtime.InteropServices;


namespace ACSR.Controls.ThirdParty.Python
{
    public delegate void CodeEvent (object sender, string Code);

    public partial class UcPyPad : UserControl
    {


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool PeekMessage([In, Out] ref MSG msg,
        HandleRef hwnd, int msgMin, int msgMax, int remove);
        public event CodeEvent OnCodeExecute;
        Dictionary<string, object> _variables;
        
        public UcPythonEditor Editor
        {
            get
            {
                return pythonEditor1;
            }
        }
        public UcPyPad()
        {
            this._variables = new Dictionary<string, object>();
            InitializeComponent();
            _specialNode = new TreeNode(_specialNodeName);
            treeViewVariables.BeforeExpand += new TreeViewCancelEventHandler(treeViewVariables_BeforeExpand);
            
        }
        private static readonly string _specialNodeName = "F2D00423-EA72-4CD8-BF73-E3587DF2B750";
        private TreeNode _specialNode;

        void AddNewNode(TreeNode parent, TreeNode child, NodeData data)
        {
            if (parent == null)
            {
                treeViewVariables.Nodes.Add(child);
            }
            else
            {
                parent.Nodes.Add(child);
            }
            child.Tag = data;
            child.Nodes.Add(_specialNodeName);
        }

        void treeViewVariables_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            var firstChild = e.Node.FirstNode;
            if (firstChild.Text.CompareTo(_specialNodeName) == 0)
            {
                e.Node.Nodes.Clear();
            }
            Dictionary<string, TreeNode> map = new Dictionary<string, TreeNode>();
            foreach (TreeNode node in e.Node.Nodes)
            {
                node.Tag = null; // unmapped
                map[node.Text] = node;
            }
            var objectToInspect = (NodeData)e.Node.Tag;
            var ctx = _engine.CreateScriptContext();
            ctx.SetVariable("targetObject", objectToInspect.Data);
            var list = ctx.ExecuteString("l = dir(targetObject); l.sort();\nl");
                //var list = ctx.GetGlobals();
                foreach (var l in list)
                {
                    string nodeName = l.ToString();
                    var nodeData = new NodeData(objectToInspect, l, l, ObjectKind.String); ;
                    if (map.ContainsKey(nodeName))
                    {
                        map[nodeName].Tag = nodeData;
                    }
                    else
                    {
                        AddNewNode(e.Node, new TreeNode(nodeName), nodeData);
                    }
                }

                treeViewVariables.BeginUpdate();
                try
                {
                    foreach (var de in map)
                    {
                        if (de.Value.Tag == null)
                        {
                            de.Value.Remove();
                        }
                    }
                }
                finally
                {
                    treeViewVariables.EndUpdate();
                }
               // AddNewNode(e.Node, new TreeNode("first child"));
               // AddNewNode(e.Node, new TreeNode("second child"));
            
        }

        public void SetVariable(string Name, object Value)
        {
            _variables[Name] = Value;
        }

        public string CodeAsString
        {
            get
            {
                return this.Editor.CodeAsString;
            }
            set
            {
                this.Editor.CodeAsString = value;
            }
        }

        public IScriptContext Compile(string Code)
        {
            try
            {
                _engine = new ScriptController(true);
                _context = _engine.CreateScriptContextFromString(Code, _context);
                foreach (var de in _variables)
                {
                    _context.SetVariable(de.Key, de.Value);
                }

                _engine.OnMessage += new MessageEvent(_engine_OnMessage);
                return _context;
            }
            catch (Exception e)
            {
                LogMessage(e.Message);
                return null;
            }
        }

        ScriptController _engine;
        IScriptContext _context;

        public void UpdateVariables()
        {
            //Compile(CodeAsString).Execute();
            Compile("").Execute();

            //treeViewVariables.Nodes.Clear();

            Dictionary<string, TreeNode> map = new Dictionary<string, TreeNode>();
            foreach (TreeNode node in treeViewVariables.Nodes)
            {
                node.Tag = null; // unmapped
                map[node.Text] = node;
            }
            
            foreach (var global in _context.GetGlobals())
            {
                string nodeName = global[0];
                var nodeData = new NodeData(null, global[1], global[0], ObjectKind.Object);
                if (map.ContainsKey(nodeName))
                {
                    map[nodeName].Tag = nodeData; // unmapped
                }
                else
                {
                    var treeNode = new TreeNode(nodeName);
                    AddNewNode(null, treeNode, nodeData);
                    map[nodeName] = treeNode;
                }

            }
            treeViewVariables.BeginUpdate();
            try
            {
                foreach (var de in map)
                {
                    if (de.Value.Tag == null)
                    {
                        de.Value.Remove();
                    }
                }
            }
            finally
            {
                treeViewVariables.EndUpdate();
            }
          
        }

        public void RunInteractiveCode(string Code)
        {
            LogMessage(">> " + Code + System.Environment.NewLine);
            RunScript(Code);
        }

        public void RunScript(string Code)
        {
            try
            {
                //LogMessage(">> " + Code + System.Environment.NewLine);
                Compile(Code);
                var retval = _context.Execute();
                if (retval != null)
                {
                    retval = _context.CreateLocalScope()
                        .SetVariable("expressionToEvaluate", retval)
                        .Evaluate("repr(expressionToEvaluate)");
                   
                    //retval = _context.EvaluateExpression("repr(expressionToEvaluate)", tempScope);
                   // _context.Scope.expressionToEvaluate = retval;
                    //Compile("repr(expressionToEvaluate)");
                    //retval = _context.Execute();
                    LogMessage(retval.ToString() + System.Environment.NewLine);
                }
                UpdateVariables();

            }
            catch (Exception e)
            {
                LogMessage(e.Message + System.Environment.NewLine);
            }
        }

       
        void LogMessage(string Message)
        {
           // richTextBox1.AppendText(Message + System.Environment.NewLine);
            richTextBox1.AppendText(Message);
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.ScrollToCaret();
        }
        void _engine_OnMessage(object sender, string Message)
        {
            LogMessage(Message);
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            RaiseUserRunScriptEvent();
           
        }

        void RaiseUserRunScriptEvent()
        {
            if (OnCodeExecute != null)
            {
                OnCodeExecute(this, CodeAsString);
            }
            RunScript(CodeAsString);
        }

        private void TrappedKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F5:
                    {
                        RaiseUserRunScriptEvent();

                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                    break;
                case Keys.O:
                    {
                        if (e.Modifiers == Keys.Control)
                        {

                            MessageBox.Show("open");
                        }
                    }
                    break;
                default:
                    {
                        e.Handled = false;
                        e.SuppressKeyPress = false;
                    }
                    break;
            }
    
        }

        private void pythonEditor1_KeyDown(object sender, KeyEventArgs e)
        {
            TrappedKeyDown(e);
        }

        private void RemovePendingMessages(int msgMin, int msgMax)
        {
            if (!this.IsDisposed)
            {
                MSG msg = new MSG();
                IntPtr handle = this.Handle;
                while (PeekMessage(ref msg,
                new HandleRef(this, handle), msgMin, msgMax, 1))
                {
                }
            }
        }
        protected override bool ProcessKeyPreview(ref Message m)
        {
            const int WM_KEYDOWN = 0x100;
           // const int WM_KEYUP = 0x101;
            const int WM_CHAR = 0x102;
            const int WM_SYSCHAR = 0x106;
            const int WM_SYSKEYDOWN = 0x104;
          //  const int WM_SYSKEYUP = 0x105;
            const int WM_IME_CHAR = 0x286;

            KeyEventArgs e = null;

            if ((m.Msg != WM_CHAR) && (m.Msg != WM_SYSCHAR) && (m.Msg != WM_IME_CHAR))
            {
                int ikey = (int)((long)m.WParam);
                Keys pressedkey = (Keys)ikey;
                e = new KeyEventArgs(pressedkey | ModifierKeys);
                if ((m.Msg == WM_KEYDOWN) || (m.Msg == WM_SYSKEYDOWN))
                {
                    TrappedKeyDown(e);
                }
                //else
                //{
                //    TrappedKeyUp(e);
                //}

                // Remove any WM_CHAR type messages if supresskeypress is true.
                if (e.SuppressKeyPress)
                {
                    this.RemovePendingMessages(WM_CHAR, WM_CHAR);
                    this.RemovePendingMessages(WM_SYSCHAR, WM_SYSCHAR);
                    this.RemovePendingMessages(WM_IME_CHAR, WM_IME_CHAR);
                }

                if (e.Handled)
                {
                    return e.Handled;
                }
            }
            return base.ProcessKeyPreview(ref m);
        }

        void RunInteractiveCodeAndUpdateCombobox(string code)
        {
            RunInteractiveCode(code);
            // string text = comboBox1.Text;
            var idx = comboBox1.Items.IndexOf(code);
            if (idx >= 0)
            {
                comboBox1.Items.RemoveAt(idx);
            }
            comboBox1.Items.Insert(0, code);
            comboBox1.Text = "";
        }

        private void comboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                RunInteractiveCodeAndUpdateCombobox(comboBox1.Text);
            }
        }

        private void treeViewVariables_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Return)
            {
                var nodeData = (NodeData)treeViewVariables.SelectedNode.Tag;


                RunInteractiveCodeAndUpdateCombobox(nodeData.ObjectName);
                return;

                var scope = _context.CreateLocalScope();
                object retVal;
                if (nodeData.Kind == ObjectKind.String)
                {

                    scope.SetVariable("objectToEvaluate", nodeData.Parent.Data);
                    retVal = scope.Evaluate(string.Format("repr(getattr(objectToEvaluate, '{0}')) ", nodeData.Data));
                }
                else
                {
                   retVal = scope.Evaluate("repr(objectToEvaluate)");
                }





                LogMessage(retVal.ToString() + System.Environment.NewLine);
            }
        }

    }

    class NodeData
    {
        object _data;

        public object Data
        {
            get { return _data; }
            set { _data = value; }
        }
        ObjectKind _kind;

        public ObjectKind Kind
        {
            get { return _kind; }
            set { _kind = value; }
        }

        NodeData _parent;
        string objectName;

        public string ObjectName
        {
            get
            {
                if (Parent != null)
                {
                    return Parent.ObjectName + "." + objectName;
                }
                else
                {
                    return objectName;
                }

            }
            
        }
        public NodeData Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }
        public NodeData(NodeData parent, object data, string objectName, ObjectKind kind)
        {
            _data = data;
            _kind = kind;
            _parent = parent;
            this.objectName = objectName;
            
        }
    }
    enum ObjectKind {
        Unknown = 0,
        Object = 1,
        String = 2
    }
    struct MSG
    {
        public IntPtr hwnd;
        public int message;
        public IntPtr wParam;
        public IntPtr lParam;
        public int time;
        public int pt_x;
        public int pt_y;
    }

}

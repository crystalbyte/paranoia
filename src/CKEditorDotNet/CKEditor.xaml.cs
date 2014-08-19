using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CKEditorDotNet
{
    // ReSharper disable once InconsistentNaming
    [ComVisible(true)]
    public partial class CKEditor : INotifyPropertyChanged
    {

        //private readonly EditorObjectForScripting _objectForScripting;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            if (handler != null)
                PropertyChanged(this, e);
        }

        private string _contentHtml;
        public string ContentHtml
        {
            get { return _contentHtml; }
            set
            {
                if (value == _contentHtml)
                    return;

                SetHtml(value);
                Debug.WriteLine(value);
                NotifyPropertyChanged(new PropertyChangedEventArgs("ContentHtml"));
            }
        }

        private bool _isReadOnly;
        public bool IsReadOnly
        {
            get
            {
                return _isReadOnly;
            }
            set
            {
                if (value == _isReadOnly)
                    return;

                NotifyPropertyChanged(new PropertyChangedEventArgs("IsReadOnly"));
                SetEditorReadonlyState(value);
                _isReadOnly = value;
            }
        }

        public CKEditor()
        {
            InitializeComponent();
            //_objectForScripting = new EditorObjectForScripting(this);
            EditorBrowser.Loaded += OnEditorBrowserLoaded;
        }

        private void OnEditorBrowserLoaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                EditorBrowser.Source = new Uri("about:blank");
                return;
            }

            //EditorBrowser.ObjectForScripting = _objectForScripting;

            var editorFile = Environment.CurrentDirectory + @"\..\..\..\CKEditorDotNet\Editor.html";
            if (!File.Exists(editorFile))
                throw new IOException("editorfile not found\n" + editorFile);

            var uri = new Uri(editorFile, UriKind.Absolute);
            EditorBrowser.Source = uri;
            Test();
        }

        private void Test()
        {
            //new Thread(() =>
            //{
            //    Thread.Sleep(3000);
            //    Dispatcher.Invoke(new Action(() =>
            //        EditorBrowser.ExecuteJavascript("Stuff")
            //    ));
            //}).Start();
        }

        private void SetEditorReadonlyState(bool readOnly)
        {
            EditorBrowser.ExecuteJavascript(string.Format("setEditorReadonlyState(\"{0}\")", readOnly));
        }

        #region EditorCommands

        public void Undo()
        {
            ExecuteCommand("undo");
        }

        public void Redo()
        {
            ExecuteCommand("redo");
        }

        public void Bold()
        {
            ExecuteCommand("bold");
        }

        public void Italic()
        {
            ExecuteCommand("italic");
        }

        public void Underline()
        {
            ExecuteCommand("underline");
        }

        public void Strike()
        {
            ExecuteCommand("strike");
        }

        public void Subscript()
        {
            ExecuteCommand("subscript");
        }

        public void Superscript()
        {
            ExecuteCommand("superscript");
        }

        private void ExecuteCommand(string command)
        {
            EditorBrowser.ExecuteJavascript(string.Format("execCommand(\"{0}\")", command));
        }

        public void SetHtml(string html)
        {
            _contentHtml = html;

            //if (_objectForScripting.EditorContent != html)
            //    EditorBrowser.InvokeScript("setEditorHtml", new object[] { html });
        }

        public string GetHtml()
        {
            var html = (string)EditorBrowser.ExecuteJavascriptWithResult("getEditorHtml()");
            return html;
        }

        #endregion

    }
}

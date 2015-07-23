using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;

namespace WebResourceLinkerVsix
{
    public class WebResourceLinkerCommandBase
    {
        public void GetDte2(DTE2 applicationObject)
        {
            this._applicationObject = applicationObject;
        }

        protected string[] _supportedFileTypes = new string[]
        {
            ".html", ".htm", ".js", ".css",
            ".gif", ".jpg", ".png", ".ico",
            ".xml", ".xsl", ".xslt",
            ".xap"
        };

        private DTE2 _applicationObject;

        private bool _publishing = false;

        protected bool IsCommandVisible(IEnumerable<SelectedItem> items)
        {
            if (items.Any(a =>
            {
                string ext = a.Name.ToLower();
                ext = ext.Substring(ext.LastIndexOf("."));

                return _supportedFileTypes.Contains(ext);
            }))
            {
                return true;
            }

            return false;
        }
        protected void HandleCodeWindowCommand(bool relinking)
        {
            if (_publishing)
            {
                WriteToOutputWindow("Publish in progress, please wait and try again when the publish is complete.");
                return;
            }

            string linkerDataPath = "";
            string ldpFormat = "{0}.webresourcelinker.xml"; // this is our mapping file, it gets stored in the project
            string solutionPath = Path.GetDirectoryName(_applicationObject.Solution.FullName);

            List<SelectedFile> selectedFiles = new List<SelectedFile>();

            // this handles a right click from a codewindow (eg: from the c#/js editor window)
            // todo: need to figure out how to support other editors like the css editor
            if (_applicationObject.ActiveWindow != null && _applicationObject.ActiveWindow.Project != null)
            {
                linkerDataPath = string.Format(ldpFormat, _applicationObject.ActiveWindow.Project.FullName);

                selectedFiles.Add(new SelectedFile
                {
                    FilePath = _applicationObject.ActiveWindow.Document.FullName,
                    FriendlyFilePath = GetFriendlyPath(solutionPath, _applicationObject.ActiveWindow.Document.FullName)
                });
            }
            else if (_applicationObject.SelectedItems != null)
            {
                var items = _applicationObject.SelectedItems.Cast<SelectedItem>().ToList();
                if (relinking && items.Count > 1) // todo: add support for multiple relinks later
                {
                    MessageBox.Show("Sorry, you can only re-link 1 web resource at a time");
                    return;
                }

                items.ForEach(item =>
                {
                    // vs has crazy logic, need to figure out the fullpath to the file so that we can correctly map files. just incase files within nested folders have the same name
                    string path = item.ProjectItem.Properties.Item("FullPath").Value.ToString();

                    if (items[0].Project != null)
                    {
                        linkerDataPath = string.Format(ldpFormat, item.Project.FullName);
                    }
                    else if (items[0].ProjectItem.ContainingProject != null)
                    {
                        linkerDataPath = string.Format(ldpFormat, item.ProjectItem.ContainingProject.FullName);
                    }

                    selectedFiles.Add(new SelectedFile { FilePath = path, FriendlyFilePath = GetFriendlyPath(solutionPath, path) });
                });
            }

            // sanity checks to make sure we dont crash
            if (!string.IsNullOrEmpty(linkerDataPath) && selectedFiles.Count > 0)
            {
                _publishing = true;
                new Controller(this).TryLinkOrPublish(linkerDataPath, selectedFiles, relinking);
            }
        }

        public void PublishingComplete()
        {
            _publishing = false; // clean up so we can run again next time
        }

        private string GetFriendlyPath(string solutionPath, string filePath)
        {
            return filePath.Replace(solutionPath, ""); // this is for the mappings xml, we'll take away all drive specifics so that if multiple devs are using the solution on different paths we still base it from where the solution dir started
        }

        private OutputWindowPane _outputWindow = null;
        public void WriteToOutputWindow(string format, params object[] args)
        {
            var paneName = "Web Resource Linker";

            if (_outputWindow == null)
            {
                Window window = _applicationObject.Windows.Item(Constants.vsWindowKindOutput);
                OutputWindow outputWindow = (OutputWindow)window.Object;
                _outputWindow = outputWindow.OutputWindowPanes.Cast<OutputWindowPane>()
                    .FirstOrDefault(pane => pane.Name.Equals(paneName, StringComparison.InvariantCultureIgnoreCase)) 
                    ?? outputWindow.OutputWindowPanes.Add("Web Resource Linker");
            }

            _outputWindow.OutputString(string.Format(format, args));
            _outputWindow.OutputString(Environment.NewLine);
        }
    }
}

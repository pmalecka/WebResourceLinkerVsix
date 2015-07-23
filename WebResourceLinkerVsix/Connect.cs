using System;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace WebResourceLinkerVsix
{
    //public class Connect : IDTExtensibility2, IDTCommandTarget
    public class Connect : IDTCommandTarget
    {
        private string[] _supportedFileTypes = new string[] 
        { 
            ".html", ".htm", ".js", ".css",
            ".gif", ".jpg", ".png", ".ico", 
            ".xml", ".xsl", ".xslt",
            ".xap"
        };

        // these are the buttons/menus we'll be adding to vs12
        private const string CODE_COMMAND_ID = "Code";
        private const string FILE_COMMAND_ID = "File";
        private const string FILE_COMMAND_RELINK_ID = "FileRelink";

        //// this holds the buttons we create, when the addin is shutdown we delete them, otherwise we start seeing duplicate buttons
        //private List<CommandBarControl> _buttons = new List<CommandBarControl>();

        public string GetCommandId(string command, bool withProgId)
        {
            return withProgId ? string.Format("{0}.{1}", _addInInstance.ProgID, command) : command;
        }

        public Connect()
        {
        }

        //public void OnConnection(object application, /*ext_ConnectMode connectMode,*/ object addInInst, ref Array custom)
        //{
        //	_applicationObject = (DTE2)application;
        //	_addInInstance = (AddIn)addInInst;
        //}

        //public void OnAddInsUpdate(ref Array custom)
        //{
        //}

        //public void OnStartupComplete(ref Array custom)
        //{
        //          AddButton("Code Window", CODE_COMMAND_ID, "Link/Publish Web Resource", "");
        //          AddButton("Item", FILE_COMMAND_ID, "Link/Publish Web Resource", "");
        //          AddButton("Item", FILE_COMMAND_RELINK_ID, "Re-link Web Resource", "");
        //}

        //public void OnDisconnection(/*ext_DisconnectMode disconnectMode,*/ ref Array custom)
        //{
        //    _buttons.ForEach(a => a.Delete());
        //}

        //public void OnBeginShutdown(ref Array custom)
        //{
        //}

        private DTE2 _applicationObject;
		private AddIn _addInInstance;

        public void Exec(string CmdName, vsCommandExecOption ExecuteOption, ref object VariantIn, ref object VariantOut, ref bool Handled)
        {
            //Handled = false;

            //if (ExecuteOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            //{
            //    if (CmdName.Equals(GetCommandId(CODE_COMMAND_ID, true), StringComparison.InvariantCultureIgnoreCase)
            //        || CmdName.Equals(GetCommandId(FILE_COMMAND_ID, true), StringComparison.InvariantCultureIgnoreCase)
            //        || CmdName.Equals(GetCommandId(FILE_COMMAND_RELINK_ID, true), StringComparison.InvariantCultureIgnoreCase))
            //    {
            //        bool relinking = CmdName.Equals(GetCommandId(FILE_COMMAND_RELINK_ID, true), StringComparison.InvariantCultureIgnoreCase);

            //        HandleCodeWindowCommand(relinking);
            //    }
            //}
        }

        // check and enable the menus/buttons if the file extension is supported
        public void QueryStatus(string CmdName, vsCommandStatusTextWanted NeededText, ref vsCommandStatus StatusOption, ref object CommandText)
        {
            if (NeededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
            {
                if (CmdName.StartsWith(_addInInstance.ProgID, StringComparison.InvariantCultureIgnoreCase))
                {
                    var status = vsCommandStatus.vsCommandStatusUnsupported;

                    if (CmdName.Equals(GetCommandId(CODE_COMMAND_ID, true), StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (_applicationObject.ActiveWindow != null && _applicationObject.ActiveWindow.Document != null)
                        {
                            string file = _applicationObject.ActiveWindow.Document.FullName.ToString().ToLower();
                            string ext = file.Substring(file.LastIndexOf("."));

                            if (_supportedFileTypes.Contains(ext))
                            {
                                status |= vsCommandStatus.vsCommandStatusEnabled | vsCommandStatus.vsCommandStatusSupported;
                            }
                        }
                    }
                    else if (CmdName.Equals(GetCommandId(FILE_COMMAND_ID, true), StringComparison.InvariantCultureIgnoreCase)
                        || CmdName.Equals(GetCommandId(FILE_COMMAND_RELINK_ID, true), StringComparison.InvariantCultureIgnoreCase))
                    {
                        var items = _applicationObject.SelectedItems.Cast<SelectedItem>();
                        if (items.Any(a =>
                        {
                            string ext = a.Name.ToLower();
                            ext = ext.Substring(ext.LastIndexOf("."));

                            return _supportedFileTypes.Contains(ext);
                        }))
                        {
                            status = vsCommandStatus.vsCommandStatusEnabled | vsCommandStatus.vsCommandStatusSupported;
                        }
                    }

                    StatusOption = status;
                }
            }
        }

        //private void AddButton(string location, string buttonId, string caption, string tooltip)
        //{
        //    Command command = null;
        //    CommandBarControl button;
        //    CommandBar commandBar;
        //    CommandBars commandBars;

        //    try
        //    {
        //        string commandName = GetCommandId(buttonId, true);
        //        try
        //        {
        //            command = _applicationObject.Commands.Item(commandName);
        //        }
        //        catch { }

        //        if (command == null)
        //        {
        //            command = _applicationObject.Commands.AddNamedCommand(_addInInstance, buttonId, caption, tooltip, true, 0, null, (int)(vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled));
        //        }

        //        commandBars = (CommandBars)_applicationObject.CommandBars;
        //        commandBar = commandBars[location];
        //        button = (CommandBarControl)command.AddControl(commandBar, commandBar.Controls.Count + 1);
        //        _buttons.Add(button);

        //        button.Caption = caption;
        //        button.TooltipText = tooltip;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //    }
        //}

        //private bool _publishing = false;
        //private void HandleCodeWindowCommand(bool relinking)
        //{
        //    if (_publishing) 
        //    {
        //        WriteToOutputWindow("Publish in progress, please wait and try again when the publish is complete.");
        //        return; 
        //    }

        //    string linkerDataPath = "";
        //    string ldpFormat = "{0}.webresourcelinker.xml"; // this is our mapping file, it gets stored in the project
        //    string solutionPath = Path.GetDirectoryName(_applicationObject.Solution.FullName);

        //    List<SelectedFile> selectedFiles = new List<SelectedFile>();
            
        //    // this handles a right click from a codewindow (eg: from the c#/js editor window)
        //    // todo: need to figure out how to support other editors like the css editor
        //    if (_applicationObject.ActiveWindow != null && _applicationObject.ActiveWindow.Project != null)
        //    {
        //        linkerDataPath = string.Format(ldpFormat, _applicationObject.ActiveWindow.Project.FullName);
                
        //        selectedFiles.Add(new SelectedFile
        //        {
        //            FilePath = _applicationObject.ActiveWindow.Document.FullName,
        //            FriendlyFilePath = GetFriendlyPath(solutionPath, _applicationObject.ActiveWindow.Document.FullName)
        //        });
        //    }
        //    else if (_applicationObject.SelectedItems != null)
        //    {
        //        var items = _applicationObject.SelectedItems.Cast<SelectedItem>().ToList();
        //        if (relinking && items.Count > 1) // todo: add support for multiple relinks later
        //        {
        //            MessageBox.Show("Sorry, you can only re-link 1 web resource at a time");
        //            return;
        //        }

        //        items.ForEach(item =>
        //            {
        //                // vs has crazy logic, need to figure out the fullpath to the file so that we can correctly map files. just incase files within nested folders have the same name
        //                string path = item.ProjectItem.Properties.Item("FullPath").Value.ToString();

        //                if (items[0].Project != null)
        //                {
        //                    linkerDataPath = string.Format(ldpFormat, item.Project.FullName);
        //                }
        //                else if (items[0].ProjectItem.ContainingProject != null)
        //                {
        //                    linkerDataPath = string.Format(ldpFormat, item.ProjectItem.ContainingProject.FullName);
        //                }

        //                selectedFiles.Add(new SelectedFile { FilePath = path, FriendlyFilePath = GetFriendlyPath(solutionPath, path) });
        //            });
        //    }

        //    // sanity checks to make sure we dont crash
        //    if (!string.IsNullOrEmpty(linkerDataPath) && selectedFiles.Count > 0)
        //    {
        //        _publishing = true;
        //        new Controller(this).TryLinkOrPublish(linkerDataPath, selectedFiles, relinking);
        //    }
        //}

        //public void PublishingComplete()
        //{
        //    _publishing = false; // clean up so we can run again next time
        //}

        //private string GetFriendlyPath(string solutionPath, string filePath)
        //{
        //    return filePath.Replace(solutionPath, ""); // this is for the mappings xml, we'll take away all drive specifics so that if multiple devs are using the solution on different paths we still base it from where the solution dir started
        //}

        //private OutputWindowPane _outputWindow = null;
        //public void WriteToOutputWindow(string format, params object[] args)
        //{
        //    if (_outputWindow == null)
        //    {
        //        Window window = _applicationObject.Windows.Item(Constants.vsWindowKindOutput);
        //        OutputWindow outputWindow = (OutputWindow)window.Object;
        //        _outputWindow = outputWindow.OutputWindowPanes.Add("Web Resource Linker");
        //    }

        //    _outputWindow.OutputString(string.Format(format, args));
        //    _outputWindow.OutputString(Environment.NewLine);
        //}
    }
}
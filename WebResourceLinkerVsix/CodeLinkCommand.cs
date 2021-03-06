﻿//------------------------------------------------------------------------------
// <copyright file="CodeLinkCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace WebResourceLinkerVsix
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CodeLinkCommand : WebResourceLinkerCommandBase
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0102;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("e2b282af-39d9-4355-844b-1538aefaedf8");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        private DTE2 _dte;
        //private Events _dteEvents;
        //private DocumentEvents _documentEvents;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeLinkCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private CodeLinkCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new OleMenuCommand(this.MenuItemCallback, menuCommandID);
                menuItem.BeforeQueryStatus += OnBeforeQueryStatus;
                commandService.AddCommand(menuItem);
            }

            _dte = (DTE2)this.ServiceProvider.GetService(typeof(DTE));
            //_dteEvents = _dte.Events;
            //_documentEvents = _dteEvents.DocumentEvents;

            base.GetDte2(_dte);
        }

        void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            var item = (OleMenuCommand)sender;
            item.Visible = false;

            if (_dte.ActiveWindow != null && _dte.ActiveWindow.Document != null)
            {
                string file = _dte.ActiveWindow.Document.FullName.ToString().ToLower();
                string ext = file.Substring(file.LastIndexOf("."));

                if (_supportedFileTypes.Contains(ext))
                {
                    item.Visible = true;
                }
            }

        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static CodeLinkCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new CodeLinkCommand(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            //    string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            //    string title = "CodeLinkCommand";

            //    // Show a message box to prove we were here
            //    VsShellUtilities.ShowMessageBox(
            //        this.ServiceProvider,
            //        message,
            //        title,
            //        OLEMSGICON.OLEMSGICON_INFO,
            //        OLEMSGBUTTON.OLEMSGBUTTON_OK,
            //        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            //}

            base.HandleCodeWindowCommand(false);
        }
    }
}

using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace WebResourceLinkerVsix
{
    public class Controller
    {
        public Controller()
        {
        }

        private WebResourceLinkerCommandBase _vsAddin = null; // this is used for writing back to the output window and setting flags to control single publish operations
        public Controller(WebResourceLinkerCommandBase vsAddin)
        {
            _vsAddin = vsAddin;
        }

        internal void TryLinkOrPublish(string linkerDataPath, List<SelectedFile> selectedFiles, bool relinking)
        {
            var linked = LinkerData.Get(linkerDataPath);

            string message = relinking ? "Initializing re-link on: {0}" : "Initializing link/publish on: {0}";
            Trace(message, linked.UniqueOrgName);

            WebResourcePublisher wrp = new WebResourcePublisher();
            wrp.Relink = relinking; // setting this will cause the wrp to mark the 1st file in selectedfiles to be relinked
            wrp.Controller = this;
            wrp.LinkerDataPath = linkerDataPath;
            wrp.SelectedFiles = selectedFiles;

            Task.Factory.StartNew(() =>
            {
                Trace("Connecting...");

                string publicUrl = "";
                IOrganizationService sdk = null;

                try
                {
                    sdk = QuickConnection.Connect(linked.DiscoveryUrl, linked.Domain, linked.Username, linked.Password, linked.UniqueOrgName, out publicUrl);
                }
                catch (Exception ex)
                {
                    Trace("Connection failed: {0}", ex.Message);
                }

                return new object[] { sdk, publicUrl };
            }).ContinueWith(state =>
            {
                object[] result = state.Result;

                var sdk = (IOrganizationService)result[0];

                wrp.Sdk = sdk;
                wrp.PublicUrl = result[1].ToString();
                wrp.ShowConnectionWindow = wrp.Sdk == null;

                wrp.Initialize();
                wrp.TryPublishing();

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void Cleanup()
        {
            if (this._vsAddin != null) { this._vsAddin.PublishingComplete(); }
        }

        internal static void SaveConnectionDetails(string linkerDataPath, string url, string org, string domain, string username, string password, string publicUrl)
        {
            try
            {
                var existing = LinkerData.Get(linkerDataPath);

                existing.DiscoveryUrl = url;
                existing.Domain = domain;
                existing.Username = username;
                existing.Password = password; // don't worry the password is encrypted :)
                existing.UniqueOrgName = org;
                existing.PublicUrl = publicUrl;

                existing.Save(linkerDataPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Unable to save connection details. Error: {0}", ex.Message));
            }
        }

        public void Trace(string format, params object[] args)
        {
            if (this._vsAddin != null)
            {
                this._vsAddin.WriteToOutputWindow(format, args);
            }
        }
    }
}

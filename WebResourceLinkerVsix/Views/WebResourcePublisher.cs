using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WebResourceLinkerVsix
{
    public partial class WebResourcePublisher : Form
    {
        #region multi-threading stuff
        private void UpdateStatus(string msg)
        {
            if (this.status.InvokeRequired)
            {
                this.status.BeginInvoke(new MethodInvoker(delegate()
                {
                    this.statusmsg.Text = msg;
                }));
            }
            else { this.statusmsg.Text = msg; }
        }

        private void ToggleControl(Control c, bool enabled)
        {
            if (c.InvokeRequired)
            {
                c.BeginInvoke(new MethodInvoker(delegate()
                {
                    c.Enabled = enabled;
                }));
            }
            else { c.Enabled = enabled; }
        }

        private void AddTreeNode(string key, string text, int imageIndex)
        {
            if (this.webresources.InvokeRequired)
            {
                this.webresources.BeginInvoke(new MethodInvoker(delegate()
                    {
                        this.webresources.Nodes.Add(key, text, imageIndex, imageIndex);
                    }));
            }
            else { this.webresources.Nodes.Add(key, text, imageIndex, imageIndex); }
        }

        private void AddTreeNode(string key, TreeNode node)
        {
            if (this.webresources.InvokeRequired)
            {
                this.webresources.BeginInvoke(new MethodInvoker(delegate()
                {
                    this.webresources.Nodes[key].Nodes.Add(node);
                }));
            }
            else { this.webresources.Nodes[key].Nodes.Add(node); }
        }
        #endregion

        public Controller Controller { get; set; }

        public bool ShowConnectionWindow { get; set; }
        public string LinkerDataPath { get; set; }
        public List<SelectedFile> SelectedFiles { get; set; }
        public SelectedFile UnmappedFile { get; set; }
        public List<SelectedFile> PublishedFiles { get; set; }

        public bool Relink { get; set; } // re-linking a file requires few extra operations so this flag needs to be set

        public WebResourcePublisher()
        {
            InitializeComponent();
        }

        public IOrganizationService Sdk { get; set; }
        public string PublicUrl { get; set; } // this is so when 'create new web resource' is clicked we can pop crm2011 web resource window

        private string[] _typeMapping = new string[] { ".htm, .html", ".css", ".js", ".xml", ".png", ".jpg", ".gif", ".xap", ".xsl, .xslt", ".ico" };
        private int[] _typeImageMapping = new[] { 0, 1, 2, 3, 4, 4, 4, 5, 3, 4 }; // maps the above idexes with the _treeImages list index below

        private ImageList _treeImages = new ImageList();

        private bool _publishing = false; // if we're publishing don't enable the controls

        private void WebResourcePublisher_Load(object sender, EventArgs e)
        {
            
        }

        private void linkorpublish_Click(object sender, EventArgs e)
        {
            TryPublishing();
        }


        private void connect_Click(object sender, EventArgs e)
        {
            ShowConnectionDialog();
        }

        private void createnew_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.PublicUrl))
            {
                MessageBox.Show("Please click on 'Connect to different org.' and try again");
                return;
            }

            // note: some urls won't work, you need to have the correct web urls set inside crm deployment manager, eg: http://server is different to http://server.fqdn.com
            System.Diagnostics.Process.Start(
                string.Format("{0}{1}main.aspx?etc=9333&pagetype=webresourceedit", 
                this.PublicUrl, !this.PublicUrl.EndsWith("/") ? "/" : ""));
        }

        private void refresh_Click(object sender, EventArgs e)
        {
            if (this.Sdk != null) { ShowExistingWebResources(); }
            else { ShowConnectionDialog(); }
        }
        
        public void Initialize()
        {
            this.UnmappedFile = null;
            this.PublishedFiles = new List<SelectedFile>();

            LoadImages();

            if (this.ShowConnectionWindow)
            {
                this.Show();

                ShowConnectionDialog();
            }
            else { ShowExistingWebResources(); }

            // when this method is called while relink is set it wont attempt to publish anything else other than the 1st file inside selectedfiles array
            if (this.Relink) { this.MarkAsUnmapped(this.SelectedFiles[0]); }
        }

        private void ShowConnectionDialog()
        {
            CrmConnection cc = new CrmConnection();
            cc.LinkerDataPath = this.LinkerDataPath;

            if (cc.ShowDialog() == DialogResult.OK)
            {
                this.Sdk = cc.Sdk;
                this.PublicUrl = cc.PublicUrl;

                ShowExistingWebResources();
            }
            else
            {
                ToggleControl(this.connect, true);
            }
        }

        private void LoadImages()
        {
            try
            {
                _treeImages.Images.Add((Image)new Bitmap(GetType(), "Resources.Icons.16x16_html.ico"));
                _treeImages.Images.Add((Image)new Bitmap(GetType(), "Resources.Icons.16x16_css.ico"));
                _treeImages.Images.Add((Image)new Bitmap(GetType(), "Resources.Icons.16x16_js.ico"));
                _treeImages.Images.Add((Image)new Bitmap(GetType(), "Resources.Icons.16x16_xml.ico"));
                _treeImages.Images.Add((Image)new Bitmap(GetType(), "Resources.Icons.16x16_images.ico"));
                _treeImages.Images.Add((Image)new Bitmap(GetType(), "Resources.Icons.16x16_silverlight.ico"));

                this.webresources.ImageList = _treeImages;
            }
            catch (Exception ex)
            {
                this.Controller.Trace("Failed to load images: {0}", ex.ToString());
            }
        }

        public void TryPublishing()
        {
            ToggleControls(false);

            if (this.Sdk != null)
            {
                LinkerData existing = LinkerData.Get(this.LinkerDataPath);

                if (this.UnmappedFile != null) // this property gets set if relink is set to true, otherwise this gets set by the logic below if there is no mapping
                {
                    PublishUnmappedResource(existing);
                }

                if (!this.Relink)
                {
                    var alreadyMappednMatching = this.SelectedFiles.Where(selectedFile =>
                        {
                            // grab correctly mapped files and publish them
                            return existing.Mappings
                                .Any(a => a.SourceFilePath.Equals(selectedFile.FriendlyFilePath, StringComparison.InvariantCultureIgnoreCase))

                                &&
                                // make sure we don't attempt to publish already published items
                                !this.PublishedFiles.Any(a => a.FilePath.Equals(selectedFile.FilePath, StringComparison.InvariantCultureIgnoreCase));

                        }).ToList();

                    if (alreadyMappednMatching.Count > 0) { PublishMappedResources(existing, alreadyMappednMatching); }

                    // now find the unmapped files and mark them as unmapped, when the 'publish/link' button is clicked this unmapped file will be picked up by the same method, see 'if this.unmappedfile != null' check above
                    this.SelectedFiles.ForEach(selectedFile =>
                        {
                            LinkerDataItem matchingItem = existing.Mappings
                                .Where(a => a.SourceFilePath.Equals(selectedFile.FriendlyFilePath, StringComparison.InvariantCultureIgnoreCase))
                                .Take(1).SingleOrDefault();

                            if (matchingItem == null)
                            {
                                MarkAsUnmapped(selectedFile);
                            }
                        });
                }

                existing.Save(this.LinkerDataPath); // save the mappings file regardless of the state of the publish since content would've been updated already
            }
            else
            {
                ToggleControl(this.connect, true); // enable the connect button so the user can try connecting to a different org
            }
        }

        private void MarkAsUnmapped(SelectedFile selectedFile)
        {
            this.UnmappedFile = selectedFile;

            this.currentmapping.Text = string.Format("Please map and publish: {0}", selectedFile.FriendlyFilePath);
            ToggleControls(true);

            this.Show(); // need to show the ui so the user can pick the web resource to map with
        }

        private void PublishMappedResources(LinkerData existing, List<SelectedFile> alreadyMappednMatching)
        {
            Guid[] webresourceIds = new Guid[alreadyMappednMatching.Count];
            string[] filePaths = new string[alreadyMappednMatching.Count];

            for (int i = 0; i < alreadyMappednMatching.Count; i++)
            {
                // bit of logic to figure out the webresourceid of the file basd on the filepath
                webresourceIds[i] = existing.Mappings
                    .Where(a => a.SourceFilePath.Equals(alreadyMappednMatching[i].FriendlyFilePath, StringComparison.InvariantCultureIgnoreCase))
                    .Select(a => a.WebResourceId).Take(1).SingleOrDefault();

                filePaths[i] = alreadyMappednMatching[i].FilePath;
            }

            Publish(webresourceIds, filePaths);

            this.PublishedFiles.AddRange(alreadyMappednMatching);
        }

        private void PublishUnmappedResource(LinkerData existing)
        {
            if (this.webresources.SelectedNode != null && this.webresources.SelectedNode.Tag != null)
            {
                // tag is only set internally so we should be ok without an error check here :P
                Guid webresourceId = new Guid(this.webresources.SelectedNode.Tag.ToString());

                // get rid of anything that's mapped with this sourcefile/webresourceid and add a clean one to avoid corruption on the mapping file
                existing.Mappings.RemoveAll(a => 
                    {
                        return a.WebResourceId == webresourceId
                            // check both the id and sourcepath because we now have to support re-linking
                            || a.SourceFilePath.Equals(this.UnmappedFile.FriendlyFilePath, StringComparison.InvariantCultureIgnoreCase);
                    });

                // add a clean mapping for this webresource and file
                existing.Mappings.Add(new LinkerDataItem
                    {
                        WebResourceId = webresourceId,
                        SourceFilePath = this.UnmappedFile.FriendlyFilePath
                    });

                Publish(new Guid[] { webresourceId }, new string[] { this.UnmappedFile.FilePath });

                this.PublishedFiles.Add(this.UnmappedFile);
                this.UnmappedFile = null;

                this.currentmapping.Text = "";
            }
        }

        private delegate Guid[] UpdateContentHander(Guid[] webresourceIds, string[] base64Contents);
        private delegate Guid[] PublishHandler(Guid[] webresourceIds);

        private void Publish(Guid[] webresourceIds, string[] filePaths)
        {
            _publishing = true;
            ToggleControls(false);

            this.Controller.Trace("Attempting to publish: {0}", string.Join("; ", filePaths.Select(a => Path.GetFileName(a))));

            UpdateContentHander uch = new UpdateContentHander(BeginUpdateContent);
            AsyncCallback callback = new AsyncCallback(EndUpdateContent);

            string[] base64Content = new string[webresourceIds.Length];
            for (int i = 0; i < webresourceIds.Length; i++)
            {
                base64Content[i] = Convert.ToBase64String(File.ReadAllBytes(filePaths[i]));
            }

            uch.BeginInvoke(webresourceIds, base64Content, callback, uch);
        }

        private Guid[] BeginUpdateContent(Guid[] webresourceIds, string[] base64Contents)
        {
            UpdateStatus("Updating content...");

            for (int i = 0; i < webresourceIds.Length; i++)
            {
                Entity resource = new Entity("webresource");
                resource["webresourceid"] = webresourceIds[i];
                resource["content"] = base64Contents[i];

                this.Sdk.Update(resource);
                this.Controller.Trace("Updated: {0}", webresourceIds[i]);
            }

            return webresourceIds;
        }

        private void EndUpdateContent(IAsyncResult result)
        {
            UpdateContentHander uch = result.AsyncState as UpdateContentHander;
            Guid[] webresourceIds = uch.EndInvoke(result);

            // once the content has been updated on all selected web resources then we can publish in bulk rather than 1 by 1
            PublishHandler ph = new PublishHandler(BeginPublish);
            AsyncCallback callback = new AsyncCallback(EndPublish);

            ph.BeginInvoke(webresourceIds, callback, ph);
        }

        private Guid[] BeginPublish(Guid[] webresourceIds)
        {
            UpdateStatus("Publishing...");

            OrganizationRequest request = new OrganizationRequest { RequestName = "PublishXml" };
            request.Parameters = new ParameterCollection();
            request.Parameters.Add(
                new KeyValuePair<string, object>("ParameterXml",
                    string.Format("<importexportxml><webresources>{0}</webresources></importexportxml>",
                        string.Join("", webresourceIds.Select(a => string.Format("<webresource>{0}</webresource>", a)))
                    )));

            this.Sdk.Execute(request);

            return webresourceIds;
        }

        private void EndPublish(IAsyncResult result)
        {
            PublishHandler uch = result.AsyncState as PublishHandler;
            var webresourceIds = uch.EndInvoke(result);

            decimal percentComplete = ((decimal)this.PublishedFiles.Count / (decimal)this.SelectedFiles.Count) * 100m;
            string msg = string.Format("Published: {0} of {1} ({2:N0}%)", this.PublishedFiles.Count, this.SelectedFiles.Count, percentComplete);

            this.Controller.Trace("Published: {0}", string.Join("; ", webresourceIds));
            UpdateStatus(msg);
            this.Controller.Trace(msg);

            _publishing = false;
            ToggleControls(true);

            // once all the selected files are published we can close the dialog & show success message
            if (this.SelectedFiles.Count == this.PublishedFiles.Count)
            {
                string confirmationMessage = "Successfully published!";
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new MethodInvoker(delegate()
                        {
                            this.Controller.Trace(confirmationMessage);
                            this.Controller.Cleanup();

                            this.Close();
                        }));
                }
                else 
                {
                    this.Controller.Trace(confirmationMessage);
                    this.Controller.Cleanup();

                    this.Close(); 
                }
            }
        }

        private delegate List<Entity> RetrieveWebResourceHandler();

        // todo: join with the solution and group by solution name in the tree
        private void ShowExistingWebResources()
        {
            ToggleControls(false);

            UpdateStatus("Loading web resources...");
            this.webresources.Nodes.Clear();

            RetrieveWebResourceHandler rwrh = new RetrieveWebResourceHandler(BeginShowExistingWebResources);
            AsyncCallback callback = new AsyncCallback(EndShowExistingWebResources);

            rwrh.BeginInvoke(callback, rwrh);
        }

        private List<Entity> BeginShowExistingWebResources()
        {
            QueryExpression qe = new QueryExpression("webresource");
            qe.ColumnSet = new ColumnSet("webresourceid", "webresourcetype", "name", "displayname");
            qe.Criteria.AddCondition("ismanaged", ConditionOperator.Equal, false);
            qe.Criteria.AddCondition("iscustomizable", ConditionOperator.Equal, true);
            qe.AddOrder("name", OrderType.Ascending);

            return this.Sdk.RetrieveMultiple(qe).Entities.ToList();
        }

        private void EndShowExistingWebResources(IAsyncResult result)
        {
            RetrieveWebResourceHandler rwrh = result.AsyncState as RetrieveWebResourceHandler;
            var results = rwrh.EndInvoke(result);

            this.Controller.Trace("Found {0} web resource(s)", results.Count);

            results
                .GroupBy(a => a.GetAttributeValue<OptionSetValue>("webresourcetype").Value)
                .OrderBy(a => a.Key).ToList()
                .ForEach(a =>
                {
                    string key = a.Key.ToString();
                    int imageIndex = _typeImageMapping[a.Key - 1];

                    AddTreeNode(key, _typeMapping[a.Key - 1], imageIndex);

                    a.ToList().ForEach(r =>
                    {
                        TreeNode tn = new TreeNode(r.GetAttributeValue<string>("name"), imageIndex, imageIndex);
                        tn.Tag = r.GetAttributeValue<Guid>("webresourceid");

                        AddTreeNode(key, tn);
                    });
                });

            UpdateStatus(string.Format("{0} web resources loaded", results.Count));

            ToggleControls(true);
        }

        private void ToggleControls(bool enable)
        {
            enable = enable && !_publishing; // only enable the buttons if we're not in a publishing state

            ToggleControl(this.linkorpublish, enable);
            ToggleControl(this.connect, enable);
            ToggleControl(this.createnew, enable);
            ToggleControl(this.refresh, enable);
        }

        private void WebResourcePublisher_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Controller.Cleanup();
        }
    }
}

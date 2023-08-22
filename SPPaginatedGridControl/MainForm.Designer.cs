namespace SPPaginatedGridControl
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            rcMainRibbon = new DevExpress.XtraBars.Ribbon.RibbonControl();
            bbiUpload = new DevExpress.XtraBars.BarButtonItem();
            bbiTestEndpoint = new DevExpress.XtraBars.BarButtonItem();
            bbiEncryptPassword = new DevExpress.XtraBars.BarButtonItem();
            rpMain = new DevExpress.XtraBars.Ribbon.RibbonPage();
            rpgDynamicConstruction = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            rpAdmin = new DevExpress.XtraBars.Ribbon.RibbonPage();
            rpgUpdateGroup = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            pContent = new Panel();
            rpgDevOptions = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            ((System.ComponentModel.ISupportInitialize)rcMainRibbon).BeginInit();
            SuspendLayout();
            // 
            // rcMainRibbon
            // 
            rcMainRibbon.ExpandCollapseItem.Id = 0;
            rcMainRibbon.Items.AddRange(new DevExpress.XtraBars.BarItem[] { rcMainRibbon.ExpandCollapseItem, rcMainRibbon.SearchEditItem, bbiUpload, bbiTestEndpoint, bbiEncryptPassword });
            rcMainRibbon.Location = new Point(0, 0);
            rcMainRibbon.MaxItemId = 6;
            rcMainRibbon.Name = "rcMainRibbon";
            rcMainRibbon.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] { rpMain, rpAdmin });
            rcMainRibbon.ShowApplicationButton = DevExpress.Utils.DefaultBoolean.False;
            rcMainRibbon.ShowExpandCollapseButton = DevExpress.Utils.DefaultBoolean.False;
            rcMainRibbon.ShowMoreCommandsButton = DevExpress.Utils.DefaultBoolean.False;
            rcMainRibbon.ShowQatLocationSelector = false;
            rcMainRibbon.ShowToolbarCustomizeItem = false;
            rcMainRibbon.Size = new Size(1091, 158);
            rcMainRibbon.Toolbar.ShowCustomizeItem = false;
            // 
            // bbiUpload
            // 
            bbiUpload.Caption = "Upload";
            bbiUpload.Enabled = false;
            bbiUpload.Id = 1;
            bbiUpload.ImageOptions.Image = (Image)resources.GetObject("bbiUpload.ImageOptions.Image");
            bbiUpload.Name = "bbiUpload";
            bbiUpload.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            // 
            // bbiTestEndpoint
            // 
            bbiTestEndpoint.Caption = "Test Endpoint";
            bbiTestEndpoint.Id = 2;
            bbiTestEndpoint.ImageOptions.Image = (Image)resources.GetObject("bbiTestEndpoint.ImageOptions.Image");
            bbiTestEndpoint.Name = "bbiTestEndpoint";
            bbiTestEndpoint.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            bbiTestEndpoint.ItemClick += OnItemClick_bbiTestEndpoint;
            // 
            // bbiEncryptPassword
            // 
            bbiEncryptPassword.Caption = "Encrypt Password";
            bbiEncryptPassword.Id = 4;
            bbiEncryptPassword.ImageOptions.Image = (Image)resources.GetObject("bbiEncryptPassword.ImageOptions.Image");
            bbiEncryptPassword.Name = "bbiEncryptPassword";
            bbiEncryptPassword.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            bbiEncryptPassword.ItemClick += OnItemClick_bbiEncryptPassword;
            // 
            // rpMain
            // 
            rpMain.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] { rpgDynamicConstruction });
            rpMain.Name = "rpMain";
            rpMain.Text = "Main Ribbon Page";
            // 
            // rpgDynamicConstruction
            // 
            rpgDynamicConstruction.ItemLinks.Add(bbiTestEndpoint);
            rpgDynamicConstruction.Name = "rpgDynamicConstruction";
            // 
            // rpAdmin
            // 
            rpAdmin.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] { rpgUpdateGroup, rpgDevOptions });
            rpAdmin.Name = "rpAdmin";
            rpAdmin.Text = "Admin";
            // 
            // rpgUpdateGroup
            // 
            rpgUpdateGroup.ItemLinks.Add(bbiUpload);
            rpgUpdateGroup.Name = "rpgUpdateGroup";
            // 
            // pContent
            // 
            pContent.Dock = DockStyle.Fill;
            pContent.Location = new Point(0, 158);
            pContent.Name = "pContent";
            pContent.Size = new Size(1091, 406);
            pContent.TabIndex = 1;
            // 
            // rpgDevOptions
            // 
            rpgDevOptions.ItemLinks.Add(bbiEncryptPassword);
            rpgDevOptions.Name = "rpgDevOptions";
            // 
            // MainForm
            // 
            AllowFormGlass = DevExpress.Utils.DefaultBoolean.True;
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1091, 564);
            Controls.Add(pContent);
            Controls.Add(rcMainRibbon);
            IconOptions.Icon = (Icon)resources.GetObject("MainForm.IconOptions.Icon");
            Name = "MainForm";
            Ribbon = rcMainRibbon;
            Text = "MainForm";
            ((System.ComponentModel.ISupportInitialize)rcMainRibbon).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DevExpress.XtraBars.Ribbon.RibbonControl rcMainRibbon;
        private DevExpress.XtraBars.Ribbon.RibbonPage rpMain;
        private DevExpress.XtraBars.BarButtonItem bbiUpload;
        private DevExpress.XtraBars.Ribbon.RibbonPage rpAdmin;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rpgUpdateGroup;
        private Panel pContent;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rpgDynamicConstruction;
        private DevExpress.XtraBars.BarButtonItem bbiTestEndpoint;
        private DevExpress.XtraBars.BarButtonItem bbiEncryptPassword;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rpgDevOptions;
    }
}
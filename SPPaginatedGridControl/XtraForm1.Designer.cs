namespace SPPaginatedGridControl
{
    partial class XtraForm1
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
            customGridControl1 = new CustomGridControl();
            gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            colabtID = new DevExpress.XtraGrid.Columns.GridColumn();
            colabtName = new DevExpress.XtraGrid.Columns.GridColumn();
            colabtNr = new DevExpress.XtraGrid.Columns.GridColumn();
            colagid = new DevExpress.XtraGrid.Columns.GridColumn();
            colagiD_H = new DevExpress.XtraGrid.Columns.GridColumn();
            colagName = new DevExpress.XtraGrid.Columns.GridColumn();
            colagPriv = new DevExpress.XtraGrid.Columns.GridColumn();
            colagStatus = new DevExpress.XtraGrid.Columns.GridColumn();
            colagTyp = new DevExpress.XtraGrid.Columns.GridColumn();
            colanstID = new DevExpress.XtraGrid.Columns.GridColumn();
            colanstStatus = new DevExpress.XtraGrid.Columns.GridColumn();
            colaufGrad = new DevExpress.XtraGrid.Columns.GridColumn();
            colaustritt = new DevExpress.XtraGrid.Columns.GridColumn();
            colaustrittGrund = new DevExpress.XtraGrid.Columns.GridColumn();
            coleintritt = new DevExpress.XtraGrid.Columns.GridColumn();
            colgeschlecht = new DevExpress.XtraGrid.Columns.GridColumn();
            colivGrad = new DevExpress.XtraGrid.Columns.GridColumn();
            collaufJahr = new DevExpress.XtraGrid.Columns.GridColumn();
            colname = new DevExpress.XtraGrid.Columns.GridColumn();
            colpersID = new DevExpress.XtraGrid.Columns.GridColumn();
            colpersNr = new DevExpress.XtraGrid.Columns.GridColumn();
            colreg = new DevExpress.XtraGrid.Columns.GridColumn();
            colsimulation = new DevExpress.XtraGrid.Columns.GridColumn();
            colstid = new DevExpress.XtraGrid.Columns.GridColumn();
            colstName = new DevExpress.XtraGrid.Columns.GridColumn();
            coltodesDatum = new DevExpress.XtraGrid.Columns.GridColumn();
            coltreeInfo = new DevExpress.XtraGrid.Columns.GridColumn();
            coltreeInfoAG = new DevExpress.XtraGrid.Columns.GridColumn();
            colvorname = new DevExpress.XtraGrid.Columns.GridColumn();
            colColumn = new DevExpress.XtraGrid.Columns.GridColumn();
            ((System.ComponentModel.ISupportInitialize)customGridControl1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridView1).BeginInit();
            SuspendLayout();
            // 
            // customGridControl1
            // 
            customGridControl1.DataSourceUrl = "https://localhost:7269/Paginated/DemoSelect";
            customGridControl1.Dock = DockStyle.Fill;
            customGridControl1.Location = new Point(0, 0);
            customGridControl1.MainView = gridView1;
            customGridControl1.Name = "customGridControl1";
            customGridControl1.Size = new Size(1102, 619);
            customGridControl1.TabIndex = 0;
            customGridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridView1 });
            // 
            // gridView1
            // 
            gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] { colabtID, colabtName, colabtNr, colagid, colagiD_H, colagName, colagPriv, colagStatus, colagTyp, colanstID, colanstStatus, colaufGrad, colaustritt, colaustrittGrund, coleintritt, colgeschlecht, colivGrad, collaufJahr, colname, colpersID, colpersNr, colreg, colsimulation, colstid, colstName, coltodesDatum, coltreeInfo, coltreeInfoAG, colvorname });
            gridView1.GridControl = customGridControl1;
            gridView1.Name = "gridView1";
            // 
            // colabtID
            // 
            colabtID.Caption = "abtID";
            colabtID.FieldName = "abtID";
            colabtID.Name = "colabtID";
            colabtID.Visible = true;
            colabtID.VisibleIndex = 0;
            colabtID.Width = 50;
            // 
            // colabtName
            // 
            colabtName.Caption = "abtName";
            colabtName.FieldName = "abtName";
            colabtName.Name = "colabtName";
            colabtName.Visible = true;
            colabtName.VisibleIndex = 1;
            colabtName.Width = 66;
            // 
            // colabtNr
            // 
            colabtNr.Caption = "abtNr";
            colabtNr.FieldName = "abtNr";
            colabtNr.Name = "colabtNr";
            colabtNr.Visible = true;
            colabtNr.VisibleIndex = 2;
            colabtNr.Width = 50;
            // 
            // colagid
            // 
            colagid.Caption = "agid";
            colagid.FieldName = "agid";
            colagid.Name = "colagid";
            colagid.Visible = true;
            colagid.VisibleIndex = 3;
            colagid.Width = 43;
            // 
            // colagiD_H
            // 
            colagiD_H.Caption = "agiD_H";
            colagiD_H.FieldName = "agiD_H";
            colagiD_H.Name = "colagiD_H";
            colagiD_H.Visible = true;
            colagiD_H.VisibleIndex = 4;
            colagiD_H.Width = 57;
            // 
            // colagName
            // 
            colagName.Caption = "agName";
            colagName.FieldName = "agName";
            colagName.Name = "colagName";
            colagName.Visible = true;
            colagName.VisibleIndex = 5;
            colagName.Width = 62;
            // 
            // colagPriv
            // 
            colagPriv.Caption = "agPriv";
            colagPriv.FieldName = "agPriv";
            colagPriv.Name = "colagPriv";
            colagPriv.Visible = true;
            colagPriv.VisibleIndex = 6;
            colagPriv.Width = 53;
            // 
            // colagStatus
            // 
            colagStatus.Caption = "agStatus";
            colagStatus.FieldName = "agStatus";
            colagStatus.Name = "colagStatus";
            colagStatus.Visible = true;
            colagStatus.VisibleIndex = 7;
            colagStatus.Width = 66;
            // 
            // colagTyp
            // 
            colagTyp.Caption = "agTyp";
            colagTyp.FieldName = "agTyp";
            colagTyp.Name = "colagTyp";
            colagTyp.Visible = true;
            colagTyp.VisibleIndex = 8;
            colagTyp.Width = 53;
            // 
            // colanstID
            // 
            colanstID.Caption = "anstID";
            colanstID.FieldName = "anstID";
            colanstID.Name = "colanstID";
            colanstID.Visible = true;
            colanstID.VisibleIndex = 9;
            colanstID.Width = 55;
            // 
            // colanstStatus
            // 
            colanstStatus.Caption = "anstStatus";
            colanstStatus.FieldName = "anstStatus";
            colanstStatus.Name = "colanstStatus";
            colanstStatus.Visible = true;
            colanstStatus.VisibleIndex = 10;
            // 
            // colaufGrad
            // 
            colaufGrad.Caption = "aufGrad";
            colaufGrad.FieldName = "aufGrad";
            colaufGrad.Name = "colaufGrad";
            colaufGrad.Visible = true;
            colaufGrad.VisibleIndex = 11;
            colaufGrad.Width = 62;
            // 
            // colaustritt
            // 
            colaustritt.Caption = "austritt";
            colaustritt.FieldName = "austritt";
            colaustritt.Name = "colaustritt";
            colaustritt.Visible = true;
            colaustritt.VisibleIndex = 12;
            colaustritt.Width = 58;
            // 
            // colaustrittGrund
            // 
            colaustrittGrund.Caption = "austrittGrund";
            colaustrittGrund.FieldName = "austrittGrund";
            colaustrittGrund.Name = "colaustrittGrund";
            colaustrittGrund.Visible = true;
            colaustrittGrund.VisibleIndex = 13;
            colaustrittGrund.Width = 87;
            // 
            // coleintritt
            // 
            coleintritt.Caption = "eintritt";
            coleintritt.FieldName = "eintritt";
            coleintritt.Name = "coleintritt";
            coleintritt.Visible = true;
            coleintritt.VisibleIndex = 14;
            coleintritt.Width = 55;
            // 
            // colgeschlecht
            // 
            colgeschlecht.Caption = "geschlecht";
            colgeschlecht.FieldName = "geschlecht";
            colgeschlecht.Name = "colgeschlecht";
            colgeschlecht.Visible = true;
            colgeschlecht.VisibleIndex = 15;
            colgeschlecht.Width = 74;
            // 
            // colivGrad
            // 
            colivGrad.Caption = "ivGrad";
            colivGrad.FieldName = "ivGrad";
            colivGrad.Name = "colivGrad";
            colivGrad.Visible = true;
            colivGrad.VisibleIndex = 16;
            colivGrad.Width = 54;
            // 
            // collaufJahr
            // 
            collaufJahr.Caption = "laufJahr";
            collaufJahr.FieldName = "laufJahr";
            collaufJahr.Name = "collaufJahr";
            collaufJahr.Visible = true;
            collaufJahr.VisibleIndex = 17;
            collaufJahr.Width = 62;
            // 
            // colname
            // 
            colname.Caption = "name";
            colname.FieldName = "name";
            colname.Name = "colname";
            colname.Visible = true;
            colname.VisibleIndex = 18;
            colname.Width = 49;
            // 
            // colpersID
            // 
            colpersID.Caption = "persID";
            colpersID.FieldName = "persID";
            colpersID.Name = "colpersID";
            colpersID.Visible = true;
            colpersID.VisibleIndex = 19;
            colpersID.Width = 55;
            // 
            // colpersNr
            // 
            colpersNr.Caption = "persNr";
            colpersNr.FieldName = "persNr";
            colpersNr.Name = "colpersNr";
            colpersNr.Visible = true;
            colpersNr.VisibleIndex = 20;
            colpersNr.Width = 55;
            // 
            // colreg
            // 
            colreg.Caption = "reg";
            colreg.FieldName = "reg";
            colreg.Name = "colreg";
            colreg.Visible = true;
            colreg.VisibleIndex = 21;
            colreg.Width = 39;
            // 
            // colsimulation
            // 
            colsimulation.Caption = "simulation";
            colsimulation.FieldName = "simulation";
            colsimulation.Name = "colsimulation";
            colsimulation.Visible = true;
            colsimulation.VisibleIndex = 22;
            colsimulation.Width = 70;
            // 
            // colstid
            // 
            colstid.Caption = "stid";
            colstid.FieldName = "stid";
            colstid.Name = "colstid";
            colstid.Visible = true;
            colstid.VisibleIndex = 23;
            colstid.Width = 40;
            // 
            // colstName
            // 
            colstName.Caption = "stName";
            colstName.FieldName = "stName";
            colstName.Name = "colstName";
            colstName.Visible = true;
            colstName.VisibleIndex = 24;
            colstName.Width = 59;
            // 
            // coltodesDatum
            // 
            coltodesDatum.Caption = "todesDatum";
            coltodesDatum.FieldName = "todesDatum";
            coltodesDatum.Name = "coltodesDatum";
            coltodesDatum.Visible = true;
            coltodesDatum.VisibleIndex = 25;
            coltodesDatum.Width = 81;
            // 
            // coltreeInfo
            // 
            coltreeInfo.Caption = "treeInfo";
            coltreeInfo.FieldName = "treeInfo";
            coltreeInfo.Name = "coltreeInfo";
            coltreeInfo.Visible = true;
            coltreeInfo.VisibleIndex = 26;
            coltreeInfo.Width = 63;
            // 
            // coltreeInfoAG
            // 
            coltreeInfoAG.Caption = "treeInfoAG";
            coltreeInfoAG.FieldName = "treeInfoAG";
            coltreeInfoAG.Name = "coltreeInfoAG";
            coltreeInfoAG.Visible = true;
            coltreeInfoAG.VisibleIndex = 27;
            coltreeInfoAG.Width = 77;
            // 
            // colvorname
            // 
            colvorname.Caption = "vorname";
            colvorname.FieldName = "vorname";
            colvorname.Name = "colvorname";
            colvorname.Visible = true;
            colvorname.VisibleIndex = 28;
            colvorname.Width = 65;
            // 
            // colColumn
            // 
            colColumn.FieldName = "Column";
            colColumn.Name = "colColumn";
            // 
            // XtraForm1
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1102, 619);
            Controls.Add(customGridControl1);
            Name = "XtraForm1";
            Text = "XtraForm1";
            ((System.ComponentModel.ISupportInitialize)customGridControl1).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridView1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private CustomGridControl customGridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraGrid.Columns.GridColumn colColumn;
        private DevExpress.XtraGrid.Columns.GridColumn colabtID;
        private DevExpress.XtraGrid.Columns.GridColumn colabtName;
        private DevExpress.XtraGrid.Columns.GridColumn colabtNr;
        private DevExpress.XtraGrid.Columns.GridColumn colagid;
        private DevExpress.XtraGrid.Columns.GridColumn colagiD_H;
        private DevExpress.XtraGrid.Columns.GridColumn colagName;
        private DevExpress.XtraGrid.Columns.GridColumn colagPriv;
        private DevExpress.XtraGrid.Columns.GridColumn colagStatus;
        private DevExpress.XtraGrid.Columns.GridColumn colagTyp;
        private DevExpress.XtraGrid.Columns.GridColumn colanstID;
        private DevExpress.XtraGrid.Columns.GridColumn colanstStatus;
        private DevExpress.XtraGrid.Columns.GridColumn colaufGrad;
        private DevExpress.XtraGrid.Columns.GridColumn colaustritt;
        private DevExpress.XtraGrid.Columns.GridColumn colaustrittGrund;
        private DevExpress.XtraGrid.Columns.GridColumn coleintritt;
        private DevExpress.XtraGrid.Columns.GridColumn colgeschlecht;
        private DevExpress.XtraGrid.Columns.GridColumn colivGrad;
        private DevExpress.XtraGrid.Columns.GridColumn collaufJahr;
        private DevExpress.XtraGrid.Columns.GridColumn colname;
        private DevExpress.XtraGrid.Columns.GridColumn colpersID;
        private DevExpress.XtraGrid.Columns.GridColumn colpersNr;
        private DevExpress.XtraGrid.Columns.GridColumn colreg;
        private DevExpress.XtraGrid.Columns.GridColumn colsimulation;
        private DevExpress.XtraGrid.Columns.GridColumn colstid;
        private DevExpress.XtraGrid.Columns.GridColumn colstName;
        private DevExpress.XtraGrid.Columns.GridColumn coltodesDatum;
        private DevExpress.XtraGrid.Columns.GridColumn coltreeInfo;
        private DevExpress.XtraGrid.Columns.GridColumn coltreeInfoAG;
        private DevExpress.XtraGrid.Columns.GridColumn colvorname;
    }
}
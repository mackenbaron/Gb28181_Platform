namespace Gb28181_Client
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("");
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("");
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnCatalog = new System.Windows.Forms.Button();
            this.btnReal = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnBye = new System.Windows.Forms.Button();
            this.lvDev = new System.Windows.Forms.ListView();
            this.Seq = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.devName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.devId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnRecord = new System.Windows.Forms.Button();
            this.btnStopRecord = new System.Windows.Forms.Button();
            this.txtStartTime = new System.Windows.Forms.TextBox();
            this.txtStopTime = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnRecordGet = new System.Windows.Forms.Button();
            this.lvRecord = new System.Windows.Forms.ListView();
            this.columnHeader0 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(12, 12);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "启动";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(149, 12);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "停止";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.ForeColor = System.Drawing.Color.Red;
            this.lblStatus.Location = new System.Drawing.Point(13, 50);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(77, 12);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "等待中。。。";
            // 
            // btnCatalog
            // 
            this.btnCatalog.Location = new System.Drawing.Point(501, 75);
            this.btnCatalog.Name = "btnCatalog";
            this.btnCatalog.Size = new System.Drawing.Size(75, 23);
            this.btnCatalog.TabIndex = 3;
            this.btnCatalog.Text = "目录查询";
            this.btnCatalog.UseVisualStyleBackColor = true;
            this.btnCatalog.Click += new System.EventHandler(this.btnCatalog_Click);
            // 
            // btnReal
            // 
            this.btnReal.Location = new System.Drawing.Point(501, 118);
            this.btnReal.Name = "btnReal";
            this.btnReal.Size = new System.Drawing.Size(75, 23);
            this.btnReal.TabIndex = 8;
            this.btnReal.Text = "直播视频";
            this.btnReal.UseVisualStyleBackColor = true;
            this.btnReal.Click += new System.EventHandler(this.btnReal_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(500, 204);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 9;
            this.button1.Text = "测试";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnBye
            // 
            this.btnBye.Location = new System.Drawing.Point(500, 161);
            this.btnBye.Name = "btnBye";
            this.btnBye.Size = new System.Drawing.Size(75, 23);
            this.btnBye.TabIndex = 10;
            this.btnBye.Text = "终止直播";
            this.btnBye.UseVisualStyleBackColor = true;
            this.btnBye.Click += new System.EventHandler(this.btnBye_Click);
            // 
            // lvDev
            // 
            this.lvDev.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Seq,
            this.devName,
            this.devId});
            this.lvDev.FullRowSelect = true;
            this.lvDev.Location = new System.Drawing.Point(12, 75);
            this.lvDev.MultiSelect = false;
            this.lvDev.Name = "lvDev";
            this.lvDev.Size = new System.Drawing.Size(483, 281);
            this.lvDev.TabIndex = 11;
            this.lvDev.UseCompatibleStateImageBehavior = false;
            this.lvDev.View = System.Windows.Forms.View.Details;
            // 
            // Seq
            // 
            this.Seq.Text = "序号";
            // 
            // devName
            // 
            this.devName.Text = "设备名称";
            this.devName.Width = 230;
            // 
            // devId
            // 
            this.devId.Text = "设备编码";
            this.devId.Width = 150;
            // 
            // btnRecord
            // 
            this.btnRecord.Location = new System.Drawing.Point(605, 161);
            this.btnRecord.Name = "btnRecord";
            this.btnRecord.Size = new System.Drawing.Size(75, 23);
            this.btnRecord.TabIndex = 12;
            this.btnRecord.Text = "录像点播";
            this.btnRecord.UseVisualStyleBackColor = true;
            this.btnRecord.Click += new System.EventHandler(this.btnRecord_Click);
            // 
            // btnStopRecord
            // 
            this.btnStopRecord.Location = new System.Drawing.Point(605, 204);
            this.btnStopRecord.Name = "btnStopRecord";
            this.btnStopRecord.Size = new System.Drawing.Size(75, 23);
            this.btnStopRecord.TabIndex = 13;
            this.btnStopRecord.Text = "终止点播";
            this.btnStopRecord.UseVisualStyleBackColor = true;
            this.btnStopRecord.Click += new System.EventHandler(this.btnStopRecord_Click);
            // 
            // txtStartTime
            // 
            this.txtStartTime.Location = new System.Drawing.Point(543, 12);
            this.txtStartTime.Name = "txtStartTime";
            this.txtStartTime.Size = new System.Drawing.Size(137, 21);
            this.txtStartTime.TabIndex = 14;
            this.txtStartTime.Text = "2016-12-14 9:00:00";
            // 
            // txtStopTime
            // 
            this.txtStopTime.Location = new System.Drawing.Point(543, 47);
            this.txtStopTime.Name = "txtStopTime";
            this.txtStopTime.Size = new System.Drawing.Size(137, 21);
            this.txtStopTime.TabIndex = 15;
            this.txtStopTime.Text = "2016-12-14 11:00:00";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(486, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 16;
            this.label2.Text = "开始时间";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(486, 51);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 17;
            this.label3.Text = "结束时间";
            // 
            // btnRecordGet
            // 
            this.btnRecordGet.Location = new System.Drawing.Point(605, 118);
            this.btnRecordGet.Name = "btnRecordGet";
            this.btnRecordGet.Size = new System.Drawing.Size(75, 23);
            this.btnRecordGet.TabIndex = 18;
            this.btnRecordGet.Text = "录像检索";
            this.btnRecordGet.UseVisualStyleBackColor = true;
            this.btnRecordGet.Click += new System.EventHandler(this.btnRecordGet_Click);
            // 
            // lvRecord
            // 
            this.lvRecord.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lvRecord.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader0,
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.lvRecord.FullRowSelect = true;
            this.lvRecord.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2});
            this.lvRecord.Location = new System.Drawing.Point(12, 362);
            this.lvRecord.MultiSelect = false;
            this.lvRecord.Name = "lvRecord";
            this.lvRecord.Size = new System.Drawing.Size(668, 125);
            this.lvRecord.TabIndex = 19;
            this.lvRecord.UseCompatibleStateImageBehavior = false;
            this.lvRecord.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader0
            // 
            this.columnHeader0.Text = "序号";
            // 
            // columnHeader1
            // 
            this.columnHeader1.Tag = "";
            this.columnHeader1.Text = "设备名称";
            this.columnHeader1.Width = 200;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "设备编码";
            this.columnHeader2.Width = 140;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "录像开始时间";
            this.columnHeader3.Width = 130;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "录像结束时间";
            this.columnHeader4.Width = 130;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(500, 255);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 20;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(690, 497);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.lvRecord);
            this.Controls.Add(this.btnRecordGet);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtStopTime);
            this.Controls.Add(this.txtStartTime);
            this.Controls.Add(this.btnStopRecord);
            this.Controls.Add(this.btnRecord);
            this.Controls.Add(this.lvDev);
            this.Controls.Add(this.btnBye);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnReal);
            this.Controls.Add(this.btnCatalog);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

     

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnCatalog;
        private System.Windows.Forms.Button btnReal;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnBye;
        private System.Windows.Forms.ListView lvDev;
        private System.Windows.Forms.ColumnHeader devName;
        private System.Windows.Forms.ColumnHeader devId;
        private System.Windows.Forms.Button btnRecord;
        private System.Windows.Forms.Button btnStopRecord;
        private System.Windows.Forms.TextBox txtStartTime;
        private System.Windows.Forms.TextBox txtStopTime;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnRecordGet;
        private System.Windows.Forms.ListView lvRecord;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader0;
        private System.Windows.Forms.ColumnHeader Seq;
        private System.Windows.Forms.Button button2;
    }
}


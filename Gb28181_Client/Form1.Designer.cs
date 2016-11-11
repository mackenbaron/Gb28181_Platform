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
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnCatalog = new System.Windows.Forms.Button();
            this.txtDeviceId = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnReal = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnBye = new System.Windows.Forms.Button();
            this.lvDev = new System.Windows.Forms.ListView();
            this.devName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.devId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnRecord = new System.Windows.Forms.Button();
            this.btnStopRecord = new System.Windows.Forms.Button();
            this.txtStartTime = new System.Windows.Forms.TextBox();
            this.txtStopTime = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(12, 12);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(149, 12);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "Stop";
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
            this.btnCatalog.Location = new System.Drawing.Point(485, 117);
            this.btnCatalog.Name = "btnCatalog";
            this.btnCatalog.Size = new System.Drawing.Size(75, 23);
            this.btnCatalog.TabIndex = 3;
            this.btnCatalog.Text = "Catalog";
            this.btnCatalog.UseVisualStyleBackColor = true;
            this.btnCatalog.Click += new System.EventHandler(this.btnCatalog_Click);
            // 
            // txtDeviceId
            // 
            this.txtDeviceId.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtDeviceId.Location = new System.Drawing.Point(91, 82);
            this.txtDeviceId.Name = "txtDeviceId";
            this.txtDeviceId.Size = new System.Drawing.Size(137, 21);
            this.txtDeviceId.TabIndex = 4;
            this.txtDeviceId.Text = "66240000012001000001";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 86);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 12);
            this.label1.TabIndex = 5;
            this.label1.Text = "远程SIP编码";
            // 
            // btnReal
            // 
            this.btnReal.Location = new System.Drawing.Point(485, 164);
            this.btnReal.Name = "btnReal";
            this.btnReal.Size = new System.Drawing.Size(75, 23);
            this.btnReal.TabIndex = 8;
            this.btnReal.Text = "Real";
            this.btnReal.UseVisualStyleBackColor = true;
            this.btnReal.Click += new System.EventHandler(this.btnReal_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(484, 257);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 9;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnBye
            // 
            this.btnBye.Location = new System.Drawing.Point(484, 209);
            this.btnBye.Name = "btnBye";
            this.btnBye.Size = new System.Drawing.Size(75, 23);
            this.btnBye.TabIndex = 10;
            this.btnBye.Text = "Bye";
            this.btnBye.UseVisualStyleBackColor = true;
            this.btnBye.Click += new System.EventHandler(this.btnBye_Click);
            // 
            // lvDev
            // 
            this.lvDev.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.devName,
            this.devId});
            this.lvDev.FullRowSelect = true;
            this.lvDev.Location = new System.Drawing.Point(12, 117);
            this.lvDev.MultiSelect = false;
            this.lvDev.Name = "lvDev";
            this.lvDev.Size = new System.Drawing.Size(458, 296);
            this.lvDev.TabIndex = 11;
            this.lvDev.UseCompatibleStateImageBehavior = false;
            this.lvDev.View = System.Windows.Forms.View.Details;
            // 
            // devName
            // 
            this.devName.Text = "设备名称";
            this.devName.Width = 260;
            // 
            // devId
            // 
            this.devId.Text = "设备编码";
            this.devId.Width = 150;
            // 
            // btnRecord
            // 
            this.btnRecord.Location = new System.Drawing.Point(234, 417);
            this.btnRecord.Name = "btnRecord";
            this.btnRecord.Size = new System.Drawing.Size(75, 23);
            this.btnRecord.TabIndex = 12;
            this.btnRecord.Text = "Record";
            this.btnRecord.UseVisualStyleBackColor = true;
            this.btnRecord.Click += new System.EventHandler(this.btnRecord_Click);
            // 
            // btnStopRecord
            // 
            this.btnStopRecord.Location = new System.Drawing.Point(234, 457);
            this.btnStopRecord.Name = "btnStopRecord";
            this.btnStopRecord.Size = new System.Drawing.Size(75, 23);
            this.btnStopRecord.TabIndex = 13;
            this.btnStopRecord.Text = "StopRecord";
            this.btnStopRecord.UseVisualStyleBackColor = true;
            // 
            // txtStartTime
            // 
            this.txtStartTime.Location = new System.Drawing.Point(91, 419);
            this.txtStartTime.Name = "txtStartTime";
            this.txtStartTime.Size = new System.Drawing.Size(137, 21);
            this.txtStartTime.TabIndex = 14;
            this.txtStartTime.Text = "2016-11-4 8:00:00";
            // 
            // txtStopTime
            // 
            this.txtStopTime.Location = new System.Drawing.Point(91, 457);
            this.txtStopTime.Name = "txtStopTime";
            this.txtStopTime.Size = new System.Drawing.Size(137, 21);
            this.txtStopTime.TabIndex = 15;
            this.txtStopTime.Text = "2016-11-4 10:00:00";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 424);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 16;
            this.label2.Text = "录像开始时间";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 461);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 17;
            this.label3.Text = "录像结束时间";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(485, 298);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 18;
            this.button2.Text = "Send";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(570, 488);
            this.Controls.Add(this.button2);
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
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtDeviceId);
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
        private System.Windows.Forms.TextBox txtDeviceId;
        private System.Windows.Forms.Label label1;
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
        private System.Windows.Forms.Button button2;
    }
}


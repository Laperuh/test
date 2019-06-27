namespace AprioriAllLib
{
    partial class Main_window
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
            this.button1 = new System.Windows.Forms.Button();
            this.textBox_data = new System.Windows.Forms.TextBox();
            this.data_FileDialog = new System.Windows.Forms.OpenFileDialog();
            this.label2 = new System.Windows.Forms.Label();
            this.support_setbox = new System.Windows.Forms.NumericUpDown();
            this.groupBox_apriori = new System.Windows.Forms.GroupBox();
            this.time1 = new System.Windows.Forms.Button();
            this.start_apriori_button = new System.Windows.Forms.Button();
            this.button_show_litemsetsApriori = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.support_setbox)).BeginInit();
            this.groupBox_apriori.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(502, 35);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.data_browse_button_Click);
            // 
            // textBox_data
            // 
            this.textBox_data.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_data.Location = new System.Drawing.Point(26, 35);
            this.textBox_data.Name = "textBox_data";
            this.textBox_data.ReadOnly = true;
            this.textBox_data.Size = new System.Drawing.Size(470, 20);
            this.textBox_data.TabIndex = 2;
            this.textBox_data.TextChanged += new System.EventHandler(this.textBox_data_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(38, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "поддержка (%)";
            // 
            // support_setbox
            // 
            this.support_setbox.Location = new System.Drawing.Point(124, 72);
            this.support_setbox.Name = "support_setbox";
            this.support_setbox.Size = new System.Drawing.Size(56, 20);
            this.support_setbox.TabIndex = 12;
            this.support_setbox.ValueChanged += new System.EventHandler(this.support_setbox_ValueChanged);
            // 
            // groupBox_apriori
            // 
            this.groupBox_apriori.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_apriori.Controls.Add(this.time1);
            this.groupBox_apriori.Controls.Add(this.start_apriori_button);
            this.groupBox_apriori.Controls.Add(this.button_show_litemsetsApriori);
            this.groupBox_apriori.Location = new System.Drawing.Point(120, 111);
            this.groupBox_apriori.Name = "groupBox_apriori";
            this.groupBox_apriori.Size = new System.Drawing.Size(354, 83);
            this.groupBox_apriori.TabIndex = 13;
            this.groupBox_apriori.TabStop = false;
            this.groupBox_apriori.Text = "Apriori";
            // 
            // time1
            // 
            this.time1.Enabled = false;
            this.time1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.time1.Location = new System.Drawing.Point(47, 19);
            this.time1.Name = "time1";
            this.time1.Size = new System.Drawing.Size(75, 33);
            this.time1.TabIndex = 16;
            this.time1.Text = "время";
            this.time1.UseVisualStyleBackColor = true;
            this.time1.Click += new System.EventHandler(this.time1_Click);
            // 
            // start_apriori_button
            // 
            this.start_apriori_button.Enabled = false;
            this.start_apriori_button.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.start_apriori_button.Location = new System.Drawing.Point(128, 19);
            this.start_apriori_button.Name = "start_apriori_button";
            this.start_apriori_button.Size = new System.Drawing.Size(75, 33);
            this.start_apriori_button.TabIndex = 12;
            this.start_apriori_button.Text = "проходка";
            this.start_apriori_button.UseVisualStyleBackColor = true;
            this.start_apriori_button.Click += new System.EventHandler(this.start_apriori_button_Click);
            // 
            // button_show_litemsetsApriori
            // 
            this.button_show_litemsetsApriori.Enabled = false;
            this.button_show_litemsetsApriori.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.button_show_litemsetsApriori.Location = new System.Drawing.Point(209, 19);
            this.button_show_litemsetsApriori.Name = "button_show_litemsetsApriori";
            this.button_show_litemsetsApriori.Size = new System.Drawing.Size(75, 32);
            this.button_show_litemsetsApriori.TabIndex = 4;
            this.button_show_litemsetsApriori.Text = "L-наборы";
            this.button_show_litemsetsApriori.UseVisualStyleBackColor = true;
            this.button_show_litemsetsApriori.Click += new System.EventHandler(this.button_show_litemsetsApriori_Click);
            // 
            // Main_window
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(674, 331);
            this.Controls.Add(this.groupBox_apriori);
            this.Controls.Add(this.support_setbox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_data);
            this.Controls.Add(this.button1);
            this.Name = "Main_window";
            this.Text = "Main_window";
            ((System.ComponentModel.ISupportInitialize)(this.support_setbox)).EndInit();
            this.groupBox_apriori.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.OpenFileDialog data_FileDialog;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox_data;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown support_setbox;
        private System.Windows.Forms.GroupBox groupBox_apriori;
        private System.Windows.Forms.Button time1;
        private System.Windows.Forms.Button start_apriori_button;
        private System.Windows.Forms.Button button_show_litemsetsApriori;
    }
}
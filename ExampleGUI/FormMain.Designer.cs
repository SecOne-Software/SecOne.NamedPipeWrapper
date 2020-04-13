namespace ExampleGUI
{
    partial class FormMain
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
            this.buttonClient = new System.Windows.Forms.Button();
            this.buttonServer = new System.Windows.Forms.Button();
            this.btnTest = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonClient
            // 
            this.buttonClient.Location = new System.Drawing.Point(26, 25);
            this.buttonClient.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.buttonClient.Name = "buttonClient";
            this.buttonClient.Size = new System.Drawing.Size(150, 44);
            this.buttonClient.TabIndex = 0;
            this.buttonClient.Text = "&Client";
            this.buttonClient.UseVisualStyleBackColor = true;
            this.buttonClient.Click += new System.EventHandler(this.buttonClient_Click);
            // 
            // buttonServer
            // 
            this.buttonServer.Location = new System.Drawing.Point(188, 25);
            this.buttonServer.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.buttonServer.Name = "buttonServer";
            this.buttonServer.Size = new System.Drawing.Size(150, 44);
            this.buttonServer.TabIndex = 1;
            this.buttonServer.Text = "&Server";
            this.buttonServer.UseVisualStyleBackColor = true;
            this.buttonServer.Click += new System.EventHandler(this.buttonServer_Click);
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(360, 25);
            this.btnTest.Margin = new System.Windows.Forms.Padding(6);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(150, 44);
            this.btnTest.TabIndex = 2;
            this.btnTest.Text = "&Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(681, 129);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.buttonServer);
            this.Controls.Add(this.buttonClient);
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Main";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonClient;
        private System.Windows.Forms.Button buttonServer;
        private System.Windows.Forms.Button btnTest;
    }
}


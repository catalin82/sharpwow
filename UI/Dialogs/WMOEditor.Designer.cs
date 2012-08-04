namespace SharpWoW.UI.Dialogs
{
    partial class WMOEditor
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
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.simpleRenderControl1 = new SharpWoW.Controls.SimpleRenderControl();
            this.SuspendLayout();
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Left;
            this.propertyGrid1.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(223, 454);
            this.propertyGrid1.TabIndex = 0;
            // 
            // simpleRenderControl1
            // 
            this.simpleRenderControl1.Dock = System.Windows.Forms.DockStyle.Right;
            this.simpleRenderControl1.Location = new System.Drawing.Point(227, 0);
            this.simpleRenderControl1.Name = "simpleRenderControl1";
            this.simpleRenderControl1.Size = new System.Drawing.Size(480, 454);
            this.simpleRenderControl1.TabIndex = 1;
            // 
            // WMOEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(707, 454);
            this.Controls.Add(this.simpleRenderControl1);
            this.Controls.Add(this.propertyGrid1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "WMOEditor";
            this.Text = "WMOEditor";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private Controls.SimpleRenderControl simpleRenderControl1;
    }
}
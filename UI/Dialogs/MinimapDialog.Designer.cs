namespace SharpWoW.UI.Dialogs
{
    partial class MinimapDialog
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
            this.minimapControl1 = new SharpWoW.Controls.MinimapControl();
            this.SuspendLayout();
            // 
            // minimapControl1
            // 
            this.minimapControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.minimapControl1.Location = new System.Drawing.Point(0, 0);
            this.minimapControl1.Minimap = null;
            this.minimapControl1.Name = "minimapControl1";
            this.minimapControl1.Size = new System.Drawing.Size(619, 619);
            this.minimapControl1.TabIndex = 0;
            // 
            // MinimapDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(619, 619);
            this.Controls.Add(this.minimapControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "MinimapDialog";
            this.Text = "MinimapDialog";
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.MinimapControl minimapControl1;
    }
}
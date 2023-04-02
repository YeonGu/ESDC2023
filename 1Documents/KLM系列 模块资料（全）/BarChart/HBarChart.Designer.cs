namespace BarChart
{
    partial class HBarChart
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // HBarChart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "HBarChart";
            this.MouseLeave += new System.EventHandler(this.OnMouseLeave);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            this.BackColorChanged += new System.EventHandler(this.HBarChart_BackColorChanged);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.OnDoubleClick);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnClick);
            this.Resize += new System.EventHandler(this.OnSize);
            this.ResumeLayout(false);

        }

        #endregion
    }
}

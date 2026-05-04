namespace QMS
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            PrintButton = new Button();
            SuspendLayout();
            // 
            // PrintButton
            // 
            PrintButton.BackColor = SystemColors.ButtonFace;
            PrintButton.Font = new Font("Segoe UI Semibold", 36F, FontStyle.Bold, GraphicsUnit.Point, 0);
            PrintButton.ForeColor = SystemColors.ActiveCaptionText;
            PrintButton.Location = new Point(213, 169);
            PrintButton.Name = "PrintButton";
            PrintButton.Size = new Size(375, 113);
            PrintButton.TabIndex = 0;
            PrintButton.Text = "Print";
            PrintButton.UseVisualStyleBackColor = false;
            PrintButton.Click += PrintButton_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(PrintButton);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private Button PrintButton;
    }
}

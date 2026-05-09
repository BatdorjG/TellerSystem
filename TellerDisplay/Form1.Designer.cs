namespace TellerDisplay
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
            labelCustomerNumber = new Label();
            SuspendLayout();
            // 
            // labelCustomerNumber
            // 
            labelCustomerNumber.AutoSize = true;
            labelCustomerNumber.BackColor = SystemColors.ActiveCaptionText;
            labelCustomerNumber.ForeColor = SystemColors.ButtonFace;
            labelCustomerNumber.Location = new Point(210, 83);
            labelCustomerNumber.Name = "labelCustomerNumber";
            labelCustomerNumber.Size = new Size(50, 20);
            labelCustomerNumber.TabIndex = 0;
            labelCustomerNumber.Text = "label1";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaptionText;
            ClientSize = new Size(468, 292);
            Controls.Add(labelCustomerNumber);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelCustomerNumber;
    }
}

namespace RegressionAnalysis
{
    partial class RegressionAnalysisForm
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
			button1 = new Button();
			SuspendLayout();
			// 
			// button1
			// 
			button1.Location = new Point(52, 209);
			button1.Name = "button1";
			button1.Size = new Size(94, 29);
			button1.TabIndex = 0;
			button1.Text = "button1";
			button1.UseVisualStyleBackColor = true;
			button1.Click += button1_Click;
			// 
			// RegressionAnalysisForm
			// 
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1170, 611);
			Controls.Add(button1);
			Font = new Font("Yu Gothic UI", 9F);
			Margin = new Padding(3, 4, 3, 4);
			Name = "RegressionAnalysisForm";
			Text = "RegressionAnalysisForm";
			ResumeLayout(false);
		}

		#endregion

		private Button button1;
	}
}

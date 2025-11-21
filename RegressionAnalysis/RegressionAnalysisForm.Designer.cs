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
			DgvAnalysisData = new DataGridView();
			label1 = new Label();
			CmbResponseVariable = new ComboBox();
			label2 = new Label();
			label3 = new Label();
			ClbPredictorVariable = new CheckedListBox();
			dataGridView1 = new DataGridView();
			groupBox1 = new GroupBox();
			label8 = new Label();
			label9 = new Label();
			BtnRunAnalysis = new Button();
			BtnLoadAnalysisData = new Button();
			LblRecordCount = new Label();
			((System.ComponentModel.ISupportInitialize)DgvAnalysisData).BeginInit();
			((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
			groupBox1.SuspendLayout();
			SuspendLayout();
			// 
			// DgvAnalysisData
			// 
			DgvAnalysisData.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			DgvAnalysisData.Location = new Point(42, 80);
			DgvAnalysisData.Name = "DgvAnalysisData";
			DgvAnalysisData.Size = new Size(414, 151);
			DgvAnalysisData.TabIndex = 0;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(42, 62);
			label1.Name = "label1";
			label1.Size = new Size(57, 15);
			label1.TabIndex = 1;
			label1.Text = "分析データ";
			// 
			// CmbResponseVariable
			// 
			CmbResponseVariable.FormattingEnabled = true;
			CmbResponseVariable.Location = new Point(20, 50);
			CmbResponseVariable.Name = "CmbResponseVariable";
			CmbResponseVariable.Size = new Size(121, 23);
			CmbResponseVariable.TabIndex = 2;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new Point(20, 29);
			label2.Name = "label2";
			label2.Size = new Size(55, 15);
			label2.TabIndex = 1;
			label2.Text = "目的変数";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Location = new Point(158, 29);
			label3.Name = "label3";
			label3.Size = new Size(55, 15);
			label3.TabIndex = 1;
			label3.Text = "説明変数";
			// 
			// ClbPredictorVariable
			// 
			ClbPredictorVariable.CheckOnClick = true;
			ClbPredictorVariable.FormattingEnabled = true;
			ClbPredictorVariable.Location = new Point(158, 49);
			ClbPredictorVariable.Name = "ClbPredictorVariable";
			ClbPredictorVariable.Size = new Size(230, 94);
			ClbPredictorVariable.TabIndex = 3;
			// 
			// dataGridView1
			// 
			dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridView1.Location = new Point(543, 80);
			dataGridView1.Name = "dataGridView1";
			dataGridView1.Size = new Size(243, 329);
			dataGridView1.TabIndex = 10;
			// 
			// groupBox1
			// 
			groupBox1.Controls.Add(label2);
			groupBox1.Controls.Add(label3);
			groupBox1.Controls.Add(CmbResponseVariable);
			groupBox1.Controls.Add(ClbPredictorVariable);
			groupBox1.Location = new Point(42, 247);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new Size(414, 162);
			groupBox1.TabIndex = 11;
			groupBox1.TabStop = false;
			groupBox1.Text = "分析条件";
			// 
			// label8
			// 
			label8.AutoSize = true;
			label8.Location = new Point(554, 55);
			label8.Name = "label8";
			label8.Size = new Size(55, 15);
			label8.TabIndex = 12;
			label8.Text = "分析結果";
			// 
			// label9
			// 
			label9.BackColor = Color.OldLace;
			label9.BorderStyle = BorderStyle.Fixed3D;
			label9.Font = new Font("Yu Gothic UI", 18F);
			label9.Location = new Point(12, 9);
			label9.Name = "label9";
			label9.Size = new Size(780, 35);
			label9.TabIndex = 13;
			label9.Text = "重回帰分析";
			// 
			// BtnRunAnalysis
			// 
			BtnRunAnalysis.Location = new Point(462, 80);
			BtnRunAnalysis.Name = "BtnRunAnalysis";
			BtnRunAnalysis.Size = new Size(75, 151);
			BtnRunAnalysis.TabIndex = 16;
			BtnRunAnalysis.Text = "分析実行";
			BtnRunAnalysis.UseVisualStyleBackColor = true;
			// 
			// BtnLoadAnalysisData
			// 
			BtnLoadAnalysisData.Location = new Point(338, 51);
			BtnLoadAnalysisData.Name = "BtnLoadAnalysisData";
			BtnLoadAnalysisData.Size = new Size(118, 23);
			BtnLoadAnalysisData.TabIndex = 17;
			BtnLoadAnalysisData.Text = "分析データ読込...";
			BtnLoadAnalysisData.UseVisualStyleBackColor = true;
			BtnLoadAnalysisData.Click += BtnLoadAnalysisData_Click;
			// 
			// LblRecordCount
			// 
			LblRecordCount.AutoSize = true;
			LblRecordCount.Location = new Point(105, 62);
			LblRecordCount.Name = "LblRecordCount";
			LblRecordCount.Size = new Size(42, 15);
			LblRecordCount.TabIndex = 18;
			LblRecordCount.Text = "(---件)";
			// 
			// RegressionAnalysisForm
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(809, 427);
			Controls.Add(LblRecordCount);
			Controls.Add(BtnLoadAnalysisData);
			Controls.Add(BtnRunAnalysis);
			Controls.Add(label9);
			Controls.Add(label8);
			Controls.Add(dataGridView1);
			Controls.Add(label1);
			Controls.Add(DgvAnalysisData);
			Controls.Add(groupBox1);
			Name = "RegressionAnalysisForm";
			Text = "RegressionAnalysisForm";
			((System.ComponentModel.ISupportInitialize)DgvAnalysisData).EndInit();
			((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
			groupBox1.ResumeLayout(false);
			groupBox1.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private DataGridView DgvAnalysisData;
		private Label label1;
		private ComboBox CmbResponseVariable;
		private Label label2;
		private Label label3;
		private CheckedListBox ClbPredictorVariable;
		private DataGridView dataGridView1;
		private GroupBox groupBox1;
		private Label label8;
		private Label label9;
		private Button BtnRunAnalysis;
		private Button BtnLoadAnalysisData;
		private Label LblRecordCount;
	}
}

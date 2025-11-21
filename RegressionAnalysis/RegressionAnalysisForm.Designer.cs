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
			DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
			DgvAnalysisData = new DataGridView();
			label1 = new Label();
			CmbResponseVariable = new ComboBox();
			label2 = new Label();
			label3 = new Label();
			ClbPredictorVariable = new CheckedListBox();
			DgvAnalysisResult = new DataGridView();
			groupBox1 = new GroupBox();
			label8 = new Label();
			label9 = new Label();
			BtnRunAnalysis = new Button();
			BtnLoadAnalysisData = new Button();
			LblRecordCount = new Label();
			LblRSquared = new Label();
			((System.ComponentModel.ISupportInitialize)DgvAnalysisData).BeginInit();
			((System.ComponentModel.ISupportInitialize)DgvAnalysisResult).BeginInit();
			groupBox1.SuspendLayout();
			SuspendLayout();
			// 
			// DgvAnalysisData
			// 
			dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = SystemColors.Control;
			dataGridViewCellStyle1.Font = new Font("Yu Gothic UI", 9F);
			dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
			dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
			DgvAnalysisData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			DgvAnalysisData.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = SystemColors.Window;
			dataGridViewCellStyle2.Font = new Font("Yu Gothic UI", 9F);
			dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
			dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
			dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
			dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
			DgvAnalysisData.DefaultCellStyle = dataGridViewCellStyle2;
			DgvAnalysisData.Location = new Point(42, 80);
			DgvAnalysisData.Name = "DgvAnalysisData";
			DgvAnalysisData.Size = new Size(414, 151);
			DgvAnalysisData.TabIndex = 0;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new Font("Yu Gothic UI", 9F, FontStyle.Bold);
			label1.Location = new Point(42, 62);
			label1.Name = "label1";
			label1.Size = new Size(58, 15);
			label1.TabIndex = 1;
			label1.Text = "分析データ";
			// 
			// CmbResponseVariable
			// 
			CmbResponseVariable.Font = new Font("Yu Gothic UI", 9F);
			CmbResponseVariable.FormattingEnabled = true;
			CmbResponseVariable.Location = new Point(20, 50);
			CmbResponseVariable.Name = "CmbResponseVariable";
			CmbResponseVariable.Size = new Size(121, 23);
			CmbResponseVariable.TabIndex = 2;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Font = new Font("Yu Gothic UI", 9F);
			label2.Location = new Point(20, 29);
			label2.Name = "label2";
			label2.Size = new Size(55, 15);
			label2.TabIndex = 1;
			label2.Text = "目的変数";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Font = new Font("Yu Gothic UI", 9F);
			label3.Location = new Point(158, 29);
			label3.Name = "label3";
			label3.Size = new Size(55, 15);
			label3.TabIndex = 1;
			label3.Text = "説明変数";
			// 
			// ClbPredictorVariable
			// 
			ClbPredictorVariable.CheckOnClick = true;
			ClbPredictorVariable.Font = new Font("Yu Gothic UI", 9F);
			ClbPredictorVariable.FormattingEnabled = true;
			ClbPredictorVariable.Location = new Point(158, 49);
			ClbPredictorVariable.Name = "ClbPredictorVariable";
			ClbPredictorVariable.Size = new Size(230, 94);
			ClbPredictorVariable.TabIndex = 3;
			// 
			// DgvAnalysisResult
			// 
			dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle3.BackColor = SystemColors.Control;
			dataGridViewCellStyle3.Font = new Font("Yu Gothic UI", 9F);
			dataGridViewCellStyle3.ForeColor = SystemColors.WindowText;
			dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
			dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
			dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
			DgvAnalysisResult.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
			DgvAnalysisResult.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle4.BackColor = SystemColors.Window;
			dataGridViewCellStyle4.Font = new Font("Yu Gothic UI", 9F);
			dataGridViewCellStyle4.ForeColor = SystemColors.ControlText;
			dataGridViewCellStyle4.SelectionBackColor = SystemColors.Highlight;
			dataGridViewCellStyle4.SelectionForeColor = SystemColors.HighlightText;
			dataGridViewCellStyle4.WrapMode = DataGridViewTriState.False;
			DgvAnalysisResult.DefaultCellStyle = dataGridViewCellStyle4;
			DgvAnalysisResult.Location = new Point(543, 98);
			DgvAnalysisResult.Name = "DgvAnalysisResult";
			DgvAnalysisResult.Size = new Size(243, 311);
			DgvAnalysisResult.TabIndex = 10;
			// 
			// groupBox1
			// 
			groupBox1.Controls.Add(label2);
			groupBox1.Controls.Add(label3);
			groupBox1.Controls.Add(CmbResponseVariable);
			groupBox1.Controls.Add(ClbPredictorVariable);
			groupBox1.Font = new Font("Yu Gothic UI", 9F, FontStyle.Bold);
			groupBox1.Location = new Point(42, 247);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new Size(414, 162);
			groupBox1.TabIndex = 11;
			groupBox1.TabStop = false;
			groupBox1.Text = "分析条件";
			// 
			// label8
			// 
			label8.BackColor = Color.BurlyWood;
			label8.BorderStyle = BorderStyle.Fixed3D;
			label8.Font = new Font("Yu Gothic UI", 9F, FontStyle.Bold);
			label8.Location = new Point(543, 58);
			label8.Name = "label8";
			label8.Size = new Size(243, 19);
			label8.TabIndex = 12;
			label8.Text = "分析結果";
			// 
			// label9
			// 
			label9.BackColor = Color.OldLace;
			label9.BorderStyle = BorderStyle.Fixed3D;
			label9.Font = new Font("Yu Gothic UI", 18F, FontStyle.Bold);
			label9.Location = new Point(12, 9);
			label9.Name = "label9";
			label9.Size = new Size(780, 35);
			label9.TabIndex = 13;
			label9.Text = "線形回帰分析";
			// 
			// BtnRunAnalysis
			// 
			BtnRunAnalysis.Location = new Point(462, 80);
			BtnRunAnalysis.Name = "BtnRunAnalysis";
			BtnRunAnalysis.Size = new Size(75, 151);
			BtnRunAnalysis.TabIndex = 16;
			BtnRunAnalysis.Text = "分析実行";
			BtnRunAnalysis.UseVisualStyleBackColor = true;
			BtnRunAnalysis.Click += BtnRunAnalysis_Click;
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
			// LblRSquared
			// 
			LblRSquared.AutoSize = true;
			LblRSquared.Location = new Point(554, 80);
			LblRSquared.Name = "LblRSquared";
			LblRSquared.Size = new Size(79, 15);
			LblRSquared.TabIndex = 19;
			LblRSquared.Text = "重決定係数：";
			// 
			// RegressionAnalysisForm
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(809, 427);
			Controls.Add(LblRSquared);
			Controls.Add(LblRecordCount);
			Controls.Add(BtnLoadAnalysisData);
			Controls.Add(BtnRunAnalysis);
			Controls.Add(label9);
			Controls.Add(label8);
			Controls.Add(DgvAnalysisResult);
			Controls.Add(label1);
			Controls.Add(DgvAnalysisData);
			Controls.Add(groupBox1);
			Font = new Font("Yu Gothic UI", 9F);
			Name = "RegressionAnalysisForm";
			Text = "RegressionAnalysisForm";
			((System.ComponentModel.ISupportInitialize)DgvAnalysisData).EndInit();
			((System.ComponentModel.ISupportInitialize)DgvAnalysisResult).EndInit();
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
		private DataGridView DgvAnalysisResult;
		private GroupBox groupBox1;
		private Label label8;
		private Label label9;
		private Button BtnRunAnalysis;
		private Button BtnLoadAnalysisData;
		private Label LblRecordCount;
		private Label LblRSquared;
	}
}

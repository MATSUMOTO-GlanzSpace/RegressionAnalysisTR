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
			DataGridViewCellStyle dataGridViewCellStyle9 = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle10 = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle11 = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle12 = new DataGridViewCellStyle();
			DgvAnalysisData = new DataGridView();
			label1 = new Label();
			CmbResponseVariable = new ComboBox();
			label2 = new Label();
			label3 = new Label();
			ClbPredictorVariable = new CheckedListBox();
			DgvAnalysisResult = new DataGridView();
			groupBox1 = new GroupBox();
			label9 = new Label();
			BtnRunAnalysis = new Button();
			BtnLoadAnalysisData = new Button();
			LblRecordCount = new Label();
			panel1 = new Panel();
			LblRSquared = new Label();
			label4 = new Label();
			label5 = new Label();
			((System.ComponentModel.ISupportInitialize)DgvAnalysisData).BeginInit();
			((System.ComponentModel.ISupportInitialize)DgvAnalysisResult).BeginInit();
			groupBox1.SuspendLayout();
			panel1.SuspendLayout();
			SuspendLayout();
			// 
			// DgvAnalysisData
			// 
			dataGridViewCellStyle9.Alignment = DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle9.BackColor = SystemColors.Control;
			dataGridViewCellStyle9.Font = new Font("Yu Gothic UI", 9F);
			dataGridViewCellStyle9.ForeColor = SystemColors.WindowText;
			dataGridViewCellStyle9.SelectionBackColor = SystemColors.Highlight;
			dataGridViewCellStyle9.SelectionForeColor = SystemColors.HighlightText;
			dataGridViewCellStyle9.WrapMode = DataGridViewTriState.True;
			DgvAnalysisData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle9;
			DgvAnalysisData.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridViewCellStyle10.Alignment = DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle10.BackColor = SystemColors.Window;
			dataGridViewCellStyle10.Font = new Font("Yu Gothic UI", 9F);
			dataGridViewCellStyle10.ForeColor = SystemColors.ControlText;
			dataGridViewCellStyle10.SelectionBackColor = SystemColors.Highlight;
			dataGridViewCellStyle10.SelectionForeColor = SystemColors.HighlightText;
			dataGridViewCellStyle10.WrapMode = DataGridViewTriState.False;
			DgvAnalysisData.DefaultCellStyle = dataGridViewCellStyle10;
			DgvAnalysisData.Location = new Point(42, 80);
			DgvAnalysisData.Name = "DgvAnalysisData";
			DgvAnalysisData.Size = new Size(962, 151);
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
			CmbResponseVariable.Location = new Point(20, 43);
			CmbResponseVariable.Name = "CmbResponseVariable";
			CmbResponseVariable.Size = new Size(137, 23);
			CmbResponseVariable.TabIndex = 2;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Font = new Font("Yu Gothic UI", 9F);
			label2.Location = new Point(20, 25);
			label2.Name = "label2";
			label2.Size = new Size(55, 15);
			label2.TabIndex = 1;
			label2.Text = "目的変数";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Font = new Font("Yu Gothic UI", 9F);
			label3.Location = new Point(176, 25);
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
			ClbPredictorVariable.Location = new Point(176, 43);
			ClbPredictorVariable.Name = "ClbPredictorVariable";
			ClbPredictorVariable.Size = new Size(198, 148);
			ClbPredictorVariable.TabIndex = 3;
			// 
			// DgvAnalysisResult
			// 
			dataGridViewCellStyle11.Alignment = DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle11.BackColor = SystemColors.Control;
			dataGridViewCellStyle11.Font = new Font("Yu Gothic UI", 9F);
			dataGridViewCellStyle11.ForeColor = SystemColors.WindowText;
			dataGridViewCellStyle11.SelectionBackColor = SystemColors.Highlight;
			dataGridViewCellStyle11.SelectionForeColor = SystemColors.HighlightText;
			dataGridViewCellStyle11.WrapMode = DataGridViewTriState.True;
			DgvAnalysisResult.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle11;
			DgvAnalysisResult.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridViewCellStyle12.Alignment = DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle12.BackColor = SystemColors.Window;
			dataGridViewCellStyle12.Font = new Font("Yu Gothic UI", 9F);
			dataGridViewCellStyle12.ForeColor = SystemColors.ControlText;
			dataGridViewCellStyle12.SelectionBackColor = SystemColors.Highlight;
			dataGridViewCellStyle12.SelectionForeColor = SystemColors.HighlightText;
			dataGridViewCellStyle12.WrapMode = DataGridViewTriState.False;
			DgvAnalysisResult.DefaultCellStyle = dataGridViewCellStyle12;
			DgvAnalysisResult.Location = new Point(37, 54);
			DgvAnalysisResult.Name = "DgvAnalysisResult";
			DgvAnalysisResult.Size = new Size(446, 127);
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
			groupBox1.Size = new Size(390, 199);
			groupBox1.TabIndex = 11;
			groupBox1.TabStop = false;
			groupBox1.Text = "分析条件";
			// 
			// label9
			// 
			label9.BackColor = Color.OldLace;
			label9.BorderStyle = BorderStyle.Fixed3D;
			label9.Font = new Font("Yu Gothic UI", 18F, FontStyle.Bold);
			label9.Location = new Point(12, 9);
			label9.Name = "label9";
			label9.Size = new Size(992, 35);
			label9.TabIndex = 13;
			label9.Text = "線形回帰分析";
			// 
			// BtnRunAnalysis
			// 
			BtnRunAnalysis.Location = new Point(438, 290);
			BtnRunAnalysis.Name = "BtnRunAnalysis";
			BtnRunAnalysis.Size = new Size(75, 156);
			BtnRunAnalysis.TabIndex = 16;
			BtnRunAnalysis.Text = "分析実行";
			BtnRunAnalysis.UseVisualStyleBackColor = true;
			BtnRunAnalysis.Click += BtnRunAnalysis_Click;
			// 
			// BtnLoadAnalysisData
			// 
			BtnLoadAnalysisData.Location = new Point(886, 54);
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
			// panel1
			// 
			panel1.BorderStyle = BorderStyle.Fixed3D;
			panel1.Controls.Add(label4);
			panel1.Controls.Add(label5);
			panel1.Controls.Add(LblRSquared);
			panel1.Controls.Add(DgvAnalysisResult);
			panel1.Location = new Point(519, 255);
			panel1.Name = "panel1";
			panel1.Size = new Size(493, 191);
			panel1.TabIndex = 21;
			// 
			// LblRSquared
			// 
			LblRSquared.AutoSize = true;
			LblRSquared.Location = new Point(122, 36);
			LblRSquared.Name = "LblRSquared";
			LblRSquared.Size = new Size(31, 15);
			LblRSquared.TabIndex = 19;
			LblRSquared.Text = "NaN";
			// 
			// label4
			// 
			label4.BackColor = Color.SeaShell;
			label4.BorderStyle = BorderStyle.Fixed3D;
			label4.Location = new Point(6, 5);
			label4.Name = "label4";
			label4.Size = new Size(477, 21);
			label4.TabIndex = 20;
			label4.Text = "分析結果";
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Location = new Point(37, 36);
			label5.Name = "label5";
			label5.Size = new Size(79, 15);
			label5.TabIndex = 19;
			label5.Text = "重決定係数：";
			// 
			// RegressionAnalysisForm
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1024, 458);
			Controls.Add(panel1);
			Controls.Add(LblRecordCount);
			Controls.Add(BtnLoadAnalysisData);
			Controls.Add(BtnRunAnalysis);
			Controls.Add(label9);
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
			panel1.ResumeLayout(false);
			panel1.PerformLayout();
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
		private Label label9;
		private Button BtnRunAnalysis;
		private Button BtnLoadAnalysisData;
		private Label LblRecordCount;
		private Panel panel1;
		private Label LblRSquared;
		private Label label4;
		private Label label5;
	}
}

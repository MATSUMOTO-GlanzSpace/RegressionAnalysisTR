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
			BtnExportResultCSV = new Button();
			label4 = new Label();
			label5 = new Label();
			LblRSquared = new Label();
			CmbFilterColumn = new ComboBox();
			TxtFilterString = new TextBox();
			BtnSetAnalisysData = new Button();
			label6 = new Label();
			label7 = new Label();
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
			DgvAnalysisData.Location = new Point(48, 107);
			DgvAnalysisData.Margin = new Padding(3, 4, 3, 4);
			DgvAnalysisData.Name = "DgvAnalysisData";
			DgvAnalysisData.RowHeadersWidth = 51;
			DgvAnalysisData.Size = new Size(1099, 201);
			DgvAnalysisData.TabIndex = 0;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new Font("Yu Gothic UI", 9F, FontStyle.Bold);
			label1.Location = new Point(48, 83);
			label1.Name = "label1";
			label1.Size = new Size(73, 20);
			label1.TabIndex = 1;
			label1.Text = "分析データ";
			// 
			// CmbResponseVariable
			// 
			CmbResponseVariable.Font = new Font("Yu Gothic UI", 9F);
			CmbResponseVariable.FormattingEnabled = true;
			CmbResponseVariable.Location = new Point(23, 57);
			CmbResponseVariable.Margin = new Padding(3, 4, 3, 4);
			CmbResponseVariable.Name = "CmbResponseVariable";
			CmbResponseVariable.Size = new Size(156, 28);
			CmbResponseVariable.TabIndex = 2;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Font = new Font("Yu Gothic UI", 9F);
			label2.Location = new Point(23, 33);
			label2.Name = "label2";
			label2.Size = new Size(69, 20);
			label2.TabIndex = 1;
			label2.Text = "目的変数";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Font = new Font("Yu Gothic UI", 9F);
			label3.Location = new Point(201, 33);
			label3.Name = "label3";
			label3.Size = new Size(69, 20);
			label3.TabIndex = 1;
			label3.Text = "説明変数";
			// 
			// ClbPredictorVariable
			// 
			ClbPredictorVariable.CheckOnClick = true;
			ClbPredictorVariable.Font = new Font("Yu Gothic UI", 9F);
			ClbPredictorVariable.FormattingEnabled = true;
			ClbPredictorVariable.Location = new Point(201, 57);
			ClbPredictorVariable.Margin = new Padding(3, 4, 3, 4);
			ClbPredictorVariable.Name = "ClbPredictorVariable";
			ClbPredictorVariable.Size = new Size(226, 180);
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
			DgvAnalysisResult.Location = new Point(42, 72);
			DgvAnalysisResult.Margin = new Padding(3, 4, 3, 4);
			DgvAnalysisResult.Name = "DgvAnalysisResult";
			DgvAnalysisResult.RowHeadersWidth = 51;
			DgvAnalysisResult.Size = new Size(510, 169);
			DgvAnalysisResult.TabIndex = 10;
			// 
			// groupBox1
			// 
			groupBox1.Controls.Add(label2);
			groupBox1.Controls.Add(label3);
			groupBox1.Controls.Add(CmbResponseVariable);
			groupBox1.Controls.Add(ClbPredictorVariable);
			groupBox1.Font = new Font("Yu Gothic UI", 9F, FontStyle.Bold);
			groupBox1.Location = new Point(48, 329);
			groupBox1.Margin = new Padding(3, 4, 3, 4);
			groupBox1.Name = "groupBox1";
			groupBox1.Padding = new Padding(3, 4, 3, 4);
			groupBox1.Size = new Size(446, 265);
			groupBox1.TabIndex = 11;
			groupBox1.TabStop = false;
			groupBox1.Text = "分析条件";
			// 
			// label9
			// 
			label9.BackColor = Color.OldLace;
			label9.BorderStyle = BorderStyle.Fixed3D;
			label9.Font = new Font("Yu Gothic UI", 18F, FontStyle.Bold);
			label9.Location = new Point(14, 12);
			label9.Name = "label9";
			label9.Size = new Size(1134, 47);
			label9.TabIndex = 13;
			label9.Text = "線形回帰分析";
			// 
			// BtnRunAnalysis
			// 
			BtnRunAnalysis.Location = new Point(501, 340);
			BtnRunAnalysis.Margin = new Padding(3, 4, 3, 4);
			BtnRunAnalysis.Name = "BtnRunAnalysis";
			BtnRunAnalysis.Size = new Size(86, 255);
			BtnRunAnalysis.TabIndex = 16;
			BtnRunAnalysis.Text = "分析実行";
			BtnRunAnalysis.UseVisualStyleBackColor = true;
			BtnRunAnalysis.Click += BtnRunAnalysis_Click;
			// 
			// BtnLoadAnalysisData
			// 
			BtnLoadAnalysisData.Location = new Point(1013, 72);
			BtnLoadAnalysisData.Margin = new Padding(3, 4, 3, 4);
			BtnLoadAnalysisData.Name = "BtnLoadAnalysisData";
			BtnLoadAnalysisData.Size = new Size(135, 31);
			BtnLoadAnalysisData.TabIndex = 17;
			BtnLoadAnalysisData.Text = "分析データ読込...";
			BtnLoadAnalysisData.UseVisualStyleBackColor = true;
			BtnLoadAnalysisData.Click += BtnLoadAnalysisData_Click;
			// 
			// LblRecordCount
			// 
			LblRecordCount.AutoSize = true;
			LblRecordCount.Location = new Point(120, 83);
			LblRecordCount.Name = "LblRecordCount";
			LblRecordCount.Size = new Size(52, 20);
			LblRecordCount.TabIndex = 18;
			LblRecordCount.Text = "(---件)";
			// 
			// panel1
			// 
			panel1.BorderStyle = BorderStyle.Fixed3D;
			panel1.Controls.Add(BtnExportResultCSV);
			panel1.Controls.Add(label4);
			panel1.Controls.Add(label5);
			panel1.Controls.Add(LblRSquared);
			panel1.Controls.Add(DgvAnalysisResult);
			panel1.Location = new Point(593, 340);
			panel1.Margin = new Padding(3, 4, 3, 4);
			panel1.Name = "panel1";
			panel1.Size = new Size(563, 253);
			panel1.TabIndex = 21;
			// 
			// BtnExportResultCSV
			// 
			BtnExportResultCSV.Location = new Point(417, 4);
			BtnExportResultCSV.Margin = new Padding(3, 4, 3, 4);
			BtnExportResultCSV.Name = "BtnExportResultCSV";
			BtnExportResultCSV.Size = new Size(135, 31);
			BtnExportResultCSV.TabIndex = 21;
			BtnExportResultCSV.Text = "CSV出力...";
			BtnExportResultCSV.UseVisualStyleBackColor = true;
			BtnExportResultCSV.Click += BtnExportResultCSV_Click;
			// 
			// label4
			// 
			label4.BackColor = Color.SeaShell;
			label4.BorderStyle = BorderStyle.Fixed3D;
			label4.Location = new Point(7, 7);
			label4.Name = "label4";
			label4.Size = new Size(403, 28);
			label4.TabIndex = 20;
			label4.Text = "分析結果";
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Location = new Point(42, 48);
			label5.Name = "label5";
			label5.Size = new Size(99, 20);
			label5.TabIndex = 19;
			label5.Text = "重決定係数：";
			// 
			// LblRSquared
			// 
			LblRSquared.AutoSize = true;
			LblRSquared.Location = new Point(139, 48);
			LblRSquared.Name = "LblRSquared";
			LblRSquared.Size = new Size(39, 20);
			LblRSquared.TabIndex = 19;
			LblRSquared.Text = "NaN";
			// 
			// CmbFilterColumn
			// 
			CmbFilterColumn.FormattingEnabled = true;
			CmbFilterColumn.Location = new Point(399, 78);
			CmbFilterColumn.Name = "CmbFilterColumn";
			CmbFilterColumn.Size = new Size(151, 28);
			CmbFilterColumn.TabIndex = 22;
			// 
			// TxtFilterString
			// 
			TxtFilterString.Location = new Point(637, 78);
			TxtFilterString.Name = "TxtFilterString";
			TxtFilterString.Size = new Size(167, 27);
			TxtFilterString.TabIndex = 23;
			// 
			// BtnSetAnalisysData
			// 
			BtnSetAnalisysData.Location = new Point(810, 77);
			BtnSetAnalisysData.Name = "BtnSetAnalisysData";
			BtnSetAnalisysData.Size = new Size(94, 29);
			BtnSetAnalisysData.TabIndex = 24;
			BtnSetAnalisysData.Text = "データ抽出";
			BtnSetAnalisysData.UseVisualStyleBackColor = true;
			BtnSetAnalisysData.Click += BtnSetAnalisysData_Click;
			// 
			// label6
			// 
			label6.AutoSize = true;
			label6.Location = new Point(319, 81);
			label6.Name = "label6";
			label6.Size = new Size(84, 20);
			label6.TabIndex = 25;
			label6.Text = "抽出列名：";
			// 
			// label7
			// 
			label7.AutoSize = true;
			label7.Location = new Point(562, 83);
			label7.Name = "label7";
			label7.Size = new Size(69, 20);
			label7.TabIndex = 26;
			label7.Text = "抽出名：";
			// 
			// RegressionAnalysisForm
			// 
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1170, 611);
			Controls.Add(label7);
			Controls.Add(label6);
			Controls.Add(BtnSetAnalisysData);
			Controls.Add(TxtFilterString);
			Controls.Add(CmbFilterColumn);
			Controls.Add(panel1);
			Controls.Add(LblRecordCount);
			Controls.Add(BtnLoadAnalysisData);
			Controls.Add(BtnRunAnalysis);
			Controls.Add(label9);
			Controls.Add(label1);
			Controls.Add(DgvAnalysisData);
			Controls.Add(groupBox1);
			Font = new Font("Yu Gothic UI", 9F);
			Margin = new Padding(3, 4, 3, 4);
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
		private Button BtnExportResultCSV;
		private ComboBox CmbFilterColumn;
		private TextBox TxtFilterString;
		private Button BtnSetAnalisysData;
		private Label label6;
		private Label label7;
	}
}

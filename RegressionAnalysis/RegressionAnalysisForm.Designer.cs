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
			BtnRunAnalysis = new Button();
			BtnSelectSalesCSVFile = new Button();
			BtnWeatherCSVFile = new Button();
			TxtSalseCSVFileName = new TextBox();
			TxtWeatherCSVFIleName = new TextBox();
			RbnCSVFile = new RadioButton();
			RbnDataBase = new RadioButton();
			label4 = new Label();
			label5 = new Label();
			label6 = new Label();
			label7 = new Label();
			TxtSalesTableName = new TextBox();
			TxtWeatherTableName = new TextBox();
			GrpAnalysisData = new GroupBox();
			BtnLoadAnalysisData = new Button();
			dataGridView1 = new DataGridView();
			groupBox1 = new GroupBox();
			label8 = new Label();
			label9 = new Label();
			((System.ComponentModel.ISupportInitialize)DgvAnalysisData).BeginInit();
			GrpAnalysisData.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
			groupBox1.SuspendLayout();
			SuspendLayout();
			// 
			// DgvAnalysisData
			// 
			DgvAnalysisData.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			DgvAnalysisData.Location = new Point(42, 62);
			DgvAnalysisData.Name = "DgvAnalysisData";
			DgvAnalysisData.Size = new Size(623, 169);
			DgvAnalysisData.TabIndex = 0;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(42, 44);
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
			ClbPredictorVariable.FormattingEnabled = true;
			ClbPredictorVariable.Location = new Point(158, 49);
			ClbPredictorVariable.Name = "ClbPredictorVariable";
			ClbPredictorVariable.Size = new Size(230, 130);
			ClbPredictorVariable.TabIndex = 3;
			// 
			// BtnRunAnalysis
			// 
			BtnRunAnalysis.Location = new Point(493, 276);
			BtnRunAnalysis.Name = "BtnRunAnalysis";
			BtnRunAnalysis.Size = new Size(75, 151);
			BtnRunAnalysis.TabIndex = 4;
			BtnRunAnalysis.Text = "分析実行";
			BtnRunAnalysis.UseVisualStyleBackColor = true;
			// 
			// BtnSelectSalesCSVFile
			// 
			BtnSelectSalesCSVFile.Location = new Point(923, 63);
			BtnSelectSalesCSVFile.Name = "BtnSelectSalesCSVFile";
			BtnSelectSalesCSVFile.Size = new Size(32, 23);
			BtnSelectSalesCSVFile.TabIndex = 5;
			BtnSelectSalesCSVFile.Text = "...";
			BtnSelectSalesCSVFile.UseVisualStyleBackColor = true;
			// 
			// BtnWeatherCSVFile
			// 
			BtnWeatherCSVFile.Location = new Point(923, 92);
			BtnWeatherCSVFile.Name = "BtnWeatherCSVFile";
			BtnWeatherCSVFile.Size = new Size(32, 23);
			BtnWeatherCSVFile.TabIndex = 5;
			BtnWeatherCSVFile.Text = "...";
			BtnWeatherCSVFile.UseVisualStyleBackColor = true;
			// 
			// TxtSalseCSVFileName
			// 
			TxtSalseCSVFileName.Location = new Point(817, 64);
			TxtSalseCSVFileName.Name = "TxtSalseCSVFileName";
			TxtSalseCSVFileName.Size = new Size(100, 23);
			TxtSalseCSVFileName.TabIndex = 6;
			// 
			// TxtWeatherCSVFIleName
			// 
			TxtWeatherCSVFIleName.Location = new Point(817, 93);
			TxtWeatherCSVFIleName.Name = "TxtWeatherCSVFIleName";
			TxtWeatherCSVFIleName.Size = new Size(100, 23);
			TxtWeatherCSVFIleName.TabIndex = 6;
			// 
			// RbnCSVFile
			// 
			RbnCSVFile.AutoSize = true;
			RbnCSVFile.Location = new Point(705, 68);
			RbnCSVFile.Name = "RbnCSVFile";
			RbnCSVFile.Size = new Size(63, 19);
			RbnCSVFile.TabIndex = 7;
			RbnCSVFile.TabStop = true;
			RbnCSVFile.Text = "CSVFile";
			RbnCSVFile.UseVisualStyleBackColor = true;
			// 
			// RbnDataBase
			// 
			RbnDataBase.AutoSize = true;
			RbnDataBase.Location = new Point(705, 125);
			RbnDataBase.Name = "RbnDataBase";
			RbnDataBase.Size = new Size(40, 19);
			RbnDataBase.TabIndex = 7;
			RbnDataBase.TabStop = true;
			RbnDataBase.Text = "DB";
			RbnDataBase.UseVisualStyleBackColor = true;
			// 
			// label4
			// 
			label4.AutoSize = true;
			label4.Location = new Point(773, 68);
			label4.Name = "label4";
			label4.Size = new Size(31, 15);
			label4.TabIndex = 8;
			label4.Text = "販売";
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Location = new Point(773, 96);
			label5.Name = "label5";
			label5.Size = new Size(31, 15);
			label5.TabIndex = 8;
			label5.Text = "気象";
			// 
			// label6
			// 
			label6.AutoSize = true;
			label6.Location = new Point(773, 127);
			label6.Name = "label6";
			label6.Size = new Size(31, 15);
			label6.TabIndex = 8;
			label6.Text = "販売";
			// 
			// label7
			// 
			label7.AutoSize = true;
			label7.Location = new Point(773, 155);
			label7.Name = "label7";
			label7.Size = new Size(31, 15);
			label7.TabIndex = 8;
			label7.Text = "気象";
			// 
			// TxtSalesTableName
			// 
			TxtSalesTableName.Location = new Point(817, 121);
			TxtSalesTableName.Name = "TxtSalesTableName";
			TxtSalesTableName.Size = new Size(100, 23);
			TxtSalesTableName.TabIndex = 6;
			TxtSalesTableName.Text = "sales";
			// 
			// TxtWeatherTableName
			// 
			TxtWeatherTableName.Location = new Point(817, 150);
			TxtWeatherTableName.Name = "TxtWeatherTableName";
			TxtWeatherTableName.Size = new Size(100, 23);
			TxtWeatherTableName.TabIndex = 6;
			TxtWeatherTableName.Text = "weather";
			// 
			// GrpAnalysisData
			// 
			GrpAnalysisData.Controls.Add(BtnLoadAnalysisData);
			GrpAnalysisData.Location = new Point(671, 42);
			GrpAnalysisData.Name = "GrpAnalysisData";
			GrpAnalysisData.Size = new Size(309, 188);
			GrpAnalysisData.TabIndex = 9;
			GrpAnalysisData.TabStop = false;
			GrpAnalysisData.Text = "読込分析データ";
			// 
			// BtnLoadAnalysisData
			// 
			BtnLoadAnalysisData.Location = new Point(203, 155);
			BtnLoadAnalysisData.Name = "BtnLoadAnalysisData";
			BtnLoadAnalysisData.Size = new Size(100, 23);
			BtnLoadAnalysisData.TabIndex = 4;
			BtnLoadAnalysisData.Text = "分析データ読込";
			BtnLoadAnalysisData.UseVisualStyleBackColor = true;
			// 
			// dataGridView1
			// 
			dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridView1.Location = new Point(574, 276);
			dataGridView1.Name = "dataGridView1";
			dataGridView1.Size = new Size(406, 150);
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
			groupBox1.Size = new Size(414, 194);
			groupBox1.TabIndex = 11;
			groupBox1.TabStop = false;
			groupBox1.Text = "分析条件";
			// 
			// label8
			// 
			label8.AutoSize = true;
			label8.Location = new Point(574, 258);
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
			label9.Size = new Size(653, 35);
			label9.TabIndex = 13;
			label9.Text = "重回帰分析(Sales-Weather)";
			// 
			// RegressionAnalysisForm
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1009, 454);
			Controls.Add(label9);
			Controls.Add(label8);
			Controls.Add(dataGridView1);
			Controls.Add(label7);
			Controls.Add(label6);
			Controls.Add(label5);
			Controls.Add(label4);
			Controls.Add(RbnDataBase);
			Controls.Add(RbnCSVFile);
			Controls.Add(TxtWeatherTableName);
			Controls.Add(TxtWeatherCSVFIleName);
			Controls.Add(TxtSalesTableName);
			Controls.Add(TxtSalseCSVFileName);
			Controls.Add(BtnWeatherCSVFile);
			Controls.Add(BtnSelectSalesCSVFile);
			Controls.Add(BtnRunAnalysis);
			Controls.Add(label1);
			Controls.Add(DgvAnalysisData);
			Controls.Add(GrpAnalysisData);
			Controls.Add(groupBox1);
			Name = "RegressionAnalysisForm";
			Text = "RegressionAnalysisForm";
			((System.ComponentModel.ISupportInitialize)DgvAnalysisData).EndInit();
			GrpAnalysisData.ResumeLayout(false);
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
		private Button BtnRunAnalysis;
		private Button BtnSelectSalesCSVFile;
		private Button BtnWeatherCSVFile;
		private TextBox TxtSalseCSVFileName;
		private TextBox TxtWeatherCSVFIleName;
		private RadioButton RbnCSVFile;
		private RadioButton RbnDataBase;
		private Label label4;
		private Label label5;
		private Label label6;
		private Label label7;
		private TextBox TxtSalesTableName;
		private TextBox TxtWeatherTableName;
		private GroupBox GrpAnalysisData;
		private Button BtnLoadAnalysisData;
		private DataGridView dataGridView1;
		private GroupBox groupBox1;
		private Label label8;
		private Label label9;
	}
}

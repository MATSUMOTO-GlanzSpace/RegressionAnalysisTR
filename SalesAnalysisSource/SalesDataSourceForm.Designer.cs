namespace SalesAnalysisSource
{
	partial class SalesDataSourceForm
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
			BtnLoadDBAnalysisData = new Button();
			BtnSelectUnitsCSVFile = new Button();
			TxtSalesTableName = new TextBox();
			BtnSelectSalesCSVFile = new Button();
			TxtWeatherTableName = new TextBox();
			BtnSelectWeatherCSVFile = new Button();
			TxtUnitsCSVFileName = new TextBox();
			TxtSalseCSVFileName = new TextBox();
			TxtWeatherCSVFileName = new TextBox();
			label7 = new Label();
			label6 = new Label();
			label4 = new Label();
			label11 = new Label();
			label5 = new Label();
			TxtUnitsTableName = new TextBox();
			label10 = new Label();
			TabControlSourceType = new TabControl();
			tabPageCSV = new TabPage();
			BtnLoadCSVAnalysisData = new Button();
			CmbTxtFilterVarietyCSV = new ComboBox();
			label1 = new Label();
			label3 = new Label();
			tabPageDataBase = new TabPage();
			CmbTxtFilterVarietyDB = new ComboBox();
			label8 = new Label();
			label2 = new Label();
			BtnCancel = new Button();
			TabControlSourceType.SuspendLayout();
			tabPageCSV.SuspendLayout();
			tabPageDataBase.SuspendLayout();
			SuspendLayout();
			// 
			// BtnLoadDBAnalysisData
			// 
			BtnLoadDBAnalysisData.DialogResult = DialogResult.OK;
			BtnLoadDBAnalysisData.Location = new Point(283, 199);
			BtnLoadDBAnalysisData.Name = "BtnLoadDBAnalysisData";
			BtnLoadDBAnalysisData.Size = new Size(100, 23);
			BtnLoadDBAnalysisData.TabIndex = 13;
			BtnLoadDBAnalysisData.Text = "分析データ読込";
			BtnLoadDBAnalysisData.UseVisualStyleBackColor = true;
			BtnLoadDBAnalysisData.Click += BtnLoadDBAnalysisData_Click;
			// 
			// BtnSelectUnitsCSVFile
			// 
			BtnSelectUnitsCSVFile.Location = new Point(329, 98);
			BtnSelectUnitsCSVFile.Name = "BtnSelectUnitsCSVFile";
			BtnSelectUnitsCSVFile.Size = new Size(32, 23);
			BtnSelectUnitsCSVFile.TabIndex = 5;
			BtnSelectUnitsCSVFile.Text = "...";
			BtnSelectUnitsCSVFile.UseVisualStyleBackColor = true;
			BtnSelectUnitsCSVFile.Click += BtnSelectUnitsCSVFile_Click;
			// 
			// TxtSalesTableName
			// 
			TxtSalesTableName.Location = new Point(75, 42);
			TxtSalesTableName.Name = "TxtSalesTableName";
			TxtSalesTableName.Size = new Size(100, 23);
			TxtSalesTableName.TabIndex = 6;
			TxtSalesTableName.Text = "sales";
			// 
			// BtnSelectSalesCSVFile
			// 
			BtnSelectSalesCSVFile.Location = new Point(329, 40);
			BtnSelectSalesCSVFile.Name = "BtnSelectSalesCSVFile";
			BtnSelectSalesCSVFile.Size = new Size(32, 23);
			BtnSelectSalesCSVFile.TabIndex = 5;
			BtnSelectSalesCSVFile.Text = "...";
			BtnSelectSalesCSVFile.UseVisualStyleBackColor = true;
			BtnSelectSalesCSVFile.Click += BtnSelectSalesCSVFile_Click;
			// 
			// TxtWeatherTableName
			// 
			TxtWeatherTableName.Location = new Point(75, 71);
			TxtWeatherTableName.Name = "TxtWeatherTableName";
			TxtWeatherTableName.Size = new Size(100, 23);
			TxtWeatherTableName.TabIndex = 6;
			TxtWeatherTableName.Text = "weather";
			// 
			// BtnSelectWeatherCSVFile
			// 
			BtnSelectWeatherCSVFile.Location = new Point(329, 69);
			BtnSelectWeatherCSVFile.Name = "BtnSelectWeatherCSVFile";
			BtnSelectWeatherCSVFile.Size = new Size(32, 23);
			BtnSelectWeatherCSVFile.TabIndex = 5;
			BtnSelectWeatherCSVFile.Text = "...";
			BtnSelectWeatherCSVFile.UseVisualStyleBackColor = true;
			BtnSelectWeatherCSVFile.Click += BtnSelectWeatherCSVFile_Click;
			// 
			// TxtUnitsCSVFileName
			// 
			TxtUnitsCSVFileName.Location = new Point(83, 98);
			TxtUnitsCSVFileName.Name = "TxtUnitsCSVFileName";
			TxtUnitsCSVFileName.Size = new Size(240, 23);
			TxtUnitsCSVFileName.TabIndex = 6;
			TxtUnitsCSVFileName.Text = "Units.csv";
			// 
			// TxtSalseCSVFileName
			// 
			TxtSalseCSVFileName.Location = new Point(83, 40);
			TxtSalseCSVFileName.Name = "TxtSalseCSVFileName";
			TxtSalseCSVFileName.Size = new Size(240, 23);
			TxtSalseCSVFileName.TabIndex = 6;
			TxtSalseCSVFileName.Text = "Sales.csv";
			// 
			// TxtWeatherCSVFileName
			// 
			TxtWeatherCSVFileName.Location = new Point(83, 69);
			TxtWeatherCSVFileName.Name = "TxtWeatherCSVFileName";
			TxtWeatherCSVFileName.Size = new Size(240, 23);
			TxtWeatherCSVFileName.TabIndex = 6;
			TxtWeatherCSVFileName.Text = "Weather.csv";
			// 
			// label7
			// 
			label7.AutoSize = true;
			label7.Location = new Point(31, 76);
			label7.Name = "label7";
			label7.Size = new Size(43, 15);
			label7.TabIndex = 8;
			label7.Text = "気象：";
			// 
			// label6
			// 
			label6.AutoSize = true;
			label6.Location = new Point(31, 48);
			label6.Name = "label6";
			label6.Size = new Size(43, 15);
			label6.TabIndex = 8;
			label6.Text = "販売：";
			// 
			// label4
			// 
			label4.AutoSize = true;
			label4.Location = new Point(39, 44);
			label4.Name = "label4";
			label4.Size = new Size(43, 15);
			label4.TabIndex = 8;
			label4.Text = "販売：";
			// 
			// label11
			// 
			label11.AutoSize = true;
			label11.Location = new Point(39, 101);
			label11.Name = "label11";
			label11.Size = new Size(43, 15);
			label11.TabIndex = 8;
			label11.Text = "単位：";
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Location = new Point(39, 72);
			label5.Name = "label5";
			label5.Size = new Size(43, 15);
			label5.TabIndex = 8;
			label5.Text = "気象：";
			// 
			// TxtUnitsTableName
			// 
			TxtUnitsTableName.Location = new Point(75, 100);
			TxtUnitsTableName.Name = "TxtUnitsTableName";
			TxtUnitsTableName.Size = new Size(100, 23);
			TxtUnitsTableName.TabIndex = 6;
			TxtUnitsTableName.Text = "units";
			// 
			// label10
			// 
			label10.AutoSize = true;
			label10.Location = new Point(31, 106);
			label10.Name = "label10";
			label10.Size = new Size(43, 15);
			label10.TabIndex = 8;
			label10.Text = "単位：";
			// 
			// TabControlSourceType
			// 
			TabControlSourceType.Controls.Add(tabPageCSV);
			TabControlSourceType.Controls.Add(tabPageDataBase);
			TabControlSourceType.Location = new Point(3, 24);
			TabControlSourceType.Name = "TabControlSourceType";
			TabControlSourceType.SelectedIndex = 0;
			TabControlSourceType.Size = new Size(397, 256);
			TabControlSourceType.TabIndex = 17;
			// 
			// tabPageCSV
			// 
			tabPageCSV.Controls.Add(BtnLoadCSVAnalysisData);
			tabPageCSV.Controls.Add(CmbTxtFilterVarietyCSV);
			tabPageCSV.Controls.Add(label1);
			tabPageCSV.Controls.Add(label3);
			tabPageCSV.Controls.Add(TxtSalseCSVFileName);
			tabPageCSV.Controls.Add(TxtUnitsCSVFileName);
			tabPageCSV.Controls.Add(label5);
			tabPageCSV.Controls.Add(BtnSelectWeatherCSVFile);
			tabPageCSV.Controls.Add(TxtWeatherCSVFileName);
			tabPageCSV.Controls.Add(label11);
			tabPageCSV.Controls.Add(BtnSelectSalesCSVFile);
			tabPageCSV.Controls.Add(label4);
			tabPageCSV.Controls.Add(BtnSelectUnitsCSVFile);
			tabPageCSV.Location = new Point(4, 24);
			tabPageCSV.Name = "tabPageCSV";
			tabPageCSV.Padding = new Padding(3);
			tabPageCSV.Size = new Size(389, 228);
			tabPageCSV.TabIndex = 0;
			tabPageCSV.Text = "CSV形式";
			tabPageCSV.UseVisualStyleBackColor = true;
			// 
			// BtnLoadCSVAnalysisData
			// 
			BtnLoadCSVAnalysisData.DialogResult = DialogResult.OK;
			BtnLoadCSVAnalysisData.Location = new Point(261, 199);
			BtnLoadCSVAnalysisData.Name = "BtnLoadCSVAnalysisData";
			BtnLoadCSVAnalysisData.Size = new Size(100, 23);
			BtnLoadCSVAnalysisData.TabIndex = 20;
			BtnLoadCSVAnalysisData.Text = "分析データ読込";
			BtnLoadCSVAnalysisData.UseVisualStyleBackColor = true;
			BtnLoadCSVAnalysisData.Click += BtnLoadCSVAnalysisData_Click;
			// 
			// CmbTxtFilterVarietyCSV
			// 
			CmbTxtFilterVarietyCSV.FormattingEnabled = true;
			CmbTxtFilterVarietyCSV.ImeMode = ImeMode.On;
			CmbTxtFilterVarietyCSV.Location = new Point(83, 150);
			CmbTxtFilterVarietyCSV.Name = "CmbTxtFilterVarietyCSV";
			CmbTxtFilterVarietyCSV.Size = new Size(278, 23);
			CmbTxtFilterVarietyCSV.TabIndex = 19;
			CmbTxtFilterVarietyCSV.DropDown += CmbTxtFilterVarietyCSV_DropDown;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(23, 13);
			label1.Name = "label1";
			label1.Size = new Size(80, 15);
			label1.TabIndex = 9;
			label1.Text = "CSVファイルパス";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Location = new Point(39, 153);
			label3.Name = "label3";
			label3.Size = new Size(43, 15);
			label3.TabIndex = 18;
			label3.Text = "品種：";
			// 
			// tabPageDataBase
			// 
			tabPageDataBase.Controls.Add(CmbTxtFilterVarietyDB);
			tabPageDataBase.Controls.Add(BtnLoadDBAnalysisData);
			tabPageDataBase.Controls.Add(label8);
			tabPageDataBase.Controls.Add(label2);
			tabPageDataBase.Controls.Add(label10);
			tabPageDataBase.Controls.Add(TxtSalesTableName);
			tabPageDataBase.Controls.Add(TxtWeatherTableName);
			tabPageDataBase.Controls.Add(TxtUnitsTableName);
			tabPageDataBase.Controls.Add(label7);
			tabPageDataBase.Controls.Add(label6);
			tabPageDataBase.Location = new Point(4, 24);
			tabPageDataBase.Name = "tabPageDataBase";
			tabPageDataBase.Padding = new Padding(3);
			tabPageDataBase.Size = new Size(389, 228);
			tabPageDataBase.TabIndex = 1;
			tabPageDataBase.Text = "DataBase形式";
			tabPageDataBase.UseVisualStyleBackColor = true;
			// 
			// CmbTxtFilterVarietyDB
			// 
			CmbTxtFilterVarietyDB.FormattingEnabled = true;
			CmbTxtFilterVarietyDB.Location = new Point(75, 150);
			CmbTxtFilterVarietyDB.Name = "CmbTxtFilterVarietyDB";
			CmbTxtFilterVarietyDB.Size = new Size(261, 23);
			CmbTxtFilterVarietyDB.TabIndex = 21;
			CmbTxtFilterVarietyDB.DropDown += CmbTxtFilterVarietyDB_DropDown;
			// 
			// label8
			// 
			label8.AutoSize = true;
			label8.Location = new Point(31, 153);
			label8.Name = "label8";
			label8.Size = new Size(43, 15);
			label8.TabIndex = 20;
			label8.Text = "品種：";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new Point(21, 13);
			label2.Name = "label2";
			label2.Size = new Size(42, 15);
			label2.TabIndex = 9;
			label2.Text = "テーブル";
			// 
			// BtnCancel
			// 
			BtnCancel.DialogResult = DialogResult.Cancel;
			BtnCancel.Location = new Point(310, 12);
			BtnCancel.Name = "BtnCancel";
			BtnCancel.Size = new Size(75, 23);
			BtnCancel.TabIndex = 18;
			BtnCancel.Text = "キャンセル";
			BtnCancel.UseVisualStyleBackColor = true;
			// 
			// SalesDataSourceForm
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(408, 288);
			Controls.Add(BtnCancel);
			Controls.Add(TabControlSourceType);
			Name = "SalesDataSourceForm";
			Text = "売上分析用データ準備";
			TabControlSourceType.ResumeLayout(false);
			tabPageCSV.ResumeLayout(false);
			tabPageCSV.PerformLayout();
			tabPageDataBase.ResumeLayout(false);
			tabPageDataBase.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		private Button BtnLoadDBAnalysisData;
		private Button BtnSelectUnitsCSVFile;
		private Button BtnSelectSalesCSVFile;
		private Button BtnSelectWeatherCSVFile;
		private Label label7;
		private Label label6;
		private Label label4;
		private Label label11;
		private Label label5;
		private Label label10;
		private TabControl TabControlSourceType;
		private TabPage tabPageCSV;
		private TabPage tabPageDataBase;
		private Label label1;
		private Label label2;
		private Label label3;
		private Label label8;
		private Button BtnLoadCSVAnalysisData;
		public TextBox TxtSalesTableName;
		public TextBox TxtWeatherTableName;
		public TextBox TxtUnitsCSVFileName;
		public TextBox TxtSalseCSVFileName;
		public TextBox TxtWeatherCSVFileName;
		public TextBox TxtUnitsTableName;
		public ComboBox CmbTxtFilterVarietyCSV;
		public ComboBox CmbTxtFilterVarietyDB;
		private Button BtnCancel;
	}
}
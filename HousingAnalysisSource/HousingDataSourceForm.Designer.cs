namespace HousingAnalysisSource
{
	partial class HousingDataSourceForm
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
			BtnLoadCSVAnalysisData = new Button();
			TxtHousingCSVFileName = new TextBox();
			BtnSelectHousingCSVFile = new Button();
			label1 = new Label();
			SuspendLayout();
			// 
			// BtnLoadCSVAnalysisData
			// 
			BtnLoadCSVAnalysisData.Location = new Point(281, 12);
			BtnLoadCSVAnalysisData.Name = "BtnLoadCSVAnalysisData";
			BtnLoadCSVAnalysisData.Size = new Size(133, 29);
			BtnLoadCSVAnalysisData.TabIndex = 0;
			BtnLoadCSVAnalysisData.Text = "分析データ読込";
			BtnLoadCSVAnalysisData.UseVisualStyleBackColor = true;
			BtnLoadCSVAnalysisData.Click += BtnLoadCSVAnalysisData_Click;
			// 
			// TxtHousingCSVFileName
			// 
			TxtHousingCSVFileName.Location = new Point(43, 61);
			TxtHousingCSVFileName.Name = "TxtHousingCSVFileName";
			TxtHousingCSVFileName.Size = new Size(371, 27);
			TxtHousingCSVFileName.TabIndex = 1;
			// 
			// BtnSelectHousingCSVFile
			// 
			BtnSelectHousingCSVFile.Location = new Point(420, 61);
			BtnSelectHousingCSVFile.Name = "BtnSelectHousingCSVFile";
			BtnSelectHousingCSVFile.Size = new Size(42, 29);
			BtnSelectHousingCSVFile.TabIndex = 2;
			BtnSelectHousingCSVFile.Text = "...";
			BtnSelectHousingCSVFile.UseVisualStyleBackColor = true;
			BtnSelectHousingCSVFile.Click += BtnSelectHousingCSVFile_Click;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(43, 38);
			label1.Name = "label1";
			label1.Size = new Size(146, 20);
			label1.TabIndex = 3;
			label1.Text = "カリフォルニア住宅価格";
			// 
			// HousingDataSourceForm
			// 
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(525, 113);
			Controls.Add(label1);
			Controls.Add(BtnSelectHousingCSVFile);
			Controls.Add(TxtHousingCSVFileName);
			Controls.Add(BtnLoadCSVAnalysisData);
			Name = "HousingDataSourceForm";
			Text = "HousingDataSource";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Button BtnLoadCSVAnalysisData;
		private TextBox TxtHousingCSVFileName;
		private Button BtnSelectHousingCSVFile;
		private Label label1;
	}
}
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
			TabControlSourceType = new TabControl();
			tabPageCSV = new TabPage();
			tabPageDataBase = new TabPage();
			TabControlSourceType.SuspendLayout();
			SuspendLayout();
			// 
			// TabControlSourceType
			// 
			TabControlSourceType.Controls.Add(tabPageCSV);
			TabControlSourceType.Controls.Add(tabPageDataBase);
			TabControlSourceType.Location = new Point(3, 32);
			TabControlSourceType.Margin = new Padding(3, 4, 3, 4);
			TabControlSourceType.Name = "TabControlSourceType";
			TabControlSourceType.SelectedIndex = 0;
			TabControlSourceType.Size = new Size(454, 341);
			TabControlSourceType.TabIndex = 17;
			// 
			// tabPageCSV
			// 
			tabPageCSV.Location = new Point(4, 29);
			tabPageCSV.Margin = new Padding(3, 4, 3, 4);
			tabPageCSV.Name = "tabPageCSV";
			tabPageCSV.Padding = new Padding(3, 4, 3, 4);
			tabPageCSV.Size = new Size(446, 308);
			tabPageCSV.TabIndex = 0;
			tabPageCSV.Text = "CSV形式";
			tabPageCSV.UseVisualStyleBackColor = true;
			// 
			// tabPageDataBase
			// 
			tabPageDataBase.Location = new Point(4, 29);
			tabPageDataBase.Margin = new Padding(3, 4, 3, 4);
			tabPageDataBase.Name = "tabPageDataBase";
			tabPageDataBase.Padding = new Padding(3, 4, 3, 4);
			tabPageDataBase.Size = new Size(446, 308);
			tabPageDataBase.TabIndex = 1;
			tabPageDataBase.Text = "DataBase形式";
			tabPageDataBase.UseVisualStyleBackColor = true;
			// 
			// SalesDataSourceForm
			// 
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(466, 384);
			Controls.Add(TabControlSourceType);
			Margin = new Padding(3, 4, 3, 4);
			Name = "SalesDataSourceForm";
			Text = "売上分析用データ準備";
			TabControlSourceType.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion
		private TabControl TabControlSourceType;
		private TabPage tabPageCSV;
		private TabPage tabPageDataBase;
	}
}
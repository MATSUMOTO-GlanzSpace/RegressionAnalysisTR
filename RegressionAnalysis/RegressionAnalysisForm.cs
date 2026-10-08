using RegressionAnalysis.Common;
using SalesAnalysisSource;
using System.Data;
using System.Text;

namespace RegressionAnalysis
{
	/// <summary>
	/// 回帰分析フォームクラス
	/// </summary>
	public partial class RegressionAnalysisForm : Form
	{
		/// <summary>
		/// RegressionAnalysisForm クラスの新しいインスタンスを初期化します
		/// </summary>
		public RegressionAnalysisForm()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			var dlg = new SalesDataSourceForm();
			dlg.ShowDialog();
		}
	}
}

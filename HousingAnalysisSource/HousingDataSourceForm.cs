using RegressionAnalysis.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HousingAnalysisSource
{
	public partial class HousingDataSourceForm : Form
	{
		// メンバー変数定義
		/// <summary>
		/// ファイル選択ダイアログ
		/// 使い回すことで、複数回開いたときに前回のディレクトリを覚えておける
		/// </summary>
		private readonly OpenFileDialog selectFileDialog = new();

		/// <summary>
		/// DataMerger（呼出し側でGetMergedDataTable実行するために保持する）
		/// </summary>
		public DataMerger? DataMerger { get; private set; }

		// コンストラクタ
		/// <summary>
		/// フォームの初期化を行う
		/// </summary>
		public HousingDataSourceForm()
		{
			InitializeComponent();

			// ファイル選択ダイアログのフィルター設定
			selectFileDialog.Filter = "CSVファイル (*.csv)|*.csv|すべてのファイル (*.*)|*.*";
		}

	}
}

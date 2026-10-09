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

		/// <summary>
		/// (CSV)分析データを読み込むボタンがクリックされるときに発生するイベント ハンドラー
		/// </summary>
		/// <param name="sender">イベントの送信元</param>
		/// <param name="e">イベント データ</param>
		private void BtnLoadCSVAnalysisData_Click(object sender, EventArgs e)
		{
			// CSVデータソースマージャー生成（実際のデータ取得は呼出し側で遅延実行する）
			var csvMerger = new HousingDataMerger.CsvHousingDataMerger(TxtHousingCSVFileName.Text);
			// DataMerger を保持
			DataMerger = csvMerger;
		}

		// イベントハンドラー定義
		/// <summary>
		/// 住宅価格CSVファイルを選択するボタンがクリックされたときに発生するイベント ハンドラー
		/// </summary>
		/// <param name="sender">イベントの送信元</param>
		/// <param name="e">イベント データ</param>
		private void BtnSelectHousingCSVFile_Click(object sender, EventArgs e)
		{
			selectFileDialog.Title = "住宅価格CSVファイルを選択してください";
			selectFileDialog.FileName = TxtHousingCSVFileName.Text;
			if (selectFileDialog.ShowDialog(this) == DialogResult.OK)
				TxtHousingCSVFileName.Text = selectFileDialog.FileName;
		}
	}
}

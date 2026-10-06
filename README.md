# 線形回帰分析アプリケーション

このリポジトリは販売データをマージして回帰分析を行うデスクトップ（WinForms）アプリケーションのソースです。

## 現在のプロジェクト構成（主要）

- RegressionAnalysis (WinFormsアプリケーション)
  - エントリポイント。UI（RegressionAnalysisForm）を提供し、DataMerger を利用して分析用データを取得・表示・分析する。
- SalesAnalysisSource (クラスライブラリ)
  - CSV / DB から販売・気象・単位データを読み込み、マージした DataTable を生成するマージャー群を提供。
  - 主なクラス: SalesDataMerger (抽象), CsvSalesDataMerger, MySqlSalesDataMerger
- RegressionAnalysis.Common (クラスライブラリ)
  - 共通ユーティリティと基底クラス群を提供。
  - 主なクラス: DataMerger（抽象基底）, DataTableHelpers, ConfigurationHelper

## 実装されている主要クラス（簡易版）

- RegressionAnalysis.Common.DataMerger
  - データ結合の抽象基底。GetMergedDataTable()（例外処理付き）、Filters 管理、CSV 用フィルタ述語生成等を提供。
- SalesAnalysisSource.SalesDataMerger
  - DataMerger を継承する抽象基底。マージ後の列名一覧を一元管理する（GetMergedDataTableColumnNames を実装）。
- SalesAnalysisSource.CsvSalesDataMerger
  - CSVの3ファイル（sales, weather, units）を読み込み LINQ で結合し、マージ済み DataTable を返す。CSV 側の元列名を統一列名へマップする。
- SalesAnalysisSource.MySqlSalesDataMerger
  - MySQL の 3テーブルを結合して DataTable を取得する。DB 側にプッシュダウンするための MergedColumnToSqlMapping と BuildSqlWhereClause を提供。
- RegressionAnalysis.RegressionAnalysisForm
  - ユーザー操作で DataMerger を受け取り分析用データのフィルタ設定・取得・表示を行う主要 UI。

## 設定ファイル

appsettings.Development.json を用いて接続文字列等を環境ごとに管理します。例:

```json
{
  "ConnectionStrings": {
    "MyDbConnection": "Server=localhost;Port=3306;Database=salesdb;User Id=root;Password=;Connection Timeout=300;"
  }
}
```

## 注意点
- DB テーブル名や列名を SQL に直接埋め込む箇所があります。外部入力を直接埋め込まないよう注意してください（実運用ではエスケープ/検証を追加すること）。

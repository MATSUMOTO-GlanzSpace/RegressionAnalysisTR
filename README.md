# 線形回帰分析アプリケーション

このリポジトリは販売データをマージして回帰分析を行うデスクトップ（WinForms）アプリケーションのソースです。

## 現在のプロジェクト構成（主要）

- RegressionAnalysis (WinFormsアプリケーション)
  - エントリポイント。UI（RegressionAnalysisForm）を提供し、DataImporterFacade / DataMerger を利用して分析用データを取得・表示・分析する。
- HousingAnalysisSource (クラスライブラリ)
  - CSV から住宅価格データを読み込み、マージした DataTable を生成するマージャー群を提供。
  - 主なクラス: HousingDataMerger (抽象), CsvHousingDataMerger, 現在はMySqlHousingDataMergerは未実装
- SalesAnalysisSource (クラスライブラリ)
  - CSV / DB から販売・気象・単位データを読み込み、マージした DataTable を生成するマージャー群を提供。
  - 主なクラス: SalesDataMerger (抽象), CsvSalesDataMerger, DbSalesDataMerger
- RegressionAnalysis.Common (クラスライブラリ)
  - 共通ユーティリティと基底クラス群を提供。
  - 主なクラス: DataImporter（1 CSV / 1 DB テーブル用基底）, CSVImporter, DBImporter, DataImporterFacade, IMergeStrategy, DataMerger（マージ用基底）, DataTableHelpers, ConfigurationHelper

## 現在のクラス構成

```text
DataImporter
├── CSVImporter
├── DBImporter
└── DataImporterFacade

IMergeStrategy
└── DataMerger
    └── SalesDataMerger
        ├── CsvSalesDataMerger
        └── DbSalesDataMerger
```

ポイント:

- `DataImporter` は 1 ソースの読み込みを担当する抽象基底です。
- `CSVImporter` / `DBImporter` は、各々単一ソースの読込を実装します。
- `DataImporterFacade` は `DataImporter` を継承し、内部で `IMergeStrategy` と複数の `IImportSource` をまとめて扱うラッパーです。
- `IMergeStrategy` はマージ処理の契約を定義し、`DataMerger` がその共通実装です。
- `SalesDataMerger` は `DataMerger` を継承し、売上ドメイン固有の統一列名とマージロジックを定義します。

## フィルタ API

- `DataImporter` / `DataMerger` のフィルタは、文字列ベースの旧仕様ヘルパーではなく `FilterOperator` を使う型安全な API を使用します。
- 例: `DataImporter.AddFilterCondition("", "年", FilterOperator.Equals, "2024")`
- この方式により、フィルタ条件の演算子が明示的になり、SQL / LINQ の両方で同じ意味で評価されます。

## 実装されている主要クラス（簡易版）

- 【回帰分析アプリケーション】RegressionAnalysis (WinFormsアプリケーション)
    - RegressionAnalysisForm
      - ユーザー操作で DataImporterFacade / DataMerger を受け取り分析用データのフィルタ設定・取得・表示を行う主要 UI。
    - Program
      - アプリケーションのエントリポイント。RegressionAnalysisForm を起動する。
- 【住宅価格データ】HousingAnalysisSource (クラスライブラリ)
    - HousingDataMerger
      - DataMerger を継承する抽象基底。マージ後の列名一覧を一元管理する（GetMergedColumnNames を実装）。
    - CsvHousingDataMerger
      - CSVファイル（housing）を `CSVImporter` で取得し、マージ済み DataTable を返す。CSV 側の元列名は直接参照し、最終的な統一列名は DataMerger 側で扱う。
- 【販売データ】SalesAnalysisSource (クラスライブラリ)
    - SalesDataMerger
      - DataMerger を継承する抽象基底。マージ後の列名と結合仕様を定義する。
    - CsvSalesDataMerger
      - 各 CSV を `CSVImporter` で読み込み、LINQ で結合し、最終 DataTable を返す。
    - DbSalesDataMerger
      - 各 DB テーブルを `DBImporter` で読み込み、統合された DataTable を取得する。
- 【共通ライブラリ】RegressionAnalysis.Common (クラスライブラリ)
    - DataImporter
      - 1 CSV / 1 DB テーブルの取得、元列名参照、ローカルフィルタを担う基底クラス。
    - CSVImporter / DBImporter
      - `GetTable()` と `GetTableColumnNames()` を実装し、単一ソースを DataTable として扱う。
    - DataImporterFacade
      - 複数ソースと `IMergeStrategy` を結びつけ、UI からは `DataImporter` として扱えるようにするファサード。 
    - IMergeStrategy
      - 複数ソースのマージ処理契約を定義するインターフェイス。
    - DataMerger
      - `IMergeStrategy` を実装し、複数ソースの join / merge / unified columns / final filters を担当する。
    - DataTableHelpers
      - DataTable の列名変更、列の追加、列の削除、列の型変換、DataTable のコピー等のユーティリティを提供。
    - ConfigurationHelper
      - appsettings.*.json から設定値を取得するユーティリティを提供。
    - LinearRegressionAnalysis
      - 線形回帰分析の計算を行うユーティリティ。最小二乗法で回帰係数を求める。

## 設定ファイル

- 実行環境名は `DOTNET_ENVIRONMENT` で切り替えます。
  - Visual Studio でのデバッグ時は `RegressionAnalysis/Properties/launchSettings.json` で設定します。
  - 未設定の場合は `Development` として扱われます。
- 設定ファイルは `RegressionAnalysis.Common/ConfigurationHelper.cs` で次の順に読み込まれます。
  1. `appsettings.json`
  2. `appsettings.{DOTNET_ENVIRONMENT}.json`
  3. 環境変数
- そのため、共通設定は `appsettings.json` に、環境ごとの差分は `appsettings.Development.json` や `appsettings.Production.json` に記述します。

`launchSettings.json` の例:

```json
{
  "profiles": {
    "RegressionAnalysis": {
      "commandName": "Project",
      "environmentVariables": {
        "DOTNET_ENVIRONMENT": "Development"
      }
    }
  }
}
```
`appsettings.Development.json` の例:（開発用に準備してください）

```json
{
  "ConnectionStrings": {
    "MySQLSales": "Server={Server};Port=3306;Database={DatabaseName};User Id={UserId};Password=******;Connection Timeout=300;",
    "MySQLHousing": "Server={Server};Port=3306;Database={DatabaseName};User Id={UserId};Password=******;Connection Timeout=300;"
  }
}
```

接続文字列は `ConfigurationHelper.GetConnectionString()` から参照され、現在は `RegressionAnalysis/Program.cs` で読み込んでいます。

## 注意点
- DB テーブル名や列名を SQL に直接埋め込む箇所があります。外部入力を直接埋め込まないよう注意してください（実運用ではエスケープ/検証を追加すること）。

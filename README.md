# 線形回帰分析アプリケーション

このリポジトリは販売データをマージして回帰分析を行うデスクトップ（WinForms）アプリケーションのソースです。

## 現在のプロジェクト構成（主要）

- RegressionAnalysis (WinFormsアプリケーション)
  - エントリポイント。UI（RegressionAnalysisForm）を提供し、DataMerger を利用して分析用データを取得・表示・分析する。
- HousingAnalysisSource (クラスライブラリ)
  - CSV から住宅価格データを読み込み、マージした DataTable を生成するマージャー群を提供。
  - 主なクラス: HousingDataMerger (抽象), CsvHousingDataMerger, 現在はMySqlHousingDataMergerは未実装
- SalesAnalysisSource (クラスライブラリ)
  - CSV / DB から販売・気象・単位データを読み込み、マージした DataTable を生成するマージャー群を提供。
  - 主なクラス: SalesDataMerger (抽象), CsvSalesDataMerger, MySqlSalesDataMerger
- RegressionAnalysis.Common (クラスライブラリ)
  - 共通ユーティリティと基底クラス群を提供。
  - 主なクラス: DataMerger（抽象基底）, DataTableHelpers, ConfigurationHelper

## 実装されている主要クラス（簡易版）

- 【回帰分析アプリケーション】RegressionAnalysis (WinFormsアプリケーション)
    - RegressionAnalysisForm
      - ユーザー操作で DataMerger を受け取り分析用データのフィルタ設定・取得・表示を行う主要 UI。
    - Program
      - アプリケーションのエントリポイント。RegressionAnalysisForm を起動する。
- 【住宅価格データ】HousingAnalysisSource (クラスライブラリ)
    - HousingDataMerger
      - DataMerger を継承する抽象基底。マージ後の列名一覧を一元管理する（GetMergedDataTableColumnNames を実装）。
    - CsvHousingDataMerger
      - CSVファイル（housing）を読み込み LINQ で結合し、マージ済み DataTable を返す。CSV 側の元列名を統一列名へマップする。
- 【販売データ】SalesAnalysisSource (クラスライブラリ)
    - SalesDataMerger
      - DataMerger を継承する抽象基底。マージ後の列名一覧を一元管理する（GetMergedDataTableColumnNames を実装）。
    - CsvSalesDataMerger
      - CSVの3ファイル（sales, weather, units）を読み込み LINQ で結合し、マージ済み DataTable を返す。CSV 側の元列名を統一列名へマップする。
    - DbSalesDataMerger
      - DB（MySQL）から販売・気象・単位データを読み込み LINQ で結合し、マージ済み DataTable を返す。DB 側の元列名を統一列名へマップする。
- 【共通ライブラリ】RegressionAnalysis.Common (クラスライブラリ)
    - DataMerger
      - データ結合の抽象基底。GetMergedDataTable()（例外処理付き）、Filters 管理、CSV 用フィルタ述語生成等を提供。
    - DbDiagnostics
      - DB 接続の診断ユーティリティ。接続文字列から DB に接続できるかを確認する。
    - MySqlDiagnostics
        - MySQL 接続の診断ユーティリティ。接続文字列から MySQL に接続できるかを確認する。
    - MySQLConnnectionFactory
      - MySQL 接続を生成するユーティリティ。接続文字列から MySqlConnection を生成する。
    - DataTableHelpers
      - DataTable の列名変更、列の追加、列の削除、列の型変換、DataTable のコピー等のユーティリティを提供。
    - DbSchemaHelper
      - DB スキーマ情報を取得するユーティリティ。テーブル名一覧、列名一覧、列の型情報を取得する。
    - ConfigurationHelper
      - appsettings.*.json から設定値を取得するユーティリティを提供。
    - LinearRegressionAnalysis
      - 線形回帰分析の計算を行うユーティリティ。最小二乗法で回帰係数を求める。
 
 ## 設定ファイル

appsettings.Development.json を用いて接続文字列等を環境ごとに管理します。例:

```json
{
  "ConnectionStrings": {
    "MySQLSales": "Server=localhost;Port=3306;Database=salesdb;User Id=root;Password=;Connection Timeout=300;"
  }
}
```

## 注意点

- DB テーブル名や列名を SQL に直接埋め込む箇所があります。外部入力を直接埋め込まないよう注意してください（実運用ではエスケープ/検証を追加すること）。

## 公開リポジトリ自動同期ルール

連携元リポジトリ(private RegressionAnalysis)管理者向け情報です。

- このリポジトリの for_training ブランチへの push を検知して、公開リポジトリ `MATSUMOTO-GlanzSpace/RegressionAnalysisTR` の master ブランチへ自動ミラーリングする GitHub Actions ワークフローを追加しています。
- workflow設定は.gitignoreで除外されており、公開リポジトリには push されません。
- 必要な Secret:
  - `PUBLIC_REPO_PAT` — 公開リポジトリへ push できる personal access token をリポジトリの Settings → Secrets and variables → Actions に登録してください。
- ワークフローの場所: `.github/workflows/push_to_public.yml`。
- 挙動: for_training に push されると該当ワークフローが起動し、公開リポジトリの master を強制更新します（force push）。履歴上書きを避けたい場合はワークフローの `--force` を削除して手動マージに変更してください。

---

## 履修生向け: 初期セットアップから演習まで

### 1. 初期 clone（演習開始時）

```bash
# 公開リポジトリから clone
git clone https://github.com/MATSUMOTO-GlanzSpace/RegressionAnalysisTR.git
cd RegressionAnalysisTR

# master ブランチで演習を開始
# （デフォルトで master がチェックアウトされています）
```

### 2. 教材の最新版を取得・merge（演習中、教材が更新された場合）

```bash
# 最新版を fetch
git fetch origin master

# 現在のローカル変更を確認
git status

# master の最新版を現在のブランチにマージ
git merge origin/master
```

> **conflict が発生した場合**:
> ```bash
> # conflict を手動で解決（エディタで編集）
> # 解決後、add/commit
> git add <conflict が発生したファイル>
> git commit -m "Merge latest master"
> ```


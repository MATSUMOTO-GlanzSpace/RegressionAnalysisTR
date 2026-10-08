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

## 公開リポジトリ自動同期ルール

連携元リポジトリ(private RegressionAnalysis)管理者向け情報です。

- このリポジトリの for_training ブランチへの push を検知して、公開リポジトリ `MATSUMOTO-GlanzSpace/RegressionAnalysisTR` の master ブランチへ自動ミラーリングする GitHub Actions ワークフローを追加しています。
- workflow設定は.gitignoreで除外されており、公開リポジトリには push されません。
- 必要な Secret:
  - `PUBLIC_REPO_PAT` — 公開リポジトリへ push できる personal access token をリポジトリの Settings → Secrets and variables → Actions に登録してください。
- ワークフローの場所: `.github/workflows/push_to_public.yml`。
- 挙動: for_training に push されると該当ワークフローが起動し、公開リポジトリの master を強制更新します（force push）。履歴上書きを避けたい場合はワークフローの `--force` を削除して手動マージに変更してください。

---

## 管理者向け: 教材更新・配布フロー

### 1. 非公開 master での修正（管理者のみ）

```bash
# master にチェックアウト（完成版）
git checkout master

# 修正を実装・テスト
# ...修正コード...

# commit して push
git add .
git commit -m "Fix: <修正内容>"
git push origin master
```

### 2. 修正を演習版（for_training）に反映（管理者）

```bash
# for_training にチェックアウト
git checkout for_training

# master から修正のみを取り込む（merge）
git merge origin/master

# push すると、自動で公開 master に同期される
git push origin for_training
```

> **注意**: この時点で、以下が実行されます：
> - `.github/workflows` 配下は公開側から除外
> - CI ワークフロー（`ci.yml`）で build/test 実行
> - 自動同期完了後、履修生が pull 可能

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


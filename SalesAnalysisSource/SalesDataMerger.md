# SalesDataMerger 説明書

このドキュメントは、`DataMerger` 基底クラスの設計と、`SalesDataMerger` 派生体系における使用方法を説明するものです。

## 目的

- **`DataMerger` 基底クラス**：CSV・MySQL データソースに共通するマージロジックを集約
- **`SalesDataMerger` 中間抽象クラス**：Sales ドメイン固有のマージ後列名を一元管理
- **派生実装クラス**：データソース固有処理のみ実装

## クラス設計階層

DataMerger（基底クラス）
├── SalesDataMerger（中間：Sales領域固有） 
│   ├── CsvSalesDataMerger（複数/単一 CSV） 
│   └── MySqlSalesDataMerger（MySQL） 
└── その他の派生クラス

## 実装パターン

### パターン1：複数 CSV ファイル結合

```csharp
public class CsvSalesDataMerger( string salesCsvPath, string weatherCsvPath, string unitsCsvPath) : SalesDataMerger
{
    protected override DataTable GetMergedDataTableCore()
    {
        // CSV読み込み → LINQ JOIN → ApplyFiltersAndConvertToDataTable() 
    }

    protected override DataTable ConvertToDataTable<T>(IEnumerable<T> data)
    {
        // リフレクションで匿名型を統一列名の DataTable に変換
    }
}
```


### パターン2：単一 CSV ファイル

```csharp
public class CsvSalesDataMerger(string salesCsvPath) : SalesDataMerger
{
    protected override DataTable GetMergedDataTableCore()
    {
        // CSV読み込み → LINQ 投影（結合なし） → ApplyFiltersAndConvertToDataTable() 
    }

    protected override DataTable ConvertToDataTable<T>(IEnumerable<T> data)
    {
        // リフレクションで匿名型を統一列名の DataTable に変換
    }
}
```


### パターン3：MySQL テーブル結合

```csharp
public class MySqlSalesDataMerger( string salesTable, string weatherTable, string unitsTable) : SalesDataMerger
{
    public override Dictionary<string, string> MergedColumnToSqlMapping { get; } = new();

    protected override DbConnection GetDbConnection()
    {
        return MySqlConnectionFactory.CreateOpenConnection();
    }

    protected override DataTable GetMergedDataTableCore()
    {
        // フィルタから SQL WHERE句生成 → SQL実行 → ExecuteMergedSqlQuery()
    }

    protected override DataTable ConvertToDataTable<T>(IEnumerable<T> data)
    {
        throw new NotSupportedException("...");
    }
}
```


## テンプレートメソッドのフロー

### CSV（CsvSalesDataMerger）

CSV読み込み → LINQ(JOIN or 投影) → ApplyFiltersAndConvertToDataTable() → Filters評価（述語） → ConvertToDataTable<T>（リフレクション） → DataTable返却


### MySQL（MySqlSalesDataMerger）

Filters → SQL WHERE句生成 → ExecuteMergedSqlQuery() → SQL実行 → DbDataAdapter.Fill() → DataTable返却


## 統一マージ後列名

部門、大分類、中分類、品種、年、月、売上、平均気温、最高気温、最低気温、降水量合計、日照時間、単位

## CSV と MySQL の差分

| 側面 | CSV | MySQL |
|------|-----|-------|
| フィルタ評価 | LINQ述語（メモリ） | SQL WHERE句（DB側） |
| 列名マッピング | リフレクション（ConvertToDataTable） | AS エイリアス（SQL SELECT） |
| MergedColumnToSqlMapping | 不要 | 必須 |

## 新しい DataMerger（例：NewCsvDataMerger / NewMySqlDataMerger）追加手順

### A. マージあり（複数ファイル/複数テーブル）

1. クラスを追加（`SalesDataMerger` 継承）
   - CSV: `NewCsvDataMerger(fileA, fileB, ...)`
   - MySQL: `NewMySqlDataMerger(tableA, tableB, ...)`
2. マージ仕様を定義
   - `GetJoinSpecification()` で JOIN キーを定義
3. `GetMergedDataTableCore()` を実装
   - CSV: 読み込み → JOIN（LINQ）→ `ApplyFilters(...)` → DataTable 化
   - MySQL: JOIN 句生成 → `BuildSqlWhereClause(...)` で WHERE 生成 → SQL 実行
4. フィルタ処理を必ず有効化
   - CSV: `ApplyFilters(...)` を通す
   - MySQL: `BuildSqlWhereClause(...)` の戻り値を SQL に連結し、生成パラメータを `ExecuteMergedSqlQuery(...)` へ渡す
5. 列名を統一
   - `GetMergedDataTableColumnNames()` の列順と、CSV 投影 / SQL `AS` エイリアスを一致させる

### B. マージなし（単一ファイル/単一テーブル）

1. クラスを追加（`SalesDataMerger` 継承）
   - CSV: `NewCsvDataMerger(filePath)`
   - MySQL: `NewMySqlDataMerger(tableName)`
2. `GetMergedDataTableCore()` を実装
   - CSV: 読み込み → 必要列へ投影 → `ApplyFilters(...)` → DataTable 化
   - MySQL: 単一テーブル SELECT を構築 → `BuildSqlWhereClause(...)` を適用
3. フィルタ処理を必ず有効化
   - マージなしでも、CSV は述語フィルタ、MySQL は WHERE + パラメータを必須で適用
4. 列名を統一
   - `GetMergedDataTableColumnNames()` の列順と、投影結果/SQL エイリアスを一致させる

### 実装時の注意（共通）

- MySQL では `BuildSqlWhereClause(...)` で生成されたプレースホルダ名（例: `@p0`）と、実際に作成する `DbParameter.ParameterName` を一致させる。
- `BuildSqlJoinClauses(...)` が返す JOIN 句に余計な記号（`{}` など）を混入させない。
- 追加後は UI 側で列コンボ（目的変数/説明変数/フィルタ列）に列名が出ることを確認する。

## 派生実装のチェックリスト

- [ ] `SalesDataMerger` を継承
- [ ] `GetMergedDataTableCore()` を実装
- [ ] `GetMergedDataTableColumnNames()` の列定義と実データ列を一致
- [ ] `ConvertToDataTable<T>()` を実装（必要な場合）
- [ ] DB の場合：`GetDbConnection()`、`MergedColumnToSqlMapping` を実装
- [ ] フィルタ処理（CSV: `ApplyFilters` / MySQL: `BuildSqlWhereClause`）を有効化

## 設計上の注意点

- **MergedColumnToSqlMapping**：DB固有のマッピングのため、MySQL実装に配置。他のDB対応時に基底移行を検討。
- **CSV列名バリデーション**：現状は期待値と異なる場合の対応（エラー・ログ・空列埋め）を踏襲。運用で問題があれば明示的バリデーション追加を推奨。
- **Null許可警告**：`factory.CreateDataAdapter()` など外部API戻り値が null許可型の場合、必要に応じて `#pragma warning disable` で抑制。

## 変更履歴

- **2026-10-09**：新規 DataMerger 追加手順を追記（マージあり/なしを分離、両方でフィルタ適用必須を明記）。
- **2026-10-09**：SalesDataMerger説明書を再作成。テンプレートメソッド設計、CSV（複数・単一）、MySQLの実装例を記載。




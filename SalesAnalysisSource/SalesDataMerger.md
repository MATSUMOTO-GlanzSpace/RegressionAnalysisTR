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

## 派生実装のチェックリスト

- [ ] `SalesDataMerger` を継承
- [ ] `GetMergedDataTableCore()` を実装
- [ ] `ConvertToDataTable<T>()` を実装
- [ ] DB の場合：`GetDbConnection()`、`MergedColumnToSqlMapping` を実装
- [ ] フィルタ・SQL WHERE 句生成は基底メソッドを利用

## 設計上の注意点

- **MergedColumnToSqlMapping**：DB固有のマッピングのため、MySQL実装に配置。他のDB対応時に基底移行を検討。
- **CSV列名バリデーション**：現状は期待値と異なる場合の対応（エラー・ログ・空列埋め）を踏襲。運用で問題があれば明示的バリデーション追加を推奨。
- **Null許可警告**：`factory.CreateDataAdapter()` など外部API戻り値が null許可型の場合、必要に応じて `#pragma warning disable` で抑制。

## 変更履歴

- **2026-10-09**：SalesDataMerger説明書を再作成。テンプレートメソッド設計、CSV（複数・単一）、MySQLの実装例を記載。




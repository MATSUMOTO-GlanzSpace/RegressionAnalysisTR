# SalesDataMerger 説明書

このドキュメントは、現在の `SalesDataMerger` 実装と、DB 接続時に使う `DBDialect` の設定方法を整理して説明するものです。

## 目的

- `DataMerger`：CSV / DB 対応の共通マージ処理を提供する基底クラス
- `SalesDataMerger`：Sales ドメイン固有の統一列名と JOIN 定義を持つ中間抽象クラス
- `CsvSalesDataMerger`：CSV 3 ファイルを LINQ JOIN で結合する実装
- `DbSalesDataMerger`：DB の 3 テーブルを結合して DataTable を取得する実装
- `DBDialect`：DB ごとの識別子クォートやパラメータ生成など SQL 生成時に必要な方言情報を扱うクラス

## 現在のクラス構成

```text
DataImporter
├── CSVImporter
├── DBImporter
└── (1 CSV/1 テーブル単位の一般的な参照機能)

DataMerger : DataImporter
└── SalesDataMerger
    ├── CsvSalesDataMerger
    └── DbSalesDataMerger

DBDialect
├── MySqlDialect
└── (将来の他 DB 方言を追加可能)
```

## 実装の現状

### 1. SalesDataMerger の基本構造

現在の `SalesDataMerger` は `DataMerger` を継承し、DB 方言と共通 import 機能を使えるようにしています。`DataImporter` により単一ソースの DataTable 参照が共通化され、`DataMerger` では結合後の統一列名とデータ依存ロジックを扱います。

```csharp
public abstract class SalesDataMerger : DataMerger
{
    public SalesDataMerger(DBDialect? dbDialect = null) : base(dbDialect)
    {
    }

    public override string[] GetMergedDataTableColumnNames() =>
    [
        "部門", "大分類", "中分類", "品種", "年", "月", "売上",
        "平均気温", "最高気温", "最低気温", "降水量合計", "日照時間", "単位"
    ];
}
```

ポイント:

- `DataMerger` のコンストラクタに `DBDialect? dbDialect = null` を渡せる
- `SalesDataMerger` 自体も `DBDialect` を保持できる
- `DbSalesDataMerger` は `dialect ?? new MySqlDialect()` を使ってデフォルトの方言を設定する

### 2. CSV 実装

CSV 側は `CsvSalesDataMerger` が `CSVImporter` で 3 つの CSV を個別に `DataTable` として取得し、その後 LINQ の `join` で `sales` / `weather` / `units` を結合しています。

```csharp
public class CsvSalesDataMerger(string csvSalesPath, string csvWeatherPath, string csvUnitsPath) : SalesDataMerger
{
    protected override DataTable GetMergedDataTableCore()
    {
        var salesImporter = new CSVImporter(csvSalesPath);
        var weatherImporter = new CSVImporter(csvWeatherPath);
        var unitsImporter = new CSVImporter(csvUnitsPath);

        var salesTable = salesImporter.GetTable();
        var weatherTable = weatherImporter.GetTable();
        var unitsTable = unitsImporter.GetTable();

        var query = from s in salesTable.AsEnumerable()
                    join w in weatherTable.AsEnumerable()
                        on new { year = s.Field<string>("年"), month = s.Field<string>("月") }
                        equals new { year = w.Field<string>("年"), month = w.Field<string>("月") }
                    join u in unitsTable.AsEnumerable()
                        on s.Field<string>("品種") equals u.Field<string>("品種")
                    select new
                    {
                        部門 = s.Field<string>("部門"),
                        大分類 = s.Field<string>("大分類"),
                        中分類 = s.Field<string>("中分類"),
                        品種 = s.Field<string>("品種"),
                        年 = s.Field<string>("年"),
                        月 = s.Field<string>("月"),
                        売上 = s.Field<string>("売上"),
                        平均気温 = w.Field<string>("平均気温(℃)"),
                        最高気温 = w.Field<string>("最高気温(℃)"),
                        最低気温 = w.Field<string>("最低気温(℃)"),
                        降水量合計 = w.Field<string>("降水量の合計(mm)"),
                        日照時間 = w.Field<string>("日照時間(時間)"),
                        単位 = u.Field<string>("単位")
                    };

        var filteredQuery = ApplyFilters(query);
        return ConvertToDataTable(filteredQuery);
    }
}
```

CSV 版では `CSVImporter.GetTable()` が 1 CSV の列名ベースアクセスを提供し、`DataMerger` の join では統一列名の最終結合が行われます。

### 3. DB 実装

DB 版は `DbSalesDataMerger` で、`DBImporter` を使って各テーブルを `DataTable` として取得し、その後で join を行います。`MergedColumnToSqlMapping` は最終出力の統一列名を DB 実際の列へ変換するために使います。

```csharp
public class DbSalesDataMerger(
    string salesTable,
    string weatherTable,
    string unitsTable,
    DBDialect? dialect = null,
    Func<DbConnection>? connectionFactory = null) : SalesDataMerger(dialect ?? new MySqlDialect())
{
    public override Dictionary<string, string> MergedColumnToSqlMapping { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["部門"] = "s.department",
        ["大分類"] = "s.primary_item",
        ["中分類"] = "s.secondary_item",
        ["品種"] = "s.variety",
        ["年"] = "s.year",
        ["月"] = "s.month",
        ["売上"] = "s.sales",
        ["平均気温"] = "w.average_temperature",
        ["最高気温"] = "w.maximum_temperature",
        ["最低気温"] = "w.lowest_temperature",
        ["降水量合計"] = "w.precipitation",
        ["日照時間"] = "w.sunshine_hours",
        ["単位"] = "u.unit"
    };
}
```

ここで重要なのは、`DbSalesDataMerger` が `SalesDataMerger(dialect ?? new MySqlDialect())` として親クラスへ `DBDialect` を渡している点です。これにより `DataMerger.RequiredDBDialect` が利用可能になります。

### 4. Filters と MergedColumnToSqlMapping の実利用例

責務の分離は次のように使い分けます。

- `DataImporter` は 1 CSV / 1 テーブルの読み取りとローカルフィルタを担当する
- `DataMerger` は複数ソースの結合・統一列名・最終的なマージ結果を担当する

```csharp
var salesImporter = new DBImporter("sales", new MySqlDialect());
salesImporter.Filters.Add(new FilterCondition("", "year", FilterOperator.Equals, "2024"));
var salesTable = salesImporter.GetTable();
```

上記の `salesImporter.Filters` は `sales` テーブルに対するローカルな絞り込みです。`GetTableColumnNames()` は `"year"` などの元データ側の列名をそのまま返します。

```csharp
var merger = new DbSalesDataMerger("sales", "weather", "units", new MySqlDialect());
merger.Filters.Add(new FilterCondition("", "売上", FilterOperator.Equals, "1500"));

merger.MergedColumnToSqlMapping["売上"] = "s.sales";
var merged = merger.GetMergedDataTable();
```

この例では `merger.Filters` は結合後の結果に対する最終フィルタです。`MergedColumnToSqlMapping` は `売上` などの統一列名を `s.sales` のような SQL の実列へ変換し、DB 側の `WHERE` 生成で利用されます。

`SalesDataMerger / HousingDataMerger` の実装側では、各ソースを `CSVImporter` / `DBImporter` から `DataTable` として取得した後、Merger 側で join / unify / final filter を行います。

## DBDialect の設定方法

### 1. 使う方言を明示して作成する

現在の実装では MySQL を使う場合は `new MySqlDialect()` を指定します。

```csharp
var merger = new SalesDataMerger.DbSalesDataMerger(
    salesTable: "sales",
    weatherTable: "weather",
    unitsTable: "units",
    dialect: new MySqlDialect());
```

### 2. 省略時の既定動作

`dialect` を省略した場合は、`DbSalesDataMerger` のコンストラクタで自動的に `new MySqlDialect()` が使われます。

```csharp
var merger = new SalesDataMerger.DbSalesDataMerger(
    "sales",
    "weather",
    "units");
```

内部的には次のような扱いです。

```csharp
: SalesDataMerger(dialect ?? new MySqlDialect())
```

つまり、MySQL 実行時の既定は `MySqlDialect` です。

### 3. DBDialect の役割

`DBDialect` は SQL 生成時に必要な DB ごとの差分を吸収します。

現在の実装では、主に次の 3 点を担当します。

- 識別子のクォート
  - `QuoteIdentifier("sales")` → `` `sales` ``
- パラメータ生成
  - `CreateParameter(name, value)` -> `MySqlParameter`
- エイリアスの正規化
  - `NormalizeAlias()` は `ToLowerInvariant()` によりカルチャ依存を避ける

```csharp
public abstract class DBDialect
{
    public string QuoteIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier) || !SafeIdentifierRegex.IsMatch(identifier))
            throw new InvalidOperationException($"不正な識別子です: {identifier}");

        return $"{IdentifierQuoteStart}{identifier}{IdentifierQuoteEnd}";
    }

    public abstract DbParameter CreateParameter(string name, object value);
    public virtual string NormalizeAlias(string alias) => alias.ToLowerInvariant();
}
```

MySQL 版は次のように実装されています。

```csharp
public sealed class MySqlDialect : DBDialect
{
    protected override string IdentifierQuoteStart => "`";
    protected override string IdentifierQuoteEnd => "`";

    public override DbParameter CreateParameter(string name, object value)
        => new MySqlParameter(name, value);
}
```

### 4. どこで使われているか

`DataMerger` 側で `RequiredDBDialect` を持っており、SQL 生成で必要な識別子やパラメータを通します。

```csharp
protected DBDialect RequiredDBDialect =>
    DBDialect ?? throw new InvalidOperationException("DB方言が設定されていません。");
```

そのため、DB 実装では `RequiredDBDialect.QuoteIdentifier(...)` や `RequiredDBDialect.CreateParameter(...)` を使って SQL を組み立てます。

## 統一マージ後列名

`SalesDataMerger` はマージ後の列名を次の 13 列で固定します。

- 部門
- 大分類
- 中分類
- 品種
- 年
- 月
- 売上
- 平均気温
- 最高気温
- 最低気温
- 降水量合計
- 日照時間
- 単位

CSV 版と DB 版のどちらでも、最終的にこの列順と列名になるように実装されています。

## CSV と DB の違い

| 観点 | CSV 実装 | DB 実装 |
|------|----------|---------|
| JOIN | LINQ `join` | SQL `JOIN` 句生成 |
| フィルタ処理 | `ApplyFilters(query)` | `BuildSqlWhereClause(...)` |
| 列名変換 | `ConvertToDataTable()` | SQL `AS` エイリアス |
| DB 方言 | 不要 | `DBDialect` が必須 |

## 新しいマージャーを追加する際の方針

### A. CSV 追加時

1. `SalesDataMerger` を継承する
2. `GetMergedDataTableCore()` を実装
3. LINQ で結合・投影を行う
4. `ApplyFilters(...)` を通す
5. `GetMergedDataTableColumnNames()` の列順と一致するように DataTable を生成する

### B. DB 追加時

1. `SalesDataMerger` を継承する
2. コンストラクタで `DBDialect` を渡す
3. `GetJoinSpecification()` で JOIN 条件を定義する
4. `BuildSqlWhereClause(...)` を使って SQL の `WHERE` を生成する
5. `RequiredDBDialect.QuoteIdentifier(...)` と `CreateParameter(...)` を使って SQL を安全に組み立てる
6. `MergedColumnToSqlMapping` を定義して `AS` 列名を統一する

## 実装時の注意

- `DbSalesDataMerger` は `dialect` 未指定時に `MySqlDialect` を使うため、MySQL 以外を使う場合は明示的に指定する
- `BuildSqlWhereClause(...)` のプレースホルダ名と `DbParameter.ParameterName` は一致させる必要がある
- `DBDialect` を設定し忘れると `RequiredDBDialect` で `InvalidOperationException` になる
- `QuoteIdentifier` は `^[A-Za-z_][A-Za-z0-9_]*$` 形式の安全な識別子のみ許可する

## 変更履歴

- **2026-10-17**：現状の `SalesDataMerger` 実装に合わせて更新。`DBDialect` の設定方法と用途を追記。
- **2026-10-09**：新規 DataMerger 追加手順と CSV/DB の違いを整理。


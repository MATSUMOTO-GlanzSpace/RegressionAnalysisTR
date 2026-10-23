# SalesDataMerger の現在構成

このドキュメントは、現在の `SalesDataMerger` と関連する `DataImporter` / `DataMerger` の構成を、実装に合わせて整理したものです。

## 1. 現在の構成

```text
DataImporter
├── CSVImporter
├── DBImporter
└── DataImporterFacade
    └── IMergeStrategy
        └── DataMerger
            └── SalesDataMerger
                ├── CsvSalesDataMerger
                └── DbSalesDataMerger
```

重要な違い:

- `DataImporter` は「1 ソースの読み込み」専用の抽象です。
- `DataMerger` は「複数ソースの統合」を行うマージ戦略です。
- `DataImporterFacade` は、戦略とソース辞書を受け取り、UI 側から `DataImporter` として扱えるようにします。
- `SalesDataMerger` は売上ドメインの統一列名と結合ロジックを持つ `DataMerger` 実装です。

## 2. 責務の分離

### DataImporter

`DataImporter` は単一ソースの読込責務を持ちます。

- CSV 1 ファイルの読込: `CSVImporter`
- DB 1 テーブルの読込: `DBImporter`
- 列名取得: `GetTableColumnNames()`
- ローカルフィルタ: `AddFilterCondition(...)`

```csharp
var salesImporter = new CSVImporter(path, "sales");
salesImporter.AddFilterCondition("", "年", FilterOperator.Equals, "2024");
var salesTable = salesImporter.GetTable();
```

### DataMerger

`DataMerger` は `IMergeStrategy` を実装し、複数ソースを結合して最終的な `DataTable` を返します。

```csharp
public abstract class DataMerger : IMergeStrategy
{
    public virtual DataTable Merge(IReadOnlyDictionary<string, IImportSource> sources) { ... }
    public virtual string[] GetMergedColumnNames() => Array.Empty<string>();
}
```

### DataImporterFacade

UI 側では `DataImporterFacade` を使って、複数ソース統合を 1 つの `DataImporter` として扱います。

```csharp
var sources = new Dictionary<string, IImportSource>
{
    ["sales"] = new CSVImporter(salesPath, "sales"),
    ["weather"] = new CSVImporter(weatherPath, "weather"),
    ["units"] = new CSVImporter(unitsPath, "units")
};

var strategy = new SalesDataMerger.CsvSalesDataMerger();
var importer = new DataImporterFacade(strategy, sources);
var merged = importer.GetTable();
```

この設計により、UI は「単一ソースのように見える importer」に対して、実際には `IMergeStrategy` を使った結合を実行しています。

## 3. SalesDataMerger の現在の実装

`SalesDataMerger` は `DataMerger` を継承し、売上データの統一列名を返します。

```csharp
public abstract class SalesDataMerger : DataMerger
{
    public override string[] GetMergedColumnNames() =>
    [
        "部門", "大分類", "中分類", "品種", "年", "月", "売上",
        "平均気温", "最高気温", "最低気温", "降水量合計", "日照時間", "単位"
    ];
}
```

## 4. CSV 実装

`CsvSalesDataMerger` は `sales` / `weather` / `units` の 3 つの `IImportSource` を読み込み、LINQ の `join` で 1 つの `DataTable` に結合します。

```csharp
var salesTable = salesSource.GetTable();
var weatherTable = weatherSource.GetTable();
var unitsTable = unitsSource.GetTable();

var query =
    from s in salesTable.AsEnumerable()
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
```

## 5. DB 実装

`DbSalesDataMerger` は `DBImporter` から各テーブルを読み込み、同じ統一列名でまとめて返します。

```csharp
var salesSource = RequireSource<IDbImportSource>(sources, "sales");
var weatherSource = RequireSource<IDbImportSource>(sources, "weather");
var unitsSource = RequireSource<IDbImportSource>(sources, "units");

var salesTable = salesSource.GetTable();
var weatherTable = weatherSource.GetTable();
var unitsTable = unitsSource.GetTable();
```

DB 側では `DBImporter` が `DBDialect` を持つため、SQL の生成や接続に必要な情報をソース側で管理します。`DataMerger` はその詳細を知る必要はありません。

## 6. 実際の生成コード

```csharp
var sources = new Dictionary<string, IImportSource>
{
    ["sales"] = new DBImporter("sales", new MySqlDialect(), null, "sales"),
    ["weather"] = new DBImporter("weather", new MySqlDialect(), null, "weather"),
    ["units"] = new DBImporter("units", new MySqlDialect(), null, "units")
};

var strategy = new SalesDataMerger.DbSalesDataMerger();
var importer = new DataImporterFacade(strategy, sources);
var table = importer.GetTable();
```

## 7. 統一列名

`SalesDataMerger` のマージ後列名は以下の 13 項目です。

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

CSV 版 / DB 版のどちらでも、最終的にこの列名と順序で返す設計です。

## 8. 変更履歴

- **2026-10-22**: 現在の `DataImporter` / `DataMerger` / `DataImporterFacade` の構成に合わせて再整理。
- **2026-10-17**: 旧構成の説明を整理し、CSV / DB の違いを補足。

# DataMerger クラス群 説明書

---

## DataMerger（抽象基底クラス）

- **役割**: データ結合処理の共通基底クラス。
- **特徴**:
  - エラーハンドリング（例外キャッチ＋ログ出力）を共通化。
  - `GetMergedDataTable()`：共通のデータ取得メソッド。内部で例外処理を行い、エラー時は空の`DataTable`を返す。
  - `LogError(Exception ex)`：仮想メソッド。標準出力にエラーメッセージを表示。必要に応じて派生クラスでオーバーライド可能。
  - `GetMergedDataTableCore()`：抽象メソッド。各派生クラスで具体的な結合処理を実装。

---

## CsvDataMerger（CSVファイル結合クラス）

- **役割**: 3つのCSVファイル（販売・気象・単位）を読み込み、結合し、`DataTable`で返す。
- **コンストラクタ引数**:
  - `salesCsvPath`：販売CSVファイルパス
  - `weatherCsvPath`：気象CSVファイルパス
  - `unitsCsvPath`：単位CSVファイルパス
- **主なメソッド**:
  - `GetMergedDataTableCore()`：
    - 3つのCSVファイルを読み込み、LINQで結合。
    - 結合結果を`DataTable`として返却。
  - `ReadCsv(string path)`：CSVファイルを`DataTable`として読み込む内部メソッド。

---

## MySqlDataMerger（MySQLテーブル結合クラス）

- **役割**: MySQLの3テーブル（sales, weather, units）を結合し、`DataTable`で返す。
- **コンストラクタ引数**:
  - `connectionString`：MySQL接続文字列
  - `salesTable`：販売テーブル名
  - `weatherTable`：気象テーブル名
  - `unitsTable`：単位テーブル名
- **主なメソッド**:
  - `GetMergedDataTableCore()`：
    - MySQLに接続し、3テーブルをSQLで結合。
    - 結合結果を`DataTable`として返却。

---

## 共通事項
- いずれのクラスも`GetMergedDataTable()`で結合済みデータを`DataTable`型で取得可能。
- エラー発生時は基底クラスの共通処理でログ出力し、空の`DataTable`を返却。
- GridView等へのバインドが容易。

---

## 利用例
```csharp
// CSVファイル結合
var csvMerger = new CsvDataMerger(salesPath, weatherPath, unitsPath);
DataTable csvResult = csvMerger.GetMergedDataTable();

// MySQLテーブル結合
var mysqlMerger = new MySqlDataMerger(connStr, "sales", "weather", "units");
DataTable dbResult = mysqlMerger.GetMergedDataTable();
```

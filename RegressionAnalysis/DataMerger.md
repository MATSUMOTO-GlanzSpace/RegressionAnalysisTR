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

## 使用するCSVファイル仕様

### 売上CSV (salesCsvPath)
| フィールド名 | 説明         |
|--------------|--------------|
| 部門         | 部門名       |
| 大分類       | 商品大分類   |
| 中分類       | 商品中分類   |
| 品種         | 商品品種     |
| 年           | 年           |
| 月           | 月           |
| 売上         | 売上数量     |

### 天気CSV (weatherCsvPath)
| フィールド名         | 説明             |
|----------------------|------------------|
| 年                   | 年               |
| 月                   | 月               |
| 平均気温(℃)         | 月平均気温       |
| 最高気温(℃)         | 月最高気温       |
| 最低気温(℃)         | 月最低気温       |
| 降水量の合計(mm)     | 月降水量合計     |
| 日照時間(時間)       | 月日照時間       |

### 単位CSV (unitsCsvPath)
| フィールド名 | 説明         |
|--------------|--------------|
| 品種         | 商品品種     |
| 単位         | 販売単位     |

---

## 使用するMySQLデータベーステーブル仕様

### 売上テーブル（salesTable）
| フィールド名        | 説明           |
|---------------------|----------------|
| id                  | id             |
| department          | 部門名         |
| primary_item        | 商品大分類     |
| secondary_item      | 商品中分類     |
| varety              | 商品品種       |
| year                | 年             |
| month               | 月             |
| sales               | 売上数量       |

### 天気テーブル（weatherTable）
| フィールド名        | 説明           |
|---------------------|----------------|
| id                  | id             |
| year                | 年             |
| month               | 月             |
| average_temperature | 月平均気温     |
| maximum_temperature | 月最高気温     |
| lowest_temperature  | 月最低気温     |
| precipitation       | 月降水量合計   |
| sunshine_hours      | 月日照時間     |

### 単位テーブル（unitsTable）
| フィールド名        | 説明           |
|---------------------|----------------|
| id                  | id             |
| varety              | 商品品種       |
| unit                | 販売単位       |

---

## 結合後データテーブルのフィールド名

| フィールド名         | 説明             |
|----------------------|------------------|
| 部門                 | 部門名           |
| 大分類               | 商品大分類       |
| 中分類               | 商品中分類       |
| 品種                 | 商品品種         |
| 年                   | 年               |
| 月                   | 月               |
| 売上                 | 売上数量         |
| 平均気温(℃)         | 月平均気温       |
| 最高気温(℃)         | 月最高気温       |
| 最低気温(℃)         | 月最低気温       |
| 降水量の合計(mm)／降水量合計 | 月降水量合計 |
| 日照時間(時間)／日照時間     | 月日照時間   |
| 単位                 | 販売単位         |

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

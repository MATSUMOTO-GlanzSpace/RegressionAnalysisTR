# SalesDataMerger リファクタリング概要

この文書は、DataMerger の継承構成を整理して SalesDataMerger を導入した変更点をまとめたものです。

目的
- DataMerger -> CsvSalesDataMerger / MySqlSalesDataMerger の直接継承構造に中間抽象クラス `SalesDataMerger` を導入し、CSV / DB に共通する振る舞いやマージ後列名を集約する。
- マージ後列名（UI/フィルタで使用する列名）を DB 側の表記に合わせて統一する。

概要
- SalesDataMerger (抽象クラス)
  - DataMerger を継承。
  - マージ後列名一覧を一元管理する `GetMergedDataTableColumnNames()` を実装する（派生クラスは通常オーバーライド不要）。
  - 統一列名は DB 側の表記（例: "平均気温" / "降水量合計" / "日照時間"）に合わせている。

- CsvSalesDataMerger
  - 基底を `SalesDataMerger` に変更。
  - CSV 側の元列名（例: "平均気温(℃)", "降水量の合計(mm)", "日照時間(時間)"）から値を取得し、基底で定義された統一列名へ詰めて DataTable を構築する。
  - Filters の評価は既存の匿名オブジェクト上での RPN 評価ロジックを継承している。

- MySqlSalesDataMerger
  - 基底を `SalesDataMerger` に変更。
  - DB プッシュダウン用の `MergedColumnToSqlMapping` は MySql 側でオーバーライドして提供する（例: "部門"->"s.department" 等）。
  - SQL の SELECT 句ではマージ後列名を AS エイリアスとして返すため、呼出し側の DataTable 列名と一致する。

統一したマージ後列名

1. 部門
2. 大分類
3. 中分類
4. 品種
5. 年
6. 月
7. 売上
8. 平均気温
9. 最高気温
10. 最低気温
11. 降水量合計
12. 日照時間
13. 単位

CSV と MySQL の差分と扱い
- CSV 元の列名は単位表記や括弧付きの単位が付くため、CsvSalesDataMerger は元列名から値を抽出して統一列名にマップする。UI とフィルタは統一列名のみを参照すれば良い。
- MySQL 側は SELECT ... AS で統一列名を返すため、DataMerger 側での追加変換は不要。

設計上の注意点・今後の検討
- `MergedColumnToSqlMapping` を基底へ移して共通定義する選択肢があるが、現状は MySQL 固有のマッピングのため MySqlSalesDataMerger に置くことを採択している。
- CsvSalesDataMerger の入力CSVで列名が期待値と異なる場合の挙動（ログ出力・エラー・空列埋め）は現状の実装を踏襲している。運用で問題があれば明示的なバリデーションを追加することを推奨する。

変更履歴
- 2026-10-06: SalesDataMerger を導入し、CsvSalesDataMerger / MySqlSalesDataMerger を基底継承するようリファクタリング。GetMergedDataTableColumnNames() を基底で統一実装。
# 線形回帰分析アプリケーション

## ソリューション構成

## アプリケーション設定ファイル

```json
RegressionAnalysis/appsettings.Development.json
{
  "__comment": "appsettings.*.jsonはGit管理対象には含まれません、手動で配置して下さい。* は環境変数 DOTNET_ENVIRONMENT を Properties/launchSettings.json にて指定します",
  "ConnectionStrings": {
    "__comment": "MyDbConnectionはMySQL(salesdb)用の接続情報です。ユーザー名・パスワードは環境に合わせて設定してください。",
    "MyDbConnection": "Server=localhost;Port=3306;Database=salesdb;User Id=root;Password=;Connection Timeout=300;"
  }
}
```

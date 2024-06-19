# DiscordMusicBot-dotNet

Discordの音楽bot 

Discord.netを使用しています。

こちらは開発終了しました

後継はこちらです [DiscordMusicBot.py](https://github.com/OneWalkDev/DiscordMusicBot.py)

## 概要 

Discordでダウンロードせずyoutubeの音楽を流すbotです。 

スラッシュコマンド版です

現在開発中です。 

## 使い方 

https://github.com/yurisi0212/DiscordMusicBot-dotNet/releases

リリースから最新バージョンをダウンロードしてください。

[NET6.0 Runtime](https://dotnet.microsoft.com/ja-jp/download/dotnet/6.0)が必要な可能性がありますので必要に応じてダウンロードしてください。

settings.iniのTokenにDiscordDeveloperPortalから取得したトークンを貼り付け、必要であれば他のオプションも変更してください。

settings.iniのGrobalがtrueの場合コマンドが登録されるまで時間がかかりますのでお待ち下さい。(discordの仕様です)

linuxで動かす場合はパッケージマネージャーでffmpeg、opus、libsodiumをダウンロードしてください。(合うものが見つからなかった...)

## TODO 

- [x] スラッシュコマンドの実装
- [x] 音楽の再生
- [x] スキップ
- [x] プレイリストの対応
- [x] 多サーバーの対応 
- [x] 検索
- [ ] 詳細な検索 
- [x] キューループ
- [x] 1曲ループ
- [x] シャッフル    

## Builds (Special Thanks)

Opus.dll Emzi0767

libsodium.dll Emzi0767

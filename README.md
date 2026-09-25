[![CI](https://github.com/zecl/FsBulletML-2/actions/workflows/pr.yml/badge.svg)](https://github.com/zecl/FsBulletML-2/actions/workflows/pr.yml)

# FsBulletML-2

BulletML（弾幕記述言語）の F# 実装。仕様は Kenta Cho (ABA Games) の
[BulletML](http://www.asahi-net.or.jp/~cs8k-cyu/bulletml/) ver 0.21。

## ディレクトリ

```
FsBulletML-2/
├── src/       エンジン、読み書き、フロント、言語サービス、型プロバイダ、Jev用ジェネレータ等
├── samples/   MonoGame / Unity / Godot / MagicOnion のサンプル
├── tests/     Unitテスト
├── bench/     ベンチマーク
├── tools/     ツール
└── license/   BulletML と白い弾幕くん
```

ソリューションは `FsBulletML2.slnx`。ライブラリ側だけは `FsBulletML2.Library.slnx`。
ライセンスは MIT（[LICENSE.md](LICENSE.md)）。

# FsBulletML2

A [BulletML](http://www.asahi-net.or.jp/~cs8k-cyu/bulletml/) engine written in F#.
It runs danmaku (bullet-hell) patterns frame by frame and leaves drawing, collision and
object pooling to your game framework: MonoGame, Unity, Godot or anything that can call .NET.

| Package | What it does |
|---|---|
| `FsBulletML2.Core` | The engine. Loads a pattern and steps one bullet per call |
| `FsBulletML2.Parser` | Reads patterns written in XML, SXML or FSB |
| `FsBulletML2.Front` | Shared front-end layer that drives many bullets per frame |
| `FsBulletML2.Dsl` | Computation expressions for writing patterns in F# |

`Core`, `Front` and `Dsl` target `net10.0` and `netstandard2.1` (Unity). `Parser` targets `net10.0`.

## Run a pattern

```fsharp
open FsBulletML2
open FsBulletML2.Domain

let xml = """<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <fire><direction type="absolute">0</direction><speed>2</speed><bullet/></fire>
    <wait>1</wait>
    <vanish/>
  </action>
</bulletml>"""

let rand () = 0.5f
let rank = 0.5f

// Load once per pattern
let script = Runner.load rand rank (readXmlString xml)
let mutable run = Runner.newRoot BulletType.Enemy script
let mutable pos = { X = 0.0f; Y = 0.0f }

// Step once per frame
for _ in 1..3 do
    let env =
        { Rand = rand
          Rank = rank
          Aim = { ToPlayer = 0.0f; ToEnemy = 0.0f }
          Spawn = { ToPlayer = 0.0f; ToEnemy = 0.0f } }

    let frame = Runner.stepWith env run { run.Motion with Pos = pos }
    pos <- { X = pos.X + frame.Delta.X; Y = pos.Y + frame.Delta.Y }
    run <- frame.Run
    // frame.Spawned: bullets fired this frame. frame.Vanished: this bullet is gone
```

## Links

- Source, samples and issues: https://github.com/zecl/FsBulletML-2
- BulletML specification (ABA Games): http://www.asahi-net.or.jp/~cs8k-cyu/bulletml/

MIT License.

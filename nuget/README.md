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
| `FsBulletML2.TypeProviders` | Type providers that turn an XML, SXML or FSB pattern into a typed value at compile time |

`Core`, `Front` and `Dsl` target `net10.0` and `netstandard2.1` (Unity). `Parser` and `TypeProviders` target `net10.0`.

## Run a pattern

### F#

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

### C#

The F# API is reachable from C#. Functions compile to PascalCase (`Runner.Load`, `NewRoot`, `StepWith`),
and the readers live in `BulletmlModule`.

```csharp
using FsBulletML2;
using Microsoft.FSharp.Core;
using static FsBulletML2.Domain;

var xml = """
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <fire><direction type="absolute">0</direction><speed>2</speed><bullet/></fire>
    <wait>1</wait>
    <vanish/>
  </action>
</bulletml>
""";

var rand = FuncConvert.FromFunc(() => 0.5f);
var rank = 0.5f;

// Load once per pattern
var script = Runner.Load(rand, rank, BulletmlModule.readXmlString(xml));
var run = Runner.NewRoot(DTD.BulletType.Enemy, script);
var pos = new Vec2(0f, 0f);

// Step once per frame
for (var i = 0; i < 3; i++)
{
    var env = new Env(rand, rank, new Aim(0f, 0f), new SpawnAim(0f, 0f));
    var m = run.Motion;
    var frame = Runner.StepWith(env, run, new Motion(pos, m.Speed, m.Dir, m.Accel));
    pos = new Vec2(pos.X + frame.Delta.X, pos.Y + frame.Delta.Y);
    run = frame.Run;
    // frame.Spawned: bullets fired this frame. frame.Vanished: this bullet is gone
}
```

## Links

- Source, samples and issues: https://github.com/zecl/FsBulletML-2
- BulletML specification (ABA Games): http://www.asahi-net.or.jp/~cs8k-cyu/bulletml/

MIT License.

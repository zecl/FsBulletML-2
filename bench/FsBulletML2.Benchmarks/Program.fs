module FsBulletML2.Benchmarks.Program

open BenchmarkDotNet.Running

/// 走らせ方
///
///   dotnet run -c Release --project bench/FsBulletML2.Benchmarks -- --filter *
///   dotnet run -c Release --project bench/FsBulletML2.Benchmarks -- --filter *StepBenchmarks*
///
/// Debug で走らせると BenchmarkDotNet が止める（最適化が効いていない数を
/// 出さないため）。測るときは必ず Release。
[<EntryPoint>]
let main argv =
  BenchmarkSwitcher.FromAssembly(typeof<StepBenchmarks>.Assembly).Run(argv) |> ignore
  0

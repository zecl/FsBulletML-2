namespace FsBulletML2.Sample.MonoGame.FSharp

open FsBulletML2

module Program =
  [<EntryPoint>]
  let main (args : string[]) = 
    BulletMLManager.Init(new BulletFunctions())
    use game = new FsBulletML2SampleGame()
    game.Run()
    0

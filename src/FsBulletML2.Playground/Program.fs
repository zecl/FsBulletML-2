namespace FsBulletML2.Playground

open Microsoft.AspNetCore.Components.WebAssembly.Hosting

module Program =

  [<EntryPoint>]
  let Main args =
    let builder = WebAssemblyHostBuilder.CreateDefault(args)
    builder.RootComponents.Add<MyApp>("#main")
    builder.Build().RunAsync() |> ignore
    0

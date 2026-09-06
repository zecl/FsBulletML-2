namespace FsBulletML2.Sample.Browser

open Microsoft.AspNetCore.Components.WebAssembly.Hosting

module Program =

  [<EntryPoint>]
  let Main args =
    let builder = WebAssemblyHostBuilder.CreateDefault(args)
    builder.RootComponents.Add<MyApp>("#main")
    builder.Build().RunAsync() |> ignore
    0

namespace FsBulletML2.TypeProviders

open System
open System.IO

[<CompilerMessage("hidden...", 13730, IsError = false, IsHidden = true)>]
type Context(onChanged: unit -> unit) =
    let disposingEvent = Event<_>()
    let mutable lastChanged = DateTime.Now.AddSeconds -1.0
    let sync = obj ()

    let trigger () =
        let shouldTrigger =
            lock sync (fun _ ->
                match lastChanged with
                | time when DateTime.Now - time <= TimeSpan.FromSeconds 1. -> false
                | _ ->
                    lastChanged <- DateTime.Now
                    true)

        if shouldTrigger then
            onChanged ()

    member __.Disposing: IEvent<unit> = disposingEvent.Publish
    member __.Trigger = trigger

    interface IDisposable with
        member __.Dispose() = disposingEvent.Trigger()

#nowarn "13730"
module internal Helper =
    let findConfigFile (resolutionFolder: string) (configFileName: string) =
        if Path.IsPathRooted configFileName then
            configFileName
        else
            Path.Combine(resolutionFolder, configFileName)

    let watchFile (fileName: string) (ctx: Context) =
        if fileName.StartsWith("http", System.StringComparison.InvariantCultureIgnoreCase) then
            ()
        else

            let path = Path.GetDirectoryName(fileName)
            let name = Path.GetFileName(fileName)
            let watcher = new FileSystemWatcher(Filter = name, Path = path)

            let onChanged =
                (fun (fsargs: FileSystemEventArgs) ->
                    match fsargs.ChangeType with
                    | WatcherChangeTypes.Changed
                    | WatcherChangeTypes.Created
                    | WatcherChangeTypes.Deleted -> ctx.Trigger()
                    | _ -> ())

            try
                watcher.Deleted.Add onChanged
                watcher.Changed.Add onChanged
                watcher.Error.Add(fun _ -> watcher.Dispose())
                watcher.EnableRaisingEvents <- true
                ctx.Disposing.Add watcher.Dispose
            with exn ->
                watcher.Dispose()

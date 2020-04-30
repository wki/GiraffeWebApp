module ServerInspector.CommandLine
open Argu

type Arguments =
    | [<AltCommandLine("-c")>] ConfigFile of string
    | [<AltCommandLine("-p")>] HttpPort of int
    | [<AltCommandLine("-n")>] NoBrowserLaunch
    | [<AltCommandLine("-v")>] Verbose
with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | ConfigFile _ -> "config file to read (default config.json in current dir)"
            | HttpPort _ -> "http port to use (default: 5555)"
            | NoBrowserLaunch -> "do not open browser at start"
            | Verbose -> "tell every action"

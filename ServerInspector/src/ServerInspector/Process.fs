// control process start and actions
module ServerInspector.Process
open System
open System.Diagnostics

type ProcessMessage =
    | Started of cmdLine:string
    | Failed of reason:string
    | Terminated of int
    | StdOutReceived of string
    | StdErrReceived of string

let splitCmdFromArgs (cmdLine:string) =
    match cmdLine.Split([|' '|], 2) with
        | [|  |] -> "", ""
        | [| cmd |] -> cmd, ""
        | [| cmd; args |] -> cmd, args
        | _ -> "", ""

/// <summary>Runs a process and waits until it returns</summary>
let run (cmdLine:string): unit =
    let (fileName, arguments) = splitCmdFromArgs cmdLine
    let p = Process.Start(fileName, arguments)
    p.WaitForExit()

/// <summary>Start a process and report all events</summary>
let start (cmdLine:string) (reportMessage: ProcessMessage -> unit): unit =
    let (fileName, arguments) = splitCmdFromArgs cmdLine

    let procStartInfo =
        ProcessStartInfo(
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            FileName = fileName,
            Arguments = arguments
        )
    let p = new Process(StartInfo = procStartInfo)

    let outputHandler f (_sender:obj) (args:DataReceivedEventArgs) = f args.Data
    let send msg = msg >> reportMessage |> outputHandler

    let exitHandler (_sender:obj) (args:EventArgs) =
        reportMessage(Terminated(p.ExitCode))
        p.Dispose()

    p.OutputDataReceived.AddHandler(DataReceivedEventHandler(send StdOutReceived))
    p.ErrorDataReceived.AddHandler(DataReceivedEventHandler(send StdErrReceived))
    p.Exited.AddHandler(EventHandler(exitHandler))

    let started =
        try
            p.Start()
        with
        | ex -> false

    if started then
        p.BeginOutputReadLine()
        p.BeginErrorReadLine()
    else
        reportMessage(Failed fileName)

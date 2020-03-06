module GiraffeWebApp.MailboxDemo

// Hint: lineContainer and timer
// will be initialized when this module is first used
// see: https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/members/constructors#static-constructors-or-type-constructors

open System
open System.Threading
open Microsoft.FSharp.Control

type Message =
    | AddLine of string
    | ListLines of AsyncReplyChannel<string list>

let lineContainer = MailboxProcessor.Start(fun inbox ->
    // remark: lines are stored in reverse.
    let rec loop lines =
        async {
            let! msg = inbox.Receive()
            match msg with
            | AddLine line ->
                printfn "Received: %s" line
                return! loop (line::lines |> List.truncate 10)
            | ListLines reply ->
                printfn "ListLines requested"
                reply.Reply(List.rev lines)
                return! loop lines
        }
    printfn "starting LineContainer"
    loop []
)

let timer =
    let random = new Random()
    async {
        let rec loop() =
            Thread.Sleep(500 + random.Next(2000))
            printfn "Tick"
            lineContainer.Post (AddLine(System.DateTime.Now.ToString()))
            loop()
        loop()
    }

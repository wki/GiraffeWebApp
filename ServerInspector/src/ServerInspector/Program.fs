module ServerInspector.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe

// open ServerInspector.Models
open ServerInspector.Views
open ServerInspector.MailboxDemo
open Microsoft.AspNetCore.Http

let checkHeader: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        match ctx.TryGetRequestHeader "X-MyHeader" with
        | None -> skipPipeline
        | Some _ -> next(ctx)

let fetchLines() =
    printfn "Fetch lines..."
    lineContainer.PostAndReply ListLines

let webApp =
    choose [
        GET >=>
            choose [
                route "/"      >=> text "hi"
                route "/hello" >=> renderHelloWorld
                routef "/hello/%s" renderHelloX
                route "/secret" >=> checkHeader >=> text "unlocked"
                route "/lines" >=> warbler(fun _ -> fetchLines() |> renderLines)
            ]
        setStatusCode 404 >=> text "Not Found" ]

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------
let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IWebHostEnvironment>()
    (match env.EnvironmentName = "Develop" with
    | true  -> app.UseDeveloperExceptionPage()
    | false -> app.UseGiraffeErrorHandler errorHandler)
        .UseStaticFiles()
        .UseGiraffe(webApp)

let configureServices (services : IServiceCollection) =
    services
        .AddGiraffe()
    |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder
        .AddFilter(fun l -> l.Equals LogLevel.Error)
        .AddConsole()
        .AddDebug()
    |> ignore

[<EntryPoint>]
let main _ =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "WebRoot")

    // periodic actions on certain mailbox processors.
    Async.Start timer

    let webTask =
        WebHostBuilder()
            .UseKestrel()
            .UseContentRoot(contentRoot)
            .UseWebRoot(webRoot)
            .Configure(Action<IApplicationBuilder> configureApp)
            .ConfigureServices(configureServices)
            .ConfigureLogging(configureLogging)
            .Build()
            .RunAsync()

    let openInBrowser =
        async {
            do! Async.Sleep 500
            Process.run "open http://localhost:5000"
        }
    openInBrowser |> Async.Start

    webTask.Wait()

    0

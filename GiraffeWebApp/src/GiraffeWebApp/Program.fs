module GiraffeWebApp.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Giraffe.GiraffeViewEngine

// ---------------------------------
// Models
// ---------------------------------
type Message =
    {
        Text : string
    }

// ---------------------------------
// Views
// ---------------------------------
let layout (content: XmlNode list) =
    html [] [
        head [] [
            title []  [ str "Giraffe" ]
        ]
        body [] content
    ]
    
let renderHelloWorld =
    let model = { Text = "world" }
    [
        div [] [ sprintf "%s" model.Text |> str ]
    ]
    |> layout
    |> htmlView

// ---------------------------------
// Web app
// ---------------------------------
let webApp =
    choose [
        GET >=>
            choose [
                route "/"      >=> text "hi"
                route "/hello" >=> renderHelloWorld
            ]
        setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------
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
    WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(contentRoot)
        .UseWebRoot(webRoot)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    
    0
    
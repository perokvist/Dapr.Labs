module Program

open System
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore
open Microsoft.Extensions.Logging
open System.Text.Json
open System.Threading.Tasks
open System.Text.Json.Serialization
open System.IO


type TestEvent =
    {
        Message : string
    }
   
let jsonOptions = JsonSerializerOptions() |> fun(x) -> 
    x.Converters.Add(JsonFSharpConverter()) |> ignore
    x 

let deserialize<'T> (json:Stream) = JsonSerializer.DeserializeAsync<'T>(json, jsonOptions).AsTask() |> Async.AwaitTask

let createLogger (ctx:HttpContext) (name:string) = ctx.RequestServices.GetService<ILoggerFactory>() |> fun(x) -> x.CreateLogger(name)
let log (logger:ILogger) (message:string) = LoggerExtensions.LogInformation(logger, message)

let helloRoute = fun(ctx:HttpContext) -> 
    createLogger ctx "sample" |> log <| "hello world" |> ignore
    ctx.Response.WriteAsync "Hello World!"

let sampleRoute = fun(ctx:HttpContext) ->
    createLogger ctx "subscriber" |> log <| "F# got event (pub/sub)" |> ignore
    ctx.Response.WriteAsync "Hello sample"

let sampleBindingRoute = fun(ctx:HttpContext) ->
    let logger = createLogger ctx "binding" |> log 
    do logger "F# got event (binding)"
    async {
        let! payload = deserialize<TestEvent> ctx.Request.Body 
        do logger payload.Message
        ctx.Response.WriteAsync("Hello sample") |> Async.AwaitTask |> ignore
    } |> Async.StartAsTask :> Task

    
let configureServices (services : IServiceCollection) =
    services.AddDaprClient() |> ignore
    services.AddLogging() |> ignore
    services.AddSingleton(jsonOptions) |> ignore

let configureApp (app : IApplicationBuilder) =
    app.UseRouting() |> ignore
    app.UseCloudEvents() |> ignore
        
    app.UseEndpoints(fun endpoints ->
            endpoints.MapSubscribeHandler() |> ignore
            endpoints.MapGet("/", fun ctx -> helloRoute ctx) |> ignore
            endpoints.MapPost("/sample", fun ctx -> sampleRoute ctx).WithTopic("sample") |> ignore
            endpoints.MapPost("/hub", fun ctx -> sampleBindingRoute ctx) |> ignore
            ) |> ignore

let configureLogging (loggerBuilder : ILoggingBuilder) =
    loggerBuilder.AddConsole() |> ignore

[<EntryPoint>]
let main args =
    WebHost.CreateDefaultBuilder(args)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0

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
   
let configureServices (services : IServiceCollection) =
    services.AddDaprClient() |> ignore
    services.AddLogging() |> ignore
    services.AddSingleton(JsonSerializerOptions()) |> ignore
                 //{
                     //PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                     //PropertyNameCaseInsensitive = true,
                 //}) |> ignore

            
let createLogger (ctx:HttpContext) (name:string) = ctx.RequestServices.GetService<ILoggerFactory>() |> fun(x) -> x.CreateLogger(name)
let log (message:string) (logger:ILogger) = LoggerExtensions.LogInformation(logger, message)

let helloRoute = fun(ctx:HttpContext) -> 
    createLogger ctx "sample" |> log "hello world" |> ignore
    ctx.Response.WriteAsync "Hello World!"

let sampleRoute = fun(ctx:HttpContext) ->
    createLogger ctx "subscriber" |> log "F# got event" |> ignore
    ctx.Response.WriteAsync "Hello sample"

let configureApp (app : IApplicationBuilder) =
    app.UseRouting() |> ignore
    app.UseCloudEvents() |> ignore
        
    app.UseEndpoints(fun endpoints ->
            endpoints.MapSubscribeHandler() |> ignore
            endpoints.MapGet("/", fun ctx -> helloRoute ctx) |> ignore
            endpoints.MapPost("/sample", fun ctx -> sampleRoute ctx).WithTopic("sample") |> ignore
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

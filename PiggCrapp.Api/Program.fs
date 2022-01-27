module PiggCrapp.Api.Program

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open PiggCrapp.Api.Handlers
open Giraffe

let webApp =
    choose [
        GET >=>
            choose [
                route "/users" >=> getUsersHandler
                routef "/users/%O" getUserHandler
            ]
        POST >=>
            choose [
                route "/users" >=> postUserHandler
            ]
        PUT >=>
            choose [
                routef "/users/%O" updateUserHandler
            ]
        DELETE >=>
            choose [
                routef "/users/%O" deleteUserHandler
            ]
        ]

let configureApp (app : IApplicationBuilder) =
    // Add Giraffe to the ASP.NET Core pipeline
    app.UseGiraffe webApp

let configureServices (services : IServiceCollection) =
    // Add Giraffe dependencies
    services.AddGiraffe() |> ignore

[<EntryPoint>]
let main _ =
    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .Configure(configureApp)
                    .ConfigureServices(configureServices)
                    |> ignore)
        .Build()
        .Run()
    0
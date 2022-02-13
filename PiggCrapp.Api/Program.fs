module PiggCrapp.Api.Program

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Newtonsoft.Json
open Newtonsoft.Json.Converters
open PiggCrapp.Api.UsersHandlers
open PiggCrapp.Api.WorkoutsHandlers
open PiggCrapp.Api.ExercisesHandlers
open PiggCrapp.Api.SetsHandlers
open Giraffe

let errorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")

    clearResponse
    >=> ServerErrors.INTERNAL_ERROR ex.Message

let shortGuidHandler shortGuid =
    ShortGuid.toGuid shortGuid

let webApp =
    choose [ GET
             >=> choose [ route "/users" >=> getUsersHandler
                          routef "/users/%s" (fun s -> shortGuidHandler s |> getUserHandler)
                          routef "/users/%s/workouts" getWorkoutsHandler
                          routef "/workouts/%s" getWorkoutHandler
                          routef "/workouts/%O/exercises" getExercisesHandler
                          routef "/exercises/%O" getExerciseHandler
                          routef "/exercises/%O/sets" getSetsHandler
                          routef "/exercises/%O/sets/%i" getSetHandler ]
             POST
             >=> choose [ route "/users" >=> postUserHandler
                          routef "/users/%s/workouts" postWorkoutHandler
                          routef "/workouts/%O/exercises" postExerciseHandler
                          routef "/exercises/%O/sets" postSetHandler ]
             PUT
             >=> choose [ routef "/users/%s" (fun string -> shortGuidHandler string |> updateUserHandler)
                          routef "/workouts/%s" updateWorkoutHandler
                          routef "/exercises/%O" updateExerciseHandler
                          routef "/exercises/%O/sets/%i" updateSetHandler ]
             DELETE
             >=> choose [ routef "/users/%s" (fun string -> shortGuidHandler string |> deleteUserHandler)
                          routef "/workouts/%s" deleteWorkoutHandler
                          routef "/exercises/%O" deleteExerciseHandler
                          routef "/exercises/%O/sets/%i" deleteSetHandler ] ]

let configureApp (app: IApplicationBuilder) =
    // Add Giraffe to the ASP.NET Core pipeline
    app
        .UseGiraffeErrorHandler(errorHandler)
        .UseGiraffe webApp

let configureServices (services: IServiceCollection) =
    // Add Giraffe dependencies
    services.AddGiraffe() |> ignore

[<EntryPoint>]
let main _ =
    Host
        .CreateDefaultBuilder()
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .Configure(configureApp)
                .ConfigureServices(configureServices)
            |> ignore)
        .Build()
        .Run()

    0

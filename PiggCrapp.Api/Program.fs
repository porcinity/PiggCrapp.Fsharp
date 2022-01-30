module PiggCrapp.Api.Program

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open PiggCrapp.Api.UsersHandlers
open PiggCrapp.Api.WorkoutsHandlers
open PiggCrapp.Api.ExercisesHandlers
open PiggCrapp.Api.SetsHandlers
open Giraffe

let webApp =
    choose [
        GET >=>
            choose [
                route "/users" >=> getUsersHandler
                routef "/users/%s" getUserHandler
                routef "/users/%O/workouts" getWorkoutsHandler
                routef "/workouts/%O" getWorkoutHandler
                routef "/workouts/%O/exercises" getExercisesHandler
                routef "/exercises/%O" getExerciseHandler
                routef "/exercises/%O/sets" getSetsHandler
                routef "/exercises/%O/sets/%i" getSetHandler
            ]
        POST >=>
            choose [
                route "/users" >=> postUserHandler
                routef "/users/%O/workouts" postWorkoutHandler
                routef "/workouts/%O/exercises" postExerciseHandler
                routef "/exercises/%O/sets" postSetHandler
            ]
        PUT >=>
            choose [
                routef "/users/%O" updateUserHandler
                routef "/workouts/%O" updateWorkoutHandler
                routef "/exercises/%O" updateExerciseHandler
                routef "/exercises/%O/sets/%i" updateSetHandler
            ]
        DELETE >=>
            choose [
                routef "/users/%O" deleteUserHandler
                routef "/workouts/%O" deleteWorkoutHandler
                routef "/exercises/%O" deleteExerciseHandler
                routef "/exercises/%O/sets/%i" deleteSetHandler
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
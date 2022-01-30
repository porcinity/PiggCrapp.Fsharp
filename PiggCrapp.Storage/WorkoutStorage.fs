module PiggCrapp.Storage.Workouts

open Npgsql.FSharp
open PiggCrapp.Domain.Ids
open PiggCrapp.Domain.Workouts

let connStr = "Host=localhost;Database=PiggCrapp;Username=pigg"
    
let findWorkoutsAsync userId =
    connStr
    |> Sql.connect
    |> Sql.query "select * from workouts where user_id = @id"
    |> Sql.parameters [ "@id", Sql.uuid <| UserId.toGuid userId]
    |> Sql.executeAsync (fun read ->
        {
           WorkoutId = read.uuid "workout_id" |> WorkoutId
           Date = read.dateTime "workout_date"
           Variation = read.text "workout_variation" |> WorkoutVariation.fromString
           Exercises = []
           Owner = read.uuid "user_id" |> UserId
        })

let findWorkoutAsync workoutId =
    connStr
    |> Sql.connect
    |> Sql.query "select workout_id, workout_date, workout_variation, user_id
                 from workouts
                 where workout_id = @workoutId"
    |> Sql.parameters [ "@workoutId", Sql.uuid workoutId ]
    |> Sql.executeAsync (fun read ->
        {
           WorkoutId = read.uuid "workout_id" |> WorkoutId
           Date = read.dateTime "workout_date"
           Variation = read.text "workout_variation" |> WorkoutVariation.fromString
           Exercises = []
           Owner = read.uuid "user_id" |> UserId
        })

let insertWorkoutAsync workout =
    connStr
    |> Sql.connect
    |> Sql.query "insert into workouts
                  (workout_id, workout_date, workout_variation, user_id)
                  values (@id, @date, @variation, @owner)"
    |> Sql.parameters [
        "@id", Sql.uuid <| WorkoutId.toGuid workout.WorkoutId
        "@date", Sql.timestamp workout.Date
        "@variation", Sql.text <| WorkoutVariation.toString workout.Variation
        "@owner", Sql.uuid <| UserId.toGuid workout.Owner
    ]
    |> Sql.executeNonQueryAsync

let updateWorkoutAsync workout =
    connStr
    |> Sql.connect
    |> Sql.query "update workouts set
                  workout_date = @date, workout_variation = @variation
                  where workout_id = @id"
    |> Sql.parameters [
        "@date", Sql.timestamp workout.Date
        "@variation", Sql.text <| WorkoutVariation.toString workout.Variation
        "@id", Sql.uuid <| WorkoutId.toGuid workout.WorkoutId
    ]
    |> Sql.executeNonQueryAsync
    
let deleteWorkoutAsync workoutId =
    connStr
    |> Sql.connect
    |> Sql.query "delete from workouts where workout_id = @id"
    |> Sql.parameters [ "@id", Sql.uuid <| WorkoutId.toGuid workoutId]
    |> Sql.executeNonQueryAsync
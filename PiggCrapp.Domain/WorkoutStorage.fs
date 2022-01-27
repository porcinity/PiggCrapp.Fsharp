module PiggCrapp.Storage

open Npgsql.FSharp
open PiggCrapp.Ids
open PiggCrapp.Workouts

let connStr = "Host=localhost;Database=PiggCrapp;Username=test;Password=test"
    
let findWorkoutsAsync userId =
    connStr
    |> Sql.connect
    |> Sql.query "select * from workouts where workout_owner = @id"
    |> Sql.parameters [ "@id", Sql.uuid <| UserId.toGuid userId]
    |> Sql.execute (fun read ->
        {
           WorkoutId = read.uuid "workout_id" |> WorkoutId
           Date = read.dateTime "workout_date"
           Variation = read.text "workout_variation" |> WorkoutVariation.fromString
           Exercises = []
           Owner = read.uuid "workout_owner" |> UserId
        })

let findWorkoutAsync workoutId =
    connStr
    |> Sql.connect
    |> Sql.query "select workout_date, workout_variation, workout_owner
                 from workouts where workout_id = @workoutId"
    |> Sql.parameters [ "@workoutId", Sql.uuid workoutId ]
    |> Sql.executeAsync (fun read ->
        {
           WorkoutId = read.uuid "workout_id" |> WorkoutId
           Date = read.dateTime "workout_date"
           Variation = read.text "workout_variation" |> WorkoutVariation.fromString
           Exercises = []
           Owner = read.uuid "workout_owner" |> UserId
        })

let insertWorkoutAsync workout =
    connStr
    |> Sql.connect
    |> Sql.query "insert into workouts
                  (workout_id, workout_date, workout_variation, workout_owner)
                  values (@id, @date, @variation, @owner)"
    |> Sql.parameters [
        "@id", Sql.uuid <| WorkoutId.toGuid workout.WorkoutId
        "@date", Sql.timestamp workout.Date
        "@variation", Sql.text <| WorkoutVariation.toString workout.Variation
    ]
    
let deleteWorkoutAsync workout =
    connStr
    |> Sql.connect
    |> Sql.query "delete from workouts where workout_id = @id"
    |> Sql.parameters [ "@id", Sql.uuid <| WorkoutId.toGuid workout.WorkoutId ]
    |> Sql.executeNonQueryAsync
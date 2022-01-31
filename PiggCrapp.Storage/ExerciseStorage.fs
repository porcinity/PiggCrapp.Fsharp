module PiggCrapp.Storage.Exercises

open CommonExtensionsAndTypesForNpgsqlFSharp
open FSharpPlus
open Npgsql.FSharp
open PiggCrapp.Domain.Ids
open PiggCrapp.Domain.Exercises

let connStr = "Host=localhost;Database=PiggCrapp;Username=pigg"

let findExercisesAsync workoutId =
    connStr
    |> Sql.connect
    |> Sql.query "select *
                  from exercises
                  where workout_id = @id"
    |> Sql.parameters [ "@id", Sql.uuid <| WorkoutId.toGuid workoutId ]
    |> Sql.executeAsync (fun read ->
        {
           ExerciseId = read.uuid "exercise_id" |> ExerciseId
           Name = read.text "exercise_type" |> ExerciseName
           Notes = None
           Sets = []
           WorkoutId = workoutId
        })

let findExerciseAsync exerciseId =
    connStr
    |> Sql.connect
    |> Sql.query "select * from exercises where exercise_id = @id"
    |> Sql.parameters [ "@id", Sql.uuid <| ExerciseId.toGuid exerciseId ]
    |> Sql.executeAsync (fun read ->
        {
           ExerciseId = read.uuid "exercise_id" |> ExerciseId
           Name = read.text "exercise_type" |> ExerciseName
           Notes = None
           Sets = []
           WorkoutId = read.uuid "workout_id" |> WorkoutId
        })
    |> Task.map List.tryHead
    
let insertExerciseAsync exercise =
    connStr
    |> Sql.connect
    |> Sql.query "insert into exercises (exercise_id, exercise_type, workout_id)
                  values (@exercise_id, @type, @workout_id)"
    |> Sql.parameters [
        "@exercise_id", Sql.uuid <| ExerciseId.toGuid exercise.ExerciseId
        "@type", Sql.text <| ExerciseName.toString exercise.Name
        "@workout_id", Sql.uuid <| WorkoutId.toGuid exercise.WorkoutId
    ]
    |> Sql.executeNonQueryAsync

let updateExerciseAsync exercise =
    connStr
    |> Sql.connect
    |> Sql.query "update exercises
                  set exercise_type = @type
                  where exercise_id = @id"
    |> Sql.parameters [
        "@type", Sql.string <| ExerciseName.toString exercise.Name
        "@id", Sql.uuid <| ExerciseId.toGuid exercise.ExerciseId
    ]
    |> Sql.executeNonQueryAsync
    
let deleteExerciseAsync exerciseId =
    connStr
    |> Sql.connect
    |> Sql.query "delete from exercises where exercise_id = @id"
    |> Sql.parameters [ "@id", Sql.uuid <| ExerciseId.toGuid exerciseId ]
    |> Sql.executeNonQueryAsync
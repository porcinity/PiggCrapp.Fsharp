module PiggCrapp.ExerciseStorage

open Npgsql.FSharp
open WorkoutModel

let connStr = "Host=localhost;Database=PiggCrapp;Username=test;Password=test"


let findExercisesAsync workoutId =
    connStr
    |> Sql.connect
    |> Sql.query "select * from exercises
                  from workouts w
                  join exercises e on w.workout_id = e.workout_id
                  where e.workout_id = @id" 
    |> Sql.parameters [ "@id", Sql.uuid <| WorkoutId.toGuid workoutId ]
    |> Sql.execute (fun read ->
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
    |> Sql.query "select * exercise where exercise_id = @id"
    |> Sql.parameters [ "@id", Sql.uuid <| ExerciseId.toGuid exerciseId ]
    |> Sql.execute (fun read ->
        {
           ExerciseId = read.uuid "exercise_id" |> ExerciseId
           Name = read.text "exercise_type" |> ExerciseName
           Notes = None
           Sets = []
           WorkoutId = read.uuid "workout_id" |> WorkoutId
        })
    
let insertExerciseAsync exercise =
    connStr
    |> Sql.connect
    |> Sql.query "insert into exercises (exercise_id, exercise_type, workout_id)
                  values (@exercise_id, @type, @workout_id"
    |> Sql.parameters [
        "@exercise_id", Sql.uuid <| ExerciseId.toGuid exercise.ExerciseId
        "@type", Sql.text <| ExerciseName.toString exercise.Name
        "@workout_id", Sql.uuid <| WorkoutId.toGuid exercise.WorkoutId
    ]
    |> Sql.executeNonQueryAsync
    
let deleteExerciseAsync exerciseId =
    connStr
    |> Sql.connect
    |> Sql.query "delete from exercises where exercise_id = @id"
    |> Sql.parameters [ "@id", Sql.uuid <| ExerciseId.toGuid exerciseId ]
    |> Sql.executeNonQueryAsync
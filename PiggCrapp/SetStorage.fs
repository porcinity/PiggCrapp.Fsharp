module PiggCrapp.SetStorage

open Npgsql.FSharp
open WorkoutModel

let connStr = "Host=localhost;Database=PiggCrapp;Username=test;Password=test"

let findSetsAsync exerciseId =
    connStr
    |> Sql.connect
    |> Sql.query "select * from sets
                  from exercises e
                  join sets s on e.exercise_id = s.exercise_id
                  where s.exercise_id = @id"
    |> Sql.parameters [ "@id", Sql.uuid <| ExerciseId.toGuid exerciseId ]
    |> Sql.execute (fun read ->
        {
           RegularSetId = read.uuid "set_id" |> RegularSetId
           Weight = read.double "weight" |> Weight
           Reps = read.int "reps" |> Reps
           Exercise = read.uuid "exercise_id" |> ExerciseId
        })
    
let findSetAsync setId =
    connStr
    |> Sql.connect
    |> Sql.query "select * from sets where set_id = @id"
    |> Sql.parameters [ "@id", Sql.uuid <| RegularSetId.toGuid setId ]
    |> Sql.execute (fun read ->
        {
           RegularSetId = read.uuid "set_id" |> RegularSetId
           Weight = read.double "weight" |> Weight
           Reps = read.int "reps" |> Reps
           Exercise = read.uuid "exercise_id" |> ExerciseId
        })
    
let insertSetAsync set =
    connStr
    |> Sql.connect
    |> Sql.query "insert into sets (set_id, weight, reps, exercise_id)
                  values (@id, @weight, @reps, @exercise_id)"
    |> Sql.parameters [
        "@id", Sql.uuid <| RegularSetId.toGuid set.RegularSetId
        "@weight", Sql.double <| Weight.toDouble set.Weight
        "@reps", Sql.int <| Reps.toInt set.Reps
        "@exercise_id", Sql.uuid <| ExerciseId.toGuid set.Exercise
    ]
    |> Sql.executeNonQueryAsync
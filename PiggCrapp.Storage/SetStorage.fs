module PiggCrapp.Storage.Sets

open Npgsql.FSharp
open PiggCrapp.Domain.Ids
open PiggCrapp.Domain.Sets
open PiggCrapp.Domain.Measurements
open FSharpPlus

let connStr = "Host=localhost;Database=PiggCrapp;Username=pigg"

let findSetsAsync exerciseId =
    connStr
    |> Sql.connect
    |> Sql.query "select *
                  from sets
                  where exercise_id = @id"
    |> Sql.parameters [ "@id", Sql.uuid <| ExerciseId.toGuid exerciseId ]
    |> Sql.executeAsync (fun read ->
        {
           RegularSetId = read.int "set_id" |> RegularSetId
           Weight = (read.double "set_weight") * 1.0<lbs> |> Weight
           Reps = read.int "set_reps" |> Reps
           Exercise = read.uuid "exercise_id" |> ExerciseId
        })
    
let findSetAsync exerciseId setId =
    connStr
    |> Sql.connect
    |> Sql.query "select * from sets where set_id = @id and exercise_id = @ex"
    |> Sql.parameters [
        "@id", Sql.int <| RegularSetId.toInt setId
        "@ex", Sql.uuid <| ExerciseId.toGuid exerciseId
    ]
    |> Sql.executeAsync (fun read ->
        {
           RegularSetId = read.int "set_id" |> RegularSetId
           Weight = (read.double "set_weight") * 1.0<lbs> |> Weight
           Reps = read.int "set_reps" |> Reps
           Exercise = read.uuid "exercise_id" |> ExerciseId
        })
    |> Task.map List.tryHead
    
let insertSetAsync set =
    connStr
    |> Sql.connect
    |> Sql.query "insert into sets (set_id, set_weight, set_reps, exercise_id)
                  values (@id, @weight, @reps, @exercise_id)"
    |> Sql.parameters [
        "@id", Sql.int <| RegularSetId.toInt set.RegularSetId
        "@weight", Sql.double <| Weight.toDouble set.Weight
        "@reps", Sql.int <| Reps.toInt set.Reps
        "@exercise_id", Sql.uuid <| ExerciseId.toGuid set.Exercise
    ]
    |> Sql.executeNonQueryAsync

let updateSetAsync set =
    connStr
    |> Sql.connect
    |> Sql.query "update sets
                  set set_weight = @weight, set_reps = @reps
                  where set_id = @setId and exercise_id = @exerciseId"
    |> Sql.parameters [
        "@setId", Sql.int <| RegularSetId.toInt set.RegularSetId
        "@weight", Sql.double <| Weight.toDouble set.Weight
        "@reps", Sql.int <| Reps.toInt set.Reps
        "@exerciseId", Sql.uuid <| ExerciseId.toGuid set.Exercise
    ]
    |> Sql.executeNonQueryAsync

let deleteSetAsync exerciseId setId =
    connStr
    |> Sql.connect
    |> Sql.query "delete from sets where set_id = @setId and exercise_id = @exerciseId"
    |> Sql.parameters [
        "@setId", Sql.int <| RegularSetId.toInt setId
        "@exerciseId", Sql.uuid <| ExerciseId.toGuid exerciseId
    ]
    |> Sql.executeNonQueryAsync
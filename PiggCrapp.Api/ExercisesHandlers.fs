module PiggCrapp.Api.ExercisesHandlers

open System
open Giraffe
open PiggCrapp.Domain.Exercises
open PiggCrapp.Domain.Ids
open PiggCrapp.ExerciseStorage
open FSharpPlus

type getExerciseDto =
    { Id : Guid
      Name : string }

module getExerciseDto =
    let fromDomain exercise =
        { Id = ExerciseId.toGuid exercise.ExerciseId
          Name = ExerciseName.toString exercise.Name }

type postExerciseDto =
    { Name : string }

module postExerciseDto =
    let toDomain dto workoutId =
        let name = ExerciseName.create dto.Name
        let workoutId' = Ok <| WorkoutId workoutId
        Exercise.create <!> name <*> workoutId'

let getExercisesHandler workoutId : HttpHandler =
    fun next ctx -> task {
        let! exercises = findExercisesAsync <| WorkoutId workoutId
        let dtos =
            exercises
            |> List.map getExerciseDto.fromDomain
        return! json dtos next ctx
    }

let getExerciseHandler exerciseId : HttpHandler =
    fun next ctx -> task {
        let! query =
            exerciseId
            |> ExerciseId
            |> findExerciseAsync
            |> Task.map List.tryHead
        match query with
        | Some exercise ->
            let dto = getExerciseDto.fromDomain exercise
            return! json dto next ctx
        | None ->
            return! RequestErrors.NOT_FOUND {||} next ctx
    }

let postExerciseHandler workoutId : HttpHandler =
    fun next ctx -> task {
        let! dto = ctx.BindJsonAsync<postExerciseDto> ()
        let result = postExerciseDto.toDomain dto workoutId
        match result with
        | Ok exercise ->
            do! insertExerciseAsync exercise |> Task.ignore
            return! json {| id = ExerciseId.toGuid exercise.ExerciseId |} next ctx
        | Error e ->
            return! RequestErrors.UNPROCESSABLE_ENTITY e next ctx
    }

let updateExerciseHandler exerciseId : HttpHandler =
    fun next ctx -> task {
        let! query =
            exerciseId
            |> ExerciseId
            |> findExerciseAsync
            |> Task.map List.tryHead
        match query with
        | Some exercise ->
            let! dto = ctx.BindJsonAsync<postExerciseDto> ()
            let updated = { exercise with Name = ExerciseName dto.Name }
            let! result = updateExerciseAsync updated
            let response = getExerciseDto.fromDomain updated
            return! json response next ctx
        | None ->
            return! RequestErrors.NOT_FOUND {||} next ctx
    }

let deleteExerciseHandler exerciseId : HttpHandler =
    fun next ctx -> task {
        match! deleteExerciseAsync <| ExerciseId exerciseId with
        | 1 -> return! Successful.NO_CONTENT next ctx
        | _ -> return! RequestErrors.NOT_FOUND {||} next ctx
    }
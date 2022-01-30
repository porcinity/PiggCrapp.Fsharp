module PiggCrapp.Api.WorkoutsHandlers

open System
open FSharpPlus
open Microsoft.AspNetCore.Http
open Giraffe
open PiggCrapp.Domain.Ids
open PiggCrapp.Domain.Workouts
open PiggCrapp.Storage.Workouts

type getWorkoutDto =
    { WorkoutId : Guid
      Date : string
      Variation : string
      Owner : Guid }

module getWorkoutDto =
    let fromDomain (workout: Workout) =
        { WorkoutId = WorkoutId.toGuid workout.WorkoutId
          Date = workout.Date.ToString("yyyy-MM-dd")
          Variation = WorkoutVariation.toString workout.Variation
          Owner = UserId.toGuid workout.Owner }

type postWorkoutDto =
    { Variation : string }

type putWorkoutDto =
    { Variation : string
      Date : string }

let getWorkoutsHandler userId next ctx = task {
    let! workouts = findWorkoutsAsync <| UserId userId
    let dtos =
        workouts
        |> List.map getWorkoutDto.fromDomain
    return! json dtos next ctx
}

let getWorkoutHandler workoutId next ctx = task {
    let! query =
        workoutId
        |> findWorkoutAsync
        |> Task.map List.tryHead
    match query with
    | Some workout ->
        let dto = getWorkoutDto.fromDomain workout
        return! json dto next ctx
    | None ->
        return! RequestErrors.NOT_FOUND {| message = "No workout with Id found." |} next ctx
}

let postWorkoutHandler userId next (ctx:HttpContext) = task {
    let! dto = ctx.BindJsonAsync<postWorkoutDto> ()
    let variation = WorkoutVariation.fromString dto.Variation
    let workout = Workout.create variation (UserId userId)
    let! res = insertWorkoutAsync workout
    let guid = WorkoutId.toGuid workout.WorkoutId
    return! redirectTo false $"https://localhost:7162/workouts/{guid}" next ctx
}

let updateWorkoutHandler workoutId next (ctx:HttpContext) = task {
    let! workout =
        workoutId
        |> findWorkoutAsync
        |> Task.map List.tryHead
    match workout with
    | Some w ->
        let! dto = ctx.BindJsonAsync<putWorkoutDto> ()
        let date = DateTime.Parse(dto.Date)
        let workout =
            { w with
                Variation = WorkoutVariation.fromString dto.Variation
                Date = date }
        updateWorkoutAsync workout |> Async.AwaitTask |> Async.RunSynchronously |> ignore
        let response = getWorkoutDto.fromDomain workout
        return! Successful.CREATED response next ctx
    | None ->
        return! RequestErrors.NOT_FOUND {| message = "No workout found with that Id"|} next ctx
}

let deleteWorkoutHandler workoutId next (ctx:HttpContext) = task {
    let! res = deleteWorkoutAsync <| WorkoutId workoutId
    if
        res = 1
    then
        ctx.SetStatusCode 204
        return! json {||} next ctx
    else
        return! RequestErrors.NOT_FOUND {| message = "No workout found with that Id" |}next ctx
}
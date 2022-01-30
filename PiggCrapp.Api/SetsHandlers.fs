module PiggCrapp.Api.SetsHandlers

open Giraffe
open PiggCrapp.Domain.Ids
open PiggCrapp.Domain.Sets
open PiggCrapp.Storage.Sets
open FSharpPlus
open FsToolkit.ErrorHandling

type getSetDto =
    { Id : int
      Reps : int
      Weight : double }

module getSetDto =
    let fromDomain (set:RegularSet) =
        { Id = RegularSetId.toInt set.RegularSetId
          Reps = Reps.toInt set.Reps
          Weight = Weight.toDouble set.Weight }

    let toDomain exerciseId dto = validation {
        let! exerciseId = Ok (ExerciseId exerciseId)
        and! regularSetId = Ok (RegularSetId dto.Id)
        and! weight = Weight.fromDbl dto.Weight
        and! reps = Reps.fromInt dto.Reps
        return RegularSet.create exerciseId regularSetId weight reps
    }

let getSetsHandler exerciseId : HttpHandler =
    fun next ctx -> task {
        let! sets =
            ExerciseId exerciseId
            |> findSetsAsync
            |> Task.map (List.map getSetDto.fromDomain)
            |> Task.map List.sort
        return! json sets next ctx
    }

let getSetHandler (exerciseId, setId) : HttpHandler =
    fun next ctx -> task {
        match! findSetAsync (ExerciseId exerciseId) (RegularSetId setId) with
        | Some set ->
            let dto = getSetDto.fromDomain set
            return! json dto next ctx
        | None -> return! RequestErrors.NOT_FOUND {||} next ctx
    }

let postSetHandler exerciseId : HttpHandler =
    fun next ctx -> task {
        let! dto = ctx.BindJsonAsync<getSetDto> ()
        match getSetDto.toDomain exerciseId dto with
        | Ok set ->
            Task.ignore (insertSetAsync set) |> ignore
            return! json {| id = RegularSetId.toInt set.RegularSetId |} next ctx
        | Error e ->
            return! RequestErrors.UNPROCESSABLE_ENTITY e next ctx
    }

let updateSetHandler (exerciseId, setId) : HttpHandler =
    fun next ctx -> task {
        let! dto = ctx.BindJsonAsync<getSetDto> ()
        let result = getSetDto.toDomain exerciseId dto
        let! set = findSetAsync (ExerciseId exerciseId) (RegularSetId setId)
        match result, set with
        | Ok updatedSet, Some originalSet ->
            let set = RegularSet.update originalSet updatedSet
            let! result = updateSetAsync set
            return! json (getSetDto.fromDomain originalSet) next ctx
        | Error e, _ -> return! RequestErrors.UNPROCESSABLE_ENTITY e next ctx
        | _, None -> return! RequestErrors.NOT_FOUND {||} next ctx
    }

let deleteSetHandler (exerciseId, setId) : HttpHandler =
    fun next ctx -> task {
        let! result = deleteSetAsync (ExerciseId exerciseId) (RegularSetId setId)
        return!
            (match result with
            | 1 -> Successful.NO_CONTENT
            | _ -> RequestErrors.NOT_FOUND "Not found.") next ctx
    }
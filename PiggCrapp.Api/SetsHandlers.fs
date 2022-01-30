module PiggCrapp.Api.SetsHandlers

open Giraffe
open PiggCrapp.Domain.Ids
open PiggCrapp.Domain.Sets
open PiggCrapp.Domain.SetStorage
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

    let toDomain exerciseId dto =
        let exerciseId = Ok (ExerciseId exerciseId)
        let regularSetId = Ok (RegularSetId dto.Id)
        let weight = Weight.fromDbl dto.Weight
        let reps = Reps.fromInt dto.Reps
        RegularSet.create <!> exerciseId <*> regularSetId <*> weight <*> reps

let getSetsHandler exerciseId : HttpHandler =
    fun next ctx -> task {
        let! sets =
            exerciseId
            |> ExerciseId
            |> findSetsAsync
            |> Task.map (List.map getSetDto.fromDomain)
        let ordered = List.sortBy (fun x -> x.Id) sets
        return! json ordered next ctx
    }

let getSetHandler (exerciseId, setId) : HttpHandler =
    fun next ctx -> task {
        let! set =
            setId
            |> RegularSetId
            |> findSetAsync (ExerciseId exerciseId)
            |> Task.map List.tryHead
            |> Task.map (Option.map getSetDto.fromDomain)
        match set with
        | Some set -> return! json set next ctx
        | None -> return! RequestErrors.NOT_FOUND {||} next ctx
    }

let postSetHandler exerciseId : HttpHandler =
    fun next ctx -> task {
        let! dto = ctx.BindJsonAsync<getSetDto> ()
        match getSetDto.toDomain exerciseId dto with
        | Ok set ->
            let! result = insertSetAsync set
            return! json {| id = RegularSetId.toInt set.RegularSetId |} next ctx
        | Error e ->
            return! RequestErrors.UNPROCESSABLE_ENTITY e next ctx
    }

let updateSetHandler (exerciseId, setId) : HttpHandler =
    fun next ctx -> task {
        let! dto = ctx.BindJsonAsync<getSetDto> ()
        let id = Ok (RegularSetId dto.Id)
        let weight = Weight.fromDbl dto.Weight
        let reps = Reps.fromInt dto.Reps
        let! set = findSetAsync (ExerciseId exerciseId) (RegularSetId setId) |> Task.map List.tryHead
        match set with
        | Some set ->
            let updatedSet = RegularSet.update <!> Ok set <*> id <*> weight <*> reps
            match updatedSet with
            | Ok set ->
                let! result = updateSetAsync set
                return! json (getSetDto.fromDomain set) next ctx
            | Error e ->
                return! RequestErrors.UNPROCESSABLE_ENTITY e next ctx
        | None ->
            return! RequestErrors.NOT_FOUND {||} next ctx
    }

let deleteSetHandler (exerciseId, setId) : HttpHandler =
    fun next ctx -> task {
        let! result = deleteSetAsync (ExerciseId exerciseId) (RegularSetId setId)
        return!
            (match result with
            | 1 -> Successful.NO_CONTENT
            | _ -> RequestErrors.NOT_FOUND "Not found.") next ctx
    }
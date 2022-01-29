module PiggCrapp.Api.SetsHandlers

open System
open Giraffe
open PiggCrapp.Domain.Ids
open PiggCrapp.Domain.Sets
open PiggCrapp.Domain.SetStorage
open FSharpPlus

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
        return! json sets next ctx
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

//let updateSetHandler (exerciseId, setId) : HttpHandler =
//    fun next ctx -> task {
//        let! dto = ctx.BindJsonAsync<getSetDto> ()
//        let! set = findSetAsync <| RegularSetId setId
//        let updatedSet =
//            { set with
//              RegularSetId = RegularSetId dto.Id
//              Weight =  }
//    }

let deleteSetHandler (exerciseId, setId) : HttpHandler =
    fun next ctx -> task {
        match! deleteSetAsync exerciseId setId with
        | 1 ->
            return! Successful.NO_CONTENT next ctx
        | _ -> return! RequestErrors.NOT_FOUND {||} next ctx
    }
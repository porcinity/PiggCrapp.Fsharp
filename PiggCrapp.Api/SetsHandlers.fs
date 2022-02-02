module PiggCrapp.Api.SetsHandlers

open System
open Giraffe
open PiggCrapp.Domain.Ids
open PiggCrapp.Domain.Sets
open PiggCrapp.Storage.Sets
open FSharpPlus
open FsToolkit.ErrorHandling

type RegularSetDto = { Id: int; Reps: int; Weight: double }

module RegularSetDto =
    let fromDomain (set: RegularSet) =
        { Id = RegularSetId.toInt set.RegularSetId
          Reps = Reps.toInt set.Reps
          Weight = Weight.toDouble set.Weight }

    let toDomain exerciseId dto =
        validation {
            let! exerciseId = Ok(ExerciseId exerciseId)
            and! regularSetId = Ok(RegularSetId dto.Id)
            and! weight = Weight.fromDbl dto.Weight
            and! reps = Reps.fromInt dto.Reps
            return RegularSet.create exerciseId regularSetId weight reps
        }

type RestPauseDto =
    { Id: int
      Range: string
      Weight: double }

// Begin: my attempt
type PostSetDto =
    { Id: int
      Weight: double
      Reps: int option
      Range: string option }

type SetDto =
    | GetSetDto of RegularSetDto
    | RestPauseDto of RestPauseDto
// End: my attempt

// Scott Wlaschin's approach
type WlaschinChoice =
    | A of RegularSetDto
    | B of RestPauseDto

type WlaschinDto =
    { Tag: string
      RegularData: RegularSetDto
      RestPauseData: RestPauseDto }

let getSetsHandler exerciseId : HttpHandler =
    fun next ctx ->
        task {
            let! sets =
                ExerciseId exerciseId
                |> findSetsAsync
                |> Task.map (List.map RegularSetDto.fromDomain)
                |> Task.map List.sort

            return! json sets next ctx
        }

let getSetHandler (exerciseId, setId) : HttpHandler =
    fun next ctx ->
        task {
            match! findSetAsync (ExerciseId exerciseId) (RegularSetId setId) with
            | Some set ->
                let dto = RegularSetDto.fromDomain set
                return! json dto next ctx
            | None -> return! RequestErrors.NOT_FOUND {|  |} next ctx
        }

let postRegularSetHandler exerciseId dto =
    fun next ctx ->
        task {
            match RegularSetDto.toDomain exerciseId dto with
            | Ok set ->
                let! result = insertSetAsync set
                return! json {| id = RegularSetId.toInt set.RegularSetId |} next ctx
            | Error e -> return! RequestErrors.UNPROCESSABLE_ENTITY e next ctx
        }

let postRpSetHandler (dto: RestPauseDto) : HttpHandler =
    fun next ctx ->
        task {
            return! json dto next ctx
        }

let postSetHandler exerciseId : HttpHandler =
    fun next ctx ->
        task {
            let! dto = ctx.BindJsonAsync<PostSetDto>()

            match dto.Range, dto.Reps with
            | Some range, None ->
                let rpDto = { Id = dto.Id; Range = range; Weight = dto.Weight }
                return! postRpSetHandler rpDto next ctx
            | None, Some reps ->
                let rsDto = { Id = dto.Id; Weight = dto.Weight; Reps = reps }
                return! postRegularSetHandler exerciseId rsDto next ctx
            | _ ->
                return! RequestErrors.UNPROCESSABLE_ENTITY "Invalid input" next ctx
        }

let toWlaschinDto dto =
    match dto.Tag with
    | "A" ->
        match box dto.RegularData with
        | null ->
            Error "A data not expected to be null."
        | _ ->
            Ok (A dto.RegularData)
    | "B" ->
        match box dto.RestPauseData with
        | null ->
            Error "B data not expected to be null."
        | _ -> dto.RestPauseData |> B |> Ok
    | _ -> Error $"Tag {dto.Tag} not recognized."

let postWlaschinHandler exerciseId : HttpHandler =
    fun next ctx ->
        task {
            let! dto = ctx.BindJsonAsync<WlaschinDto> ()
            let result = toWlaschinDto dto

            match result with
            | Ok (A regularSetDto) ->
                return! postRegularSetHandler exerciseId regularSetDto next ctx
            | Ok (B restPauseDto) ->
                return! postRpSetHandler restPauseDto next ctx
            | Error e ->
                return! json e next ctx
        }

let updateSetHandler (exerciseId, setId) : HttpHandler =
    fun next ctx ->
        task {
            let! dto = ctx.BindJsonAsync<RegularSetDto>()
            let result = RegularSetDto.toDomain exerciseId dto
            let! set = findSetAsync (ExerciseId exerciseId) (RegularSetId setId)

            match result, set with
            | Ok updatedSet, Some originalSet ->
                let set = RegularSet.update originalSet updatedSet
                Task.ignore (updateSetAsync set) |> ignore
                return! json (RegularSetDto.fromDomain originalSet) next ctx
            | Error e, _ -> return! RequestErrors.UNPROCESSABLE_ENTITY e next ctx
            | _, None -> return! RequestErrors.NOT_FOUND {|  |} next ctx
        }

let deleteSetHandler (exerciseId, setId) : HttpHandler =
    fun next ctx ->
        task {
            match! deleteSetAsync (ExerciseId exerciseId) (RegularSetId setId) with
            | 1 -> return! Successful.NO_CONTENT next ctx
            | _ -> return! RequestErrors.NOT_FOUND "Not found." next ctx
        }

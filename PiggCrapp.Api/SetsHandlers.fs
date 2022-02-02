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

type WidowMakerDto =
    { Id: int
      Weight: double
      TargetReps: int
      ActualReps: int
      CompletionTime: double }

type ExtremeStretchDto =
    { Id: int
      Weight: double
      Time: double }

type PostSet =
    | A of RegularSetDto
    | B of RestPauseDto
    | C of WidowMakerDto
    | D of ExtremeStretchDto

type PostSetDto =
    { Tag: string
      RegularData: RegularSetDto
      RestPauseData: RestPauseDto
      WidowMakerData: WidowMakerDto
      ExtremeStretchData: ExtremeStretchDto }

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

let postRestPauseHandler (dto: RestPauseDto) : HttpHandler =
    fun next ctx -> task { return! json dto next ctx }

let postWidowMakerSetHandler (dto: WidowMakerDto) : HttpHandler =
    fun next ctx -> task { return! json dto next ctx }

let postExtremeStretchHandler (dto: ExtremeStretchDto) : HttpHandler =
    fun next ctx -> task { return! json dto next ctx }

let toSetChoiceDto dto =
    match dto.Tag with
    | "A" ->
        match box dto.RegularData with
        | null -> Error "Please provide data for regular set."
        | _ -> Ok(A dto.RegularData)
    | "B" ->
        match box dto.RestPauseData with
        | null -> Error "Please provide data for rest pause set."
        | _ -> dto.RestPauseData |> B |> Ok
    | "C" ->
        match box dto.WidowMakerData with
        | null -> Error "Please provide rest pause set data."
        | _ -> dto.WidowMakerData |> C |> Ok
    | "D" ->
        match box dto.ExtremeStretchData with
        | null -> Error "Please provide data for extreme stretch set."
        | _ -> dto.ExtremeStretchData |> D |> Ok
    | _ -> Error $"Tag {dto.Tag} not recognized."

let postSetHandler exerciseId : HttpHandler =
    fun next ctx ->
        task {
            let! dto = ctx.BindJsonAsync<PostSetDto>()

            match toSetChoiceDto dto with
            | Ok (A regularSetDto) -> return! postRegularSetHandler exerciseId regularSetDto next ctx
            | Ok (B restPauseDto) -> return! postRestPauseHandler restPauseDto next ctx
            | Ok (C widowMakerDto) -> return! postWidowMakerSetHandler widowMakerDto next ctx
            | Ok (D extremeStretchDto) -> return! postExtremeStretchHandler extremeStretchDto next ctx
            | Error e -> return! json e next ctx
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

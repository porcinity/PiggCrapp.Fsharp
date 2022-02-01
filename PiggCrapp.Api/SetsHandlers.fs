module PiggCrapp.Api.SetsHandlers

open Giraffe
open PiggCrapp.Domain.Ids
open PiggCrapp.Domain.Sets
open PiggCrapp.Storage.Sets
open FSharpPlus
open FsToolkit.ErrorHandling

[<CLIMutable>]
type GetSetDto = { Id: int; Reps: int; Weight: double }

module GetSetDto =
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

[<CLIMutable>]
type RestPauseDto =
    { Id: int
      Range: string
      Weight: double }

type SetDto =
    | GetSetDto of GetSetDto
    | RestPauseDto of RestPauseDto

let getSetsHandler exerciseId : HttpHandler =
    fun next ctx ->
        task {
            let! sets =
                ExerciseId exerciseId
                |> findSetsAsync
                |> Task.map (List.map GetSetDto.fromDomain)
                |> Task.map List.sort

            return! json sets next ctx
        }

let getSetHandler (exerciseId, setId) : HttpHandler =
    fun next ctx ->
        task {
            match! findSetAsync (ExerciseId exerciseId) (RegularSetId setId) with
            | Some set ->
                let dto = GetSetDto.fromDomain set
                return! json dto next ctx
            | None -> return! RequestErrors.NOT_FOUND {|  |} next ctx
        }

let postGetSetHandler exerciseId dto =
    fun next ctx ->
        task {
            match GetSetDto.toDomain exerciseId dto with
            | Ok set ->
                Task.ignore (insertSetAsync set) |> ignore
                return! json {| id = RegularSetId.toInt set.RegularSetId |} next ctx
            | Error e -> return! RequestErrors.UNPROCESSABLE_ENTITY e next ctx
        }

let postRpSetHandler exerciseId dto =
    fun next ctx ->
        task {
            return! text "Not yet implemented" next ctx
        }

let postSetHandler exerciseId : HttpHandler =
    fun next ctx ->
        task {
            let! dto = ctx.BindJsonAsync<SetDto>()
            match dto with
            | GetSetDto gs ->
                return! postGetSetHandler exerciseId gs next ctx
            | RestPauseDto rp ->
                return! postRpSetHandler exerciseId rp next ctx
        }

let updateSetHandler (exerciseId, setId) : HttpHandler =
    fun next ctx ->
        task {
            let! dto = ctx.BindJsonAsync<GetSetDto>()
            let result = GetSetDto.toDomain exerciseId dto
            let! set = findSetAsync (ExerciseId exerciseId) (RegularSetId setId)

            match result, set with
            | Ok updatedSet, Some originalSet ->
                let set = RegularSet.update originalSet updatedSet
                Task.ignore (updateSetAsync set) |> ignore
                return! json (GetSetDto.fromDomain originalSet) next ctx
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

module PiggCrapp.Api.UsersHandlers

open Giraffe
open PiggCrapp.Domain.Users
open PiggCrapp.Domain.Ids
open PiggCrapp.Domain.Measurements
open PiggCrapp.Storage.Users
open FsToolkit.ErrorHandling
open FSharpPlus

type getUserDto =
    { Id: string
      Name: string
      Age: int
      Weight: double
      JoinDate: string }

module getUserDto =
    let fromDomain (user: User) =
        { Id = ShortGuid.fromGuid (UserId.toGuid user.UserId)
          Name = UserName.toString user.Name
          Age = UserAge.toInt user.Age
          Weight = UserWeight.toFloat user.Weight
          JoinDate = user.CreatedDate.ToString("yyyy-MM-dd") }

module PostUserDto =
    type T =
        { Name: string
          Age: int
          Weight: float }

    let toDomain dto =
        validation {
            let! name = UserName.fromString dto.Name
            and! age = UserAge.fromInt dto.Age
            and! weight = dto.Weight * 1.0<lbs> |> UserWeight.create
            return User.create name age weight
        }

let getUsersHandler: HttpHandler =
    fun next ctx ->
        task {
            let! users = findUsersAsync ()
            let dtos = users |> List.map getUserDto.fromDomain
            return! json dtos next ctx
        }

let getUserHandler id : HttpHandler =
    fun next ctx ->
        task {
            let guid = ShortGuid.toGuid id

            match! findUserAsyncResult guid with
            | Some (Ok u) ->
                let dto = getUserDto.fromDomain u
                return! json dto next ctx
            | Some (Error e) ->
                return!
                    ServerErrors.INTERNAL_ERROR
                        {| message = "Invalid data from database."
                           errors = e |}
                        next
                        ctx
            | None -> return! RequestErrors.NOT_FOUND {|  |} next ctx
        }

let postUserHandler: HttpHandler =
    fun next ctx ->
        task {
            let! dto = ctx.BindJsonAsync<PostUserDto.T>()
            match PostUserDto.toDomain dto with
            | Ok user ->
                insertUserAsync user
                |> Async.AwaitTask
                |> Async.RunSynchronously
                |> ignore

                let response = getUserDto.fromDomain user
                return! Successful.CREATED response next ctx
            | Error e -> return! RequestErrors.UNPROCESSABLE_ENTITY {| errors = e |} next ctx
        }

let updateUserHandler userId : HttpHandler =
    fun next ctx ->
        task {
            let! dto = ctx.BindJsonAsync<PostUserDto.T>()
            let updateUser = PostUserDto.toDomain dto
            let! user = userId |> findUserAsync

            match updateUser, user with
            | Ok update, Some user ->
                let updateResult = User.update user update

                updateUserAsync updateResult
                |> Async.AwaitTask
                |> Async.RunSynchronously
                |> ignore

                return! Successful.CREATED {|  |} next ctx
            | Error e, _ -> return! RequestErrors.UNPROCESSABLE_ENTITY e next ctx
            | _, None -> return! RequestErrors.NOT_FOUND "" next ctx
        }

let deleteUserHandler userId : HttpHandler =
    fun next ctx ->
        task {
            match! deleteUserAsync (UserId userId) with
            | 1 -> return! Successful.NO_CONTENT next ctx
            | _ -> return! RequestErrors.NOT_FOUND "" next ctx
        }

module PiggCrapp.Api.Handlers

open System
open Microsoft.AspNetCore.Http
open Giraffe
open PiggCrapp.Domain.Users
open PiggCrapp.Domain.Ids
open PiggCrapp.Domain.Measurements
open PiggCrapp.UserStorage
open FSharpPlus

let apply fResult xResult =
    match fResult,xResult with
    | Ok f, Ok x -> Ok (f x)
    | Error ex, Ok _ -> Error ex
    | Ok _, Error ex -> Error ex
    | Error ex1, Error ex2 -> Error (ex1 @ ex2)

let (<!>) = Result.map
let (<*>) = apply

type getUserDto =
    { Id : Guid
      Name : string
      Age : int
      Weight : double
      JoinDate : string }

module getUserDto =
    let fromDomain (user: User) =
        { Id = UserId.toGuid user.UserId
          Name = UserName.toString user.Name
          Age = UserAge.toInt user.Age
          Weight = UserWeight.toFloat user.Weight
          JoinDate = user.CreatedDate.ToString("yyyy-MM-dd") }
        
module PostUserDto =
    type T =
        { Name : string
          Age : int
          Weight : float }
        
    let toDomain dto =
        let name = UserName.fromString dto.Name
        let age = UserAge.fromInt dto.Age
        let weight = dto.Weight * 1.0<lbs> |> UserWeight.create
        User.create <!> name <*> age <*> weight

let getUsersHandler next ctx = task {
    let! users = findUsersAsync ()
    let dtos =
        users
        |> List.map getUserDto.fromDomain
    return! json dtos next ctx 
}

let getUserHandler id next ctx = task {
    let! user =
        id
        |> findUserAsync
        |> Task.map List.tryHead
    match user with
    | Some u ->
        let dto = getUserDto.fromDomain u
        return! json dto next ctx
    | None ->
        ctx.SetStatusCode 404
        return! json {| message = "No user found with that Id" |} next ctx
}

let postUserHandler next (ctx: HttpContext) = task {
    let! dto = ctx.BindJsonAsync<PostUserDto.T> ()
    match PostUserDto.toDomain dto with
    | Ok user ->
        insertUserAsync user |> Async.AwaitTask |> Async.RunSynchronously |> ignore
        let response = getUserDto.fromDomain user
        ctx.SetStatusCode 201
        return! json response next ctx
    | Error e ->
        return! RequestErrors.UNPROCESSABLE_ENTITY {| errors = e |} next ctx
}

let updateUserHandler userId next (ctx:HttpContext) = task {
    let! dto = ctx.BindJsonAsync<PostUserDto.T> ()
    let! user =
        userId
        |> findUserAsync
        |> Task.map List.tryHead
    match user with
    | Some user ->
        let name = UserName.fromString dto.Name
        let age = UserAge.fromInt dto.Age
        let weight = UserWeight.create <| dto.Weight * 1.0<lbs>
        let result = User.update user <!> name <*> age <*> weight
        match result with
        | Ok resultValue ->
            updateUserAsnc resultValue |> Async.AwaitTask |> Async.RunSynchronously |> ignore
            ctx.SetStatusCode 201
            return! json {||} next ctx
        | Error e ->
            return! RequestErrors.UNPROCESSABLE_ENTITY e next ctx
    | None ->
        ctx.SetStatusCode 404
        return! json {| |} next ctx
}

let deleteUserHandler userId next (ctx: HttpContext) = task {
    match! deleteUserAsync <| UserId userId with
    | 1 ->
        ctx.SetStatusCode 204
        return! json {||} next ctx
    | _ ->
        return! RequestErrors.NOT_FOUND {| message = "no user with that Id" |} next ctx
}
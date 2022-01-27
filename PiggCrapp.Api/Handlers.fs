module PiggCrapp.Api.Handlers

open System
open Microsoft.AspNetCore.Http
open Giraffe
open WorkoutModel
open PiggCrapp.UserStorage
    
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
      Weight : double }

module getUserDto =
    let fromDomain (user: User) =
        { Id = UserId.toGuid user.UserId
          Name = UserName.toString user.Name
          Weight = UserWeight.toFloat user.Weight }
        
module PostUserDto =
    type T =
        { Name : string
          Weight : double }
        
    let toDomain dto =
        { UserId = Guid.NewGuid() |> UserId
          Name = dto.Name |>UserName
          Weight = dto.Weight * 1.0<lbs> |> UserWeight }

let getUsersHandler next ctx = task {
    let! users = findUsersAsync ()
    let dtos =
        users
        |> List.map (fun x -> getUserDto.fromDomain x)
    return! json dtos next ctx 
}

let getUserHandler id next ctx = task {
    let! user =
        id
        |> findUserAsync
    let result = user |> List.head
    let dto = getUserDto.fromDomain result
    return! json dto next ctx
}

let postUserHandler next (ctx: HttpContext) = task {
    let! dto = ctx.BindJsonAsync<PostUserDto.T> ()
    let name = UserName.fromString dto.Name
    let weight = dto.Weight * 1.0<lbs> |> UserWeight.create
    let userResult = User.create <!> name <*> weight
    match userResult with
    | Ok user ->
        let! insert = insertUserAsync user
        ctx.SetStatusCode 201
        return! json insert next ctx
    | Error e ->
        return! RequestErrors.BAD_REQUEST e next ctx
}

let deleteUserHandler userId next (ctx: HttpContext) = task {
    match! deleteUserAsync <| UserId userId with
    | 1 ->
        ctx.SetStatusCode 204
        return! json {||} next ctx
    | _ ->
        return! RequestErrors.BAD_REQUEST {| message = "no user with that Id" |} next ctx     
}
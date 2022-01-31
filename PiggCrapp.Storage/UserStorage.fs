module PiggCrapp.Storage.Users

open System
open FSharpPlus
open FsToolkit.ErrorHandling
open Npgsql.FSharp
open PiggCrapp.Domain.Ids
open PiggCrapp.Domain.Users
open PiggCrapp.Domain.Measurements

let connStr = "Host=localhost;Database=PiggCrapp;Username=pigg"

type dbDto =
    { Id : Guid
      Name : string
      Age : int
      Weight : double
      Created : DateTime }

let toDomain dto = validation {
        let! name = UserName.fromString dto.Name
        and! age = UserAge.fromInt dto.Age
        and! weight = dto.Weight * 1.0<lbs> |> UserWeight.create
        return { UserId = UserId dto.Id
                 Name = name
                 Age = age
                 Weight = weight
                 CreatedDate = dto.Created }
    }

let findUsersAsync () =
    connStr
    |> Sql.connect
    |> Sql.query "select * from users"
    |> Sql.executeAsync (fun read ->
        {
            UserId = read.uuid "user_id" |> UserId
            Name = read.text "user_name" |> UserName
            Age = read.int "user_age" |> UserAge
            Weight = read.double "user_weight" * 1.0<lbs> |> UserWeight
            CreatedDate = read.dateTime "created_date"
        })
    
let findUserAsync userId =
    connStr
    |> Sql.connect
    |> Sql.query "select * from users where user_id = @id"
    |> Sql.parameters [ "@id", Sql.uuid userId ]
    |> Sql.executeAsync (fun read ->
        {
            UserId = read.uuid "user_id" |> UserId
            Name = read.text "user_name" |> UserName
            Age = read.int "user_age" |> UserAge
            Weight = read.double "user_weight" * 1.0<lbs> |> UserWeight
            CreatedDate = read.dateTime "created_date"
        })
    |> Task.map List.tryHead

let findUserAsyncResult userId =
    connStr
    |> Sql.connect
    |> Sql.query "select * from users where user_id = @id"
    |> Sql.parameters [ "@id", Sql.uuid userId ]
    |> Sql.executeAsync (fun read ->
        result {
                let dto =
                    { Id = read.uuid "user_id"
                      Name = read.text "user_name"
                      Age = read.int "user_age"
                      Weight = read.double "user_weight"
                      Created = read.dateTime "created_date" }
                let! resultUser = toDomain dto
                return resultUser
        })
    |> Task.map List.tryHead
    
let insertUserAsync user =
    connStr
    |> Sql.connect
    |> Sql.query "insert into users (user_id, user_name, user_age, user_weight, created_date)
                  values (@id, @name, @age, @weight, @date)"
    |> Sql.parameters [
        "@id", Sql.uuid <| UserId.toGuid user.UserId
        "@name", Sql.text <| UserName.toString user.Name
        "@age", Sql.int <| UserAge.toInt user.Age
        "@weight", Sql.double <| UserWeight.toFloat user.Weight
        "@date", Sql.timestamp <| user.CreatedDate
    ]
    |> Sql.executeNonQueryAsync

let updateUserAsync (user:User) =
    connStr
    |> Sql.connect
    |> Sql.query "update users set
                  user_name = @name, user_age = @age, user_weight = @weight
                  where user_id = @id"
    |> Sql.parameters [
        "@name", Sql.text <| UserName.toString user.Name
        "@age", Sql.int <| UserAge.toInt user.Age
        "@weight", Sql.double <| UserWeight.toFloat user.Weight
        "@id", Sql.uuid <| UserId.toGuid user.UserId
    ]
    |> Sql.executeNonQueryAsync
    
let deleteUserAsync userId =
    connStr
    |> Sql.connect
    |> Sql.query "delete from users where user_id = @id"
    |> Sql.parameters [ "@id", Sql.uuid <| UserId.toGuid userId ]
    |> Sql.executeNonQueryAsync
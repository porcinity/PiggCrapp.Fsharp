module PiggCrapp.UserStorage

open Npgsql.FSharp
open PiggCrapp.Users
open PiggCrapp.Measurements

let connStr = "Host=localhost;Database=PiggCrapp;Username=pigg"

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
    
let deleteUserAsync userId =
    connStr
    |> Sql.connect
    |> Sql.query "delete from users where user_id = @id"
    |> Sql.parameters [ "@id", Sql.uuid <| UserId.toGuid userId ]
    |> Sql.executeNonQueryAsync
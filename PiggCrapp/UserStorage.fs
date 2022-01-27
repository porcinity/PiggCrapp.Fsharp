module PiggCrapp.UserStorage

open Npgsql.FSharp
open WorkoutModel

let connStr = "Host=localhost;Database=PiggCrapp;Username=pigg"

let findUsersAsync =
    connStr
    |> Sql.connect
    |> Sql.query "select * from users"
    |> Sql.executeAsync (fun read ->
        {
            UserId = read.uuid "user_id" |> UserId
            Name = read.text "user_name" |> UserName
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
        })
    
let insertUserAsync user =
    connStr
    |> Sql.connect
    |> Sql.query "insert into users (user_id, user_name)
                  values (@id, @name)"
    |> Sql.parameters [
        "@id", Sql.uuid <| UserId.toGuid user.UserId
        "@name", Sql.text <| UserName.toString user.Name
    ]
    |> Sql.executeNonQueryAsync
    
let deleteUserAsync user =
    connStr
    |> Sql.connect
    |> Sql.query "delete from users where user_id = @id"
    |> Sql.parameters [ "@id", Sql.uuid <| UserId.toGuid user.UserId ]
    |> Sql.executeNonQueryAsync
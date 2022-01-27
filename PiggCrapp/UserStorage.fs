module PiggCrapp.UserStorage

open Npgsql.FSharp
open WorkoutModel

let connStr = "Host=localhost;Database=PiggCrapp;Username=pigg"


let findUsersAsync () =
    connStr
    |> Sql.connect
    |> Sql.query "select * from users"
    |> Sql.executeAsync (fun read ->
        {
            UserId = read.uuid "user_id" |> UserId
            Name = read.text "user_name" |> UserName
            Weight = read.double "user_weight" * 1.0<lbs> |> UserWeight 
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
            Weight = read.double "user_weight" * 1.0<lbs> |> UserWeight
        })
    
let insertUserAsync user =
    connStr
    |> Sql.connect
    |> Sql.query "insert into users (user_id, user_name, user_weight)
                  values (@id, @name, @weight)"
    |> Sql.parameters [
        "@id", Sql.uuid <| UserId.toGuid user.UserId
        "@name", Sql.text <| UserName.toString user.Name
        "@weight", Sql.double <| UserWeight.toFloat user.Weight
    ]
    |> Sql.executeNonQueryAsync
    
let deleteUserAsync userId =
    connStr
    |> Sql.connect
    |> Sql.query "delete from users where user_id = @id"
    |> Sql.parameters [ "@id", Sql.uuid <| UserId.toGuid userId ]
    |> Sql.executeNonQueryAsync
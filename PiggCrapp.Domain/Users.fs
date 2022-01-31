module PiggCrapp.Domain.Users

open System
open System.Text.RegularExpressions
open PiggCrapp.Domain.Measurements
open PiggCrapp.Domain.Ids

type UserName = UserName of string

module UserName =
    let (|Empty|TooLong|SpecialChars|ValidExerciseName|) (str: string) =
        match str with
        | "" -> Empty
        | x when String.length (x) > 50 -> TooLong
        | x when Regex.IsMatch(x, "^[a-zA-Z][a-zA-Z\s]*$") = false -> SpecialChars
        | _ -> ValidExerciseName

    let fromString (name: string) =
        let trim = name.TrimStart(' ').TrimEnd(' ')

        match trim with
        | Empty -> Error [ "User's name can't be blank." ]
        | TooLong -> Error [ "User's name can't be more than 50 characters." ]
        | SpecialChars -> Error [ "User's name can't contain numbers or special chars." ]
        | ValidExerciseName -> UserName trim |> Ok

    let toString (UserName name) = name

type UserWeight = UserWeight of double<lbs>

module UserWeight =
    let (|TooHeavy|TooLight|Good|) n =
        match n with
        | x when x > 400.0<lbs> -> TooHeavy
        | x when x < 80.0<lbs> -> TooLight
        | _ -> Good

    let create num =
        match num with
        | TooHeavy -> Error [ "User's weight must be less than or equal to 400 lbs" ]
        | TooLight -> Error [ "User's weight must be greater than or equal to 80 lbs" ]
        | Good -> Ok <| UserWeight num

    let toFloat (UserWeight num) = num / 1.0<lbs>

type UserAge = UserAge of int

module UserAge =
    let (|TooOld|TooYoung|Good|) age =
        match age with
        | x when x > 100 -> TooOld
        | x when x < 18 -> TooYoung
        | _ -> Good

    let fromInt num =
        match num with
        | TooOld -> Error [ "Age can't be greater than 100." ]
        | TooYoung -> Error [ "Age can't be less than 18." ]
        | Good -> Ok <| UserAge num

    let toInt (UserAge i) = i

type User =
    { UserId: UserId
      Name: UserName
      Age: UserAge
      Weight: UserWeight
      CreatedDate: DateTime }

module User =
    let create name age weight =
        { UserId = Guid.NewGuid() |> UserId
          Name = name
          Age = age
          Weight = weight
          CreatedDate = DateTime.Now }

    let update user updatedUser =
        { user with
              Name = updatedUser.Name
              Age = updatedUser.Age
              Weight = updatedUser.Weight }

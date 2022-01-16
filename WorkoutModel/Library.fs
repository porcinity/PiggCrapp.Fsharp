namespace WorkoutModel

open System
open System.Text.RegularExpressions
    
type WorkoutVariation =
    | UpperA
    | UpperB
    | UpperC
    | LowerA
    | LowerB
    | LowerC

module WorkoutVariation =
    let toString =
        fun variation ->
            match variation with
            | UpperA -> "Upper A"
            | UpperB -> "Upper B"
            | UpperC -> "Upper C"
            | LowerA -> "Lower A"
            | LowerB -> "Lower B"
            | LowerC -> "Lower C"

[<Measure>]
type lbs

type Weight = Weight of int<lbs>

module Weight =
    let (|TooHeavy|TooLight|GoodWeight|) (n: int<lbs>) =
        match n with
        | x when x > 1000<lbs> -> TooHeavy
        | x when x < 1<lbs> -> TooLight
        | _ -> GoodWeight
    
    let fromInt n =
        match n with
        | TooHeavy -> Error [ "Too heavy." ]
        | TooLight -> Error [ "Too light." ]
        | GoodWeight -> Weight n |> Ok

type Reps = Reps of int

module Reps =
    let (|TooMany|TooFew|GoodReps|) n =
        match n with
        | x when x > 100 -> TooMany
        | x when x < 0 -> TooFew
        | _ -> GoodReps
        
    let fromInt n =
        match n with
        | TooMany -> Error [ "Too many reps." ]
        | TooFew -> Error [ "Too few reps." ]
        | GoodReps -> Reps n |> Ok
    
type RegularSet =
    { Weight : Weight
      Reps : Reps }
    
module RegularSet =
    let create =
        fun weight reps ->
            { Weight = weight
              Reps = reps }
    
type RestPauseSet =
    { Weight : Weight
      RestPauseSets : Reps list
    }
    
module RestPauseSet =
    
    let create =
        fun weight repsList ->
            { Weight = weight
              RestPauseSets = repsList }
    
type DropSet = { DropSets : RegularSet list }
    
type Set =
    | Regular of RegularSet
    | RestPause of RestPauseSet
    | DropSet of DropSet

type ExerciseId = ExerciseId of Guid

type ExerciseName = ExerciseName of string

module ExerciseName =    
    let (|Empty|TooLong|SpecialChars|ValidExerciseName|) (str: string) =
        match str with
        | "" -> Empty
        | x when String.length(x) > 50 -> TooLong
        | x when Regex.IsMatch(x, "^[a-zA-Z][a-zA-Z\s]*$") = false -> SpecialChars
        | _ -> ValidExerciseName

    let create (name: string) =
        let trim = name.TrimStart(' ').TrimEnd(' ')
        match trim with
        | Empty -> Error [ "Exercise name can't be blank." ]
        | TooLong -> Error [ "Exercise name can't be more than 50 characters." ]
        | SpecialChars -> Error [ "Exercise name can't contain numbers or special chars." ]
        | ValidExerciseName -> trim |> Ok

type Exercise =
    { ExerciseId : ExerciseId
      Name : ExerciseName
      Sets : Set list }

type WorkoutId = WorkoutId of Guid

type Workout =
    { WorkoutId : WorkoutId
      Date : DateOnly
      Variation : WorkoutVariation
      Exercises : Exercise list }

type UserId = UserId of Guid

type UserName = UserName of string

type User =
    { UserId : UserId
      Name : UserName }

type LogId = LogId of Guid

type WorkoutLog =
    { LogId : LogId
      Owner : User
      Workouts : Workout list }
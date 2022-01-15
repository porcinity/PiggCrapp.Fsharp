namespace WorkoutModel

open System

type Split =
    | Upper
    | Lower
    
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
    {
        Weight : Weight
        Reps : Reps
    }
    
module RegularSet =
    let create =
        fun weight reps ->
            { Weight = weight
              Reps = reps }
    
type RestPauseSet =
    {
        Weight : Weight
        FirstSet : Reps
        SecondSet : Reps
        ThirdSet : Reps
    }
    
type DropSet =
    {
        Drops : RegularSet list
    }
    
type Set =
    | Regular of RegularSet
    | RestPause of RestPauseSet
    | DropSet of DropSet

type ExerciseId = ExerciseId of Guid

type ExerciseName = ExerciseName of string

module ExerciseName =
    let (|Empty|TooLong|BeginWithWhitespace|ValidExerciseName|) string =
        match string with
        | "" -> Empty
        | x when String.length(x) > 50 -> TooLong
        | x when String.IsNullOrWhiteSpace(x) -> BeginWithWhitespace
        | _ -> ValidExerciseName

    let create name =
        match name with
        | Empty -> Error [ "Exercise name can't be blank." ]
        | TooLong -> Error [ "Exercise name can't be more than 50 characters." ]
        | BeginWithWhitespace -> Error [ "Exercise name can't begin with white space." ]
        | ValidExerciseName -> ExerciseName name |> Ok

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
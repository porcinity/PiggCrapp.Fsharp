namespace WorkoutModel

open System

type Split =
    | Upper
    | Lower
    
type SplitVariation =
    | A
    | B
    | C
    
type WorkoutVariation = WorkoutVariation of string

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
    
type Weight = Weight of int

module Weight =
    let (|TooHeavy|TooLight|GoodWeight|) n =
        match n with
        | x when x > 1000 -> TooHeavy
        | x when x < 1 -> TooLight
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
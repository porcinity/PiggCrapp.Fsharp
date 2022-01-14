﻿namespace WorkoutModel

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
    let create =
        fun split variation ->
            match split, variation with
            | Upper, A -> WorkoutVariation "Upper A"
            | Upper, B -> WorkoutVariation "Upper B"
            | Upper, C -> WorkoutVariation "Upper C"
            | Lower, A -> WorkoutVariation "Lower A"
            | Lower, B -> WorkoutVariation "Lower B"
            | Lower, C -> WorkoutVariation "Lower C"
    
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
    
type RegularSet =
    {
        Weight : Weight
        Reps : Reps
    }
    
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
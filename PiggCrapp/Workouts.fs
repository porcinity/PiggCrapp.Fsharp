module PiggCrapp.Workouts

open System
open PiggCrapp.Ids

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
            
    let fromString =
        fun str ->
            match str with
            | "Upper A" -> Ok UpperA
            | "Upper B" -> Ok UpperB
            | "Upper C" -> Ok UpperC
            | "Lower A" -> Ok LowerA
            | "Lower B" -> Ok LowerB
            | "Lower C" -> Ok LowerC
            | _ -> Error [ "Invalid input." ]

type Workout =
    { WorkoutId : WorkoutId
      Date : DateTime
      Variation : WorkoutVariation
      Exercises : Exercise list
      Owner : UserId }
    
module WorkoutId =
    let toGuid (WorkoutId guid) = guid
    
module Workout =
    let create variation owner =
        { WorkoutId = WorkoutId <| Guid.NewGuid()
          Date = DateTime.Now
          Variation = variation
          Exercises = []
          Owner = owner.UserId }
    
    let changeVariation workout variation =
        { workout with Variation = variation }
        
    let changeDate workout date =
        { workout with Date = date }

    let addExercise workout exercise =
        { workout with Exercises = workout.Exercises @ [exercise] }
    
    let removeExercise workout exercise =
        let newList =
            workout.Exercises
            |> List.filter (fun e -> e <> exercise)
        { workout with Exercises = newList }
module PiggCrapp.Domain.Workouts

open System
open PiggCrapp.Domain.Exercises
open PiggCrapp.Domain.Ids

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
            
    // Come back to this. Make function return a Result? Requires large refactoring of ExerciseStorage.
    let fromString =
        fun str ->
            match str with
            | "Upper A" -> UpperA
            | "Upper B" -> UpperB
            | "Upper C" -> UpperC
            | "Lower A" -> LowerA
            | "Lower B" -> LowerB
            | "Lower C" -> LowerC

type Workout =
    { WorkoutId : WorkoutId
      Date : DateTime
      Variation : WorkoutVariation
      Exercises : Exercise list
      Owner : UserId }
    
module WorkoutId =
    let toGuid (WorkoutId guid) = guid
    
module Workout =
    let create variation userId =
        { WorkoutId = WorkoutId <| Guid.NewGuid()
          Date = DateTime.Now
          Variation = variation
          Exercises = []
          Owner = userId }
    
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
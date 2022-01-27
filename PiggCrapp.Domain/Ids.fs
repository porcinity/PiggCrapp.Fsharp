module PiggCrapp.Domain.Ids

open System

type ExerciseId = ExerciseId of Guid

module ExerciseId =
    let toGuid (ExerciseId guid) = guid

type WorkoutId = WorkoutId of Guid

module WorkoutId =
    let toGuid (WorkoutId guid) = guid

type UserId = UserId of Guid

module UserId =
    let toGuid (UserId guid) = guid
    
//type Id =
//    | ExerciseId of Guid
//    | WorkoutId of Guid
//    | UserId of Guid
    
//module Id =
//    let unwrap id =
//        match id with
//        | ExerciseId id -> id
//        | WorkoutId id -> id
//        | UserId id -> id
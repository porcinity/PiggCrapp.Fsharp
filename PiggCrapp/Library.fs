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
            
    let fromString =
        fun str ->
            match str with
            | "Upper A" -> UpperA
            | "Upper B" -> UpperB
            | "Upper C" -> UpperC
            | "Lower A" -> LowerA
            | "Lower B" -> LowerB
            | "Lower C" -> LowerC

//[<Measure>] type lbs

type Weight = Weight of double

module Weight =
    let (|TooHeavy|TooLight|GoodWeight|) (n: double) =
        match n with
        | x when x > 1000.00 -> TooHeavy
        | x when x < 1.00 -> TooLight
        | _ -> GoodWeight
    
    let fromDbl n =
        match n with
        | TooHeavy -> Error [ "Too heavy." ]
        | TooLight -> Error [ "Too light." ]
        | GoodWeight -> Weight n |> Ok
        
    let toDouble (Weight n) = n

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
        
    let toInt (Reps n) = n

type RegularSetId = RegularSetId of Guid

module RegularSetId =
    let toGuid (RegularSetId id) = id
    
type ExerciseId = ExerciseId of Guid
    
type RegularSet =
    { RegularSetId : RegularSetId 
      Weight : Weight
      Reps : Reps
      Exercise : ExerciseId }
    
module RegularSet =
    let create =
        fun exerciseId weight reps ->
            { RegularSetId = RegularSetId <| Guid.NewGuid()
              Weight = weight
              Reps = reps
              Exercise = exerciseId }
    
type RpSetId = RpSetId of Guid
    
type RestPauseSet =
    { RpSetId : RpSetId
      Weight : Weight
      RestPauseSets : Reps list
      Exercise : ExerciseId }
    
module RestPauseSet =
    let create =
        fun weight exerciseId ->
            { RpSetId = RpSetId <| Guid.NewGuid() 
              Weight = weight
              RestPauseSets = []
              Exercise = exerciseId }
    
    let addReps rpSet reps =
        { rpSet with RestPauseSets = rpSet.RestPauseSets @ [reps] }
    
    let removeReps rpSet reps =
        let setMinus =
            rpSet.RestPauseSets
            |> List.filter (fun r -> r <> reps)
        { rpSet with RestPauseSets = setMinus }
    
type DropSetId = DropSetId of Guid
    
type DropSet =
    { DropSetId : DropSetId
      DropSets : RegularSet list
      Exercise : ExerciseId }
    
type Set =
    | Regular of RegularSet
    | RestPause of RestPauseSet
    | DropSet of DropSet
   
type SeatHeight =
    | SeatHeight of int
    
type SeatPosition =
    | SeatPosition of int
    
type SeatSetting =
    | SeatHeight
    | SeatPosition
    | MultiSetting of SeatHeight * SeatPosition
    
type Manufacturer =
    | Hoist
    | Nautilus
    
type PlateLoaded = PlateLoaded 
    
type MachineType =
    | PlateLoaded
    | Selectorized of Manufacturer
    | CableStack
    
type ExerciseNotes =
    { SeatSetting : SeatSetting
      MachineType : MachineType }

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
        
    let toString (ExerciseName name) = name

type WorkoutId = WorkoutId of Guid

module WorkoutId =
    let toGuid (WorkoutId id) = id

module ExerciseId =
    let toGuid (ExerciseId id) = id

type Exercise =
    { ExerciseId : ExerciseId
      Name : ExerciseName
      Notes : ExerciseNotes
      Sets : Set list
      WorkoutId : WorkoutId }
    
module Exercise =
    let create exerciseName exerciseNotes workoutId =
        { ExerciseId = ExerciseId <| Guid.NewGuid()
          Name = exerciseName
          Notes = exerciseNotes
          Sets = []
          WorkoutId = workoutId }
        
    let changeName exercise name =
        { exercise with Name = name }
    
    let addSet exercise set =
        { exercise with Sets = exercise.Sets @ [set] }
        
    let removeSet exercise set =
        let setsWithout =
            exercise.Sets
            |> List.filter (fun x -> x <> set)
        { exercise with Sets = setsWithout }

type UserId = UserId of Guid

module UserId =
    let toGuid (UserId id) = id

type UserName = UserName of string

module UserName =
    let toString (UserName name) = name

type User =
    { UserId : UserId
      Name : UserName }

type Workout =
    { WorkoutId : WorkoutId
      Date : DateTime
      Variation : WorkoutVariation
      Exercises : Exercise list
      Owner : UserId }
    
module Workout =
    let create variation owner =
        let today = DateTime.Now
        { WorkoutId = WorkoutId <| Guid.NewGuid()
          Date = today
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
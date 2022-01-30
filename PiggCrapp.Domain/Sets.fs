module PiggCrapp.Domain.Sets

open System
open PiggCrapp.Domain.Ids

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

type RegularSetId = RegularSetId of int

module RegularSetId =
    let toInt (RegularSetId id) = id
    
type RegularSet =
    { RegularSetId : RegularSetId 
      Weight : Weight
      Reps : Reps
      Exercise : ExerciseId }
    
module RegularSet =
    let create =
        fun exerciseId setId weight reps ->
            { RegularSetId = setId
              Weight = weight
              Reps = reps
              Exercise = exerciseId }

    let update =
        fun set id weight reps ->
            { set with
                RegularSetId = id
                Weight = weight
                Reps = reps }
    
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
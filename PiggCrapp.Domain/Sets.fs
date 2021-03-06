module PiggCrapp.Domain.Sets

open PiggCrapp.Domain.Ids
open PiggCrapp.Domain.Measurements

type Weight = Weight of double<lbs>

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
        | GoodWeight -> Weight (n * 1.0<lbs>) |> Ok

    let toDouble (Weight n) : double = n / 1.0<lbs>

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
    { RegularSetId: RegularSetId
      Weight: Weight
      Reps: Reps
      Exercise: ExerciseId }

module RegularSet =
    let create =
        fun exerciseId setId weight reps ->
            { RegularSetId = setId
              Weight = weight
              Reps = reps
              Exercise = exerciseId }

    let update =
        fun set updatedSet ->
            { set with
                  RegularSetId = updatedSet.RegularSetId
                  Weight = updatedSet.Weight
                  Reps = updatedSet.Reps }

type RpSetId = RpSetId of int

module RpSetId =
    let toInt (RpSetId id) = id

type RestPauseRange =
    | Base
    | Medium
    | High

type RestPauseSet =
    { RpSetId: RpSetId
      Range: RestPauseRange
      Weight: Weight
      RestPauseSets: Reps list
      Exercise: ExerciseId }

module RestPauseSet =
    let create =
        fun id weight range exerciseId ->
            { RpSetId = id
              Range = range
              Weight = weight
              RestPauseSets = []
              Exercise = exerciseId }

    let addReps rpSet reps =
        { rpSet with
              RestPauseSets = rpSet.RestPauseSets @ [ reps ] }

    let removeReps rpSet reps =
        let setMinus =
            rpSet.RestPauseSets
            |> List.filter (fun r -> r <> reps)

        { rpSet with RestPauseSets = setMinus }

type WidowMakerSetId = WidowMakerSetId of int

module WidowMakerSetId =
    let toInt (WidowMakerSetId id) = id

type Time = Time of double<sec>

type WidowMaker =
    { WidowMakerSetId: WidowMakerSetId
      Weight: Weight
      TargetReps: Reps
      ActualReps: Reps
      CompletionTime: Time }

type ExtremeStretchId = ExtremeStretchId of int

module ExtremeStretchId =
    let toInt (ExtremeStretchId id) = id

type ExtremeStretch =
    { ExtremeStretchId: ExtremeStretchId
      Weight: Weight
      Time: Time }

type Set =
    | Regular of RegularSet
    | RestPause of RestPauseSet
    | WidowMaker of WidowMaker
    | ExtremeStretch of ExtremeStretch

module Set =
    let unwrapId set =
        match set with
        | Regular set ->
            RegularSetId.toInt set.RegularSetId
        | RestPause set ->
            RpSetId.toInt set.RpSetId
        | WidowMaker set ->
            WidowMakerSetId.toInt set.WidowMakerSetId
        | ExtremeStretch set ->
            ExtremeStretchId.toInt set.ExtremeStretchId
module PiggCrapp.Domain.Exercises

open System
open System.Text.RegularExpressions
open PiggCrapp.Domain.Ids
open PiggCrapp.Domain.Sets
open PiggCrapp.Domain.ExerciseNotes

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
        | ValidExerciseName -> ExerciseName trim |> Ok
        
    let toString (ExerciseName name) = name
        
type Exercise =
    { ExerciseId : ExerciseId
      Name : ExerciseName
      Notes : ExerciseNotes option
      Sets : Set list
      WorkoutId : WorkoutId }
    
module Exercise =
    let create exerciseName workoutId =
        { ExerciseId = ExerciseId <| Guid.NewGuid()
          Name = exerciseName
          Notes = None
          Sets = []
          WorkoutId = workoutId }
        
    let changeName (exercise: Exercise) name =
        { exercise with Name = name }
    
    let addSet exercise set =
        { exercise with Sets = exercise.Sets @ [set] }
        
    let removeSet exercise set =
        let setsWithout =
            exercise.Sets
            |> List.filter (fun x -> x <> set)
        { exercise with Sets = setsWithout }
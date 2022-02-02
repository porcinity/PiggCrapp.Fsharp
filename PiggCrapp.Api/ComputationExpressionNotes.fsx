// Notes for myself on building a computation expression for the Result type.

module Result =

    // Applicative function
    let apply fResult xResult =
            match fResult, xResult with
            | Ok f, Ok x -> Ok (f x)
            | Error e, Ok _ -> Error e
            | Ok _, Error e -> Error e
            | Error x, Error y -> Error (x @ y)

    let (<*>) = apply

    // Wraps a tuple, collects errors.
    let zip x1 x2 =
        match x1,x2 with
        | Ok x1res, Ok x2res -> Ok (x1res, x2res)
        | Error e, Ok _ -> Error e
        | Ok _, Error e -> Error e
        | Error e, Error w -> Error (e @ w)

    // The zip function using apply.
    let zipApply x y =
        let toTuple x y = (x, y)
        Ok toTuple <*> x <*> y

type ResultBuilder() =

    // return
    // Wrap or "lift" a value.
    member __.Return(x) = Ok x

    // return!
    // Return result as is (no lifting).
    member __.ReturnFrom(x) = x

    // let!
    // Monadic bind function; unwraps the parameter.
    member __.Bind(res, binder) = Result.bind binder res

    // let!
    // Wrap a function.
    member __.BindReturn(x, f) = Result.map f x

    // and!
    // "Apply" function, with implementation left to "zip" function.
    member _.MergeSources(t1: Result<'T,'U>, t2: Result<'T1,'U>) = Result.zip t1 t2


// Recipes:
//    Bind + Return = Monadic Workflow
//    Bind + MergeSources + Return = Applicative Workflow.
//    BindReturn + MergeSources + Return = Applicative Workflow.

let result = ResultBuilder()

let validate str =
    match str with
    | "" -> Error [ "Can't be blank" ]
    | _ -> Ok str

let combineStrings (str:string) (str2:string) (str3:string) = $"[String #1: {str}] [String #2: {str2}] [String #3: {str3}]"

let resultTest x y z =
    result {
        let! str1 = validate x
        and! str2 = validate y
        and! str3 = validate z
        return combineStrings str1 str2 str3
    }

let valNum num =
    match num with
    | x when x < 1 -> Error [ "Too small" ]
    | x when x > 10 -> Error [ "Too big" ]
    | _ -> Ok num

let addition (x:int) (y:int) =
    result {
        let! first = valNum x
        let! second = valNum (first + y)

        return second
    }

type Name = Name of string

type Age = Age of int

type Person =
    {
      Name: Name
      Age: Age }

module Person =
    let basePerson =
        { Name = Name "New User"
          Age = Age 20 }

type PersonBuilder() =
    member _.Zero _ = Person.basePerson

    member _.Yield _ = Person.basePerson

    [<CustomOperation("name")>]
    member _.Name(person, name) = { person with Name = Name name }

    [<CustomOperation("age")>]
    member _.Age(person, age) = { person with Age = Age age }

let person = PersonBuilder()

let newPerson str int =
    person {
        name str
        age int
    }
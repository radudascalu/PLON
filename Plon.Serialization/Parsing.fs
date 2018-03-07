// Implementation based on https://fsharpforfunandprofit.com/posts/understanding-parser-combinators-4/.
module Parsing

type Result<'a> =
    | Success of 'a
    | Failure of string

type Parser<'t> = Parser of (string -> Result<'t * string>)

let parseChar charToMatch =
    let innerFn str = 
        if System.String.IsNullOrEmpty(str) then
            Failure "Empty string."
        else 
            let first = str.[0]
            if first = charToMatch then
                Success (charToMatch, str)
            else 
                Failure (sprintf "Expected %c, but received %c." charToMatch first)
    Parser innerFn

let parseA = parseChar 'A'

let run parser input =
    let (Parser innerFn) = parser
    innerFn input

let andThen parser1 parser2 =
    let innerFn input = 
        let result1 = run parser1 input
        match result1 with
        | Failure message -> Failure message
        | Success (value, remaining) ->
            let result2 = run parser2 remaining
            match result2 with
            | Failure message -> Failure message
            | Success (value2, remaining2) ->
                Success ((value, value2), remaining2)
    Parser innerFn

let (.>>.) = andThen

let orElse parser1 parser2 =
    let innerFn input =
        let result1 = run parser1 input
        match result1 with
        | Failure message ->
            let result2 = run parser2 input
            match result2 with
            | Failure message -> Failure message
            | Success (value, remaining) ->
                Success (value, remaining)
        | Success (value, remaining) ->
            Success (value, remaining)
    Parser innerFn

let (<|>) = orElse

let choice parsers =
    List.reduce (<|>) parsers

let anyOf listOfChars =
    listOfChars
    |> List.map parseChar
    |> choice

let mapParser fn parser =
    let innerFn input =
        let result = run parser input
        match result with
        | Success (value, remaining) ->
            Success (fn value, remaining)
        | Failure error ->
            Failure error
    Parser innerFn

let (<!>) = mapParser

let (|>>) x f = mapParser f x

let returnParser value =
    let innerFn input =
        Success (value, input)
    Parser innerFn


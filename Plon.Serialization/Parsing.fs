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

let returnP value =
    let innerFn input =
        Success (value, input)
    Parser innerFn

let applyP fP xP =
    fP .>>. xP
    |> mapParser (fun (f, x) -> f x)
    
let (<*>) = applyP

let lift2 f xP yP =
    returnP f <*> xP <*> yP

let rec sequence parserList =
    let cons head tail = head :: tail
    let consP = lift2 cons
    match parserList with
    | [] ->
        returnP []
    | head :: tail ->
        consP head (sequence tail)
        
let parseString str =
    str
    |> List.ofSeq
    |> List.map parseChar
    |> sequence    
    |> mapParser (fun x -> System.String(List.toArray x))     
        
let rec parseZeroOrMore parser input =
    let result = run parser input
    match result with
    | Failure errorMessage ->
        ([], input)
    | Success (parsed, inputAfterParse) ->
        let (nextParsed, nextInputAfterParse) = parseZeroOrMore parser inputAfterParse
        ((parsed :: nextParsed), nextInputAfterParse) 
        
let many parser =
    let innerFn input =
        let (parsed, remainingInput) = parseZeroOrMore parser input
        Success (parsed, remainingInput)
    Parser innerFn
    
let oneOrMore parser =
    let innerFn input =
        let result = run parser input
        match result with 
        | Failure errorMessage ->
            Failure errorMessage
        | Success (parsed, inputAfterParse) ->
            let (nextParsed, nextInputAfterParse) = parseZeroOrMore parser inputAfterParse
            Success (parsed :: nextParsed, nextInputAfterParse)
    Parser innerFn
    

// Implementation based on https://fsharpforfunandprofit.com/series/understanding-parser-combinators.html.
module Plon.Serialization.Parsing.Combinators

type ParserLabel = string
type ParserError = string

type Result<'a> =
    | Success of 'a
    | Failure of ParserLabel * ParserError

type Parser<'t> = {
    parseFn: (string -> Result<'t * string>)
    label: ParserLabel
}

let satisfy predicate label =
    let innerFn input = 
        if System.String.IsNullOrEmpty(input) then
            Failure (label, "Empty string.")
        else 
            let first = input.[0]
            if predicate first then
                Success (first, input.[1..])
            else 
                Failure (label, (sprintf "Unexpected %c" first))
    { parseFn = innerFn; label = label }

let run parser input =
    parser.parseFn input

let setLabel parser newLabel = 
    let innerFn input =
        let result = parser.parseFn input
        match result with
        | Success s ->
            Success s
        | Failure (label, error) ->
            Failure (newLabel, error)
    { parseFn = innerFn; label = newLabel }

let getLabel parser =
    parser.label

let (<?>) = setLabel

let printResult result = 
    match result with
    | Success (value, input) ->
        printfn "%A" value
    | Failure (label, error) ->
        printfn "Error parsing %s\n%s" label error

let bindP fn parser =
    let label = "unknown"
    let innerFn input =
        let result = run parser input
        match result with
        | Failure (label, error) ->
            Failure (label, error)
        | Success (parsed, remainingInput) ->
            let parser2 = fn parsed
            run parser2 remainingInput
    { parseFn = innerFn; label = label }

let (>>=) parser fn = bindP fn parser

let returnP value =
    let innerFn input =
        Success (value, input)
    { parseFn = innerFn; label = "unknown" }

let andThen parser1 parser2 =
    let label = sprintf "%s and then %s" (getLabel parser1) (getLabel parser2)
    parser1 >>= (fun result1 ->
    parser2 >>= (fun result2 ->
        returnP (result1, result2)))
    <?> label

let (.>>.) = andThen

let orElse parser1 parser2 =
    let label = sprintf "%s or else %s" (getLabel parser1) (getLabel parser2)
    let innerFn input =
        let result1 = run parser1 input
        match result1 with
        | Failure (label, error)->
            let result2 = run parser2 input
            match result2 with
            | Failure (label, error) -> Failure (label, error)
            | Success (value, remaining) ->
                Success (value, remaining)
        | Success (value, remaining) ->
            Success (value, remaining)
    { parseFn = innerFn; label = label }

let (<|>) = orElse

let choice parsers =
    List.reduce (<|>) parsers

let mapParser fn parser =
    let label = "unknown"
    let innerFn input =
        let result = run parser input
        match result with
        | Success (value, remaining) ->
            Success (fn value, remaining)
        | Failure (label, error) ->
            Failure (label, error)
    { parseFn = innerFn; label = label }

let (<!>) = mapParser

let (|>>) x f = mapParser f x

let applyP fP xP =
    fP >>= (fun f ->
    xP >>= (fun x ->
        returnP (f x)))
    
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
        
let rec parseZeroOrMore parser input =
    let result = run parser input
    match result with
    | Failure (label, error) ->
        ([], input)
    | Success (parsed, inputAfterParse) ->
        let (nextParsed, nextInputAfterParse) = parseZeroOrMore parser inputAfterParse
        ((parsed :: nextParsed), nextInputAfterParse) 
        
let many parser =
    let innerFn input =
        let (parsed, remainingInput) = parseZeroOrMore parser input
        Success (parsed, remainingInput)
    { parseFn = innerFn; label = "unknown" }
    
let oneOrMore parser =
    parser >>= (fun head ->
    many parser >>= (fun tail ->
        returnP (head :: tail)))
    
let opt parser =
    let some = parser |>> Some
    let none = returnP None
    some <|> none

let (>>.) parser1 parser2 =
    parser1 .>>. parser2
    |>> (fun (a, b) -> b)

let (.>>) parser1 parser2 =
    parser1 .>>. parser2
    |>> (fun (a, b) -> a)

let between parser1 parser2 parser3 =
    parser1 >>. parser2 .>> parser3

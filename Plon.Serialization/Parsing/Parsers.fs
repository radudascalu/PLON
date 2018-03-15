// Implementation based on https://fsharpforfunandprofit.com/series/understanding-parser-combinators.html.
module Plon.Serialization.Parsing.Parsers

open Combinators
open System

/// Parses a char.
let parseChar charToMatch =
    let predicate = (fun chr -> chr = charToMatch)
    let label = sprintf "%c" charToMatch
    satisfy predicate label

/// Chooses any of a list of chars.
let anyOf listOfChars =
    let label = sprintf "%A" listOfChars
    listOfChars
    |> List.map parseChar
    |> choice
    <?> label

/// Converts a char list to a string.
let charListToString charList =
    String(List.toArray charList)

/// Parses a sequence of zero or more characters.
let zeroOrMoreChars charParser =
    many charParser
    |>> charListToString
    
/// Parses a sequence of one or more characters.
let oneOrMoreChars charParser =
    oneOrMore charParser
    |>> charListToString
    
/// Parses a string.
let parseString str =
    let label = str
    str 
    |> List.ofSeq
    |> List.map parseChar
    |> sequence
    |>> charListToString 
    <?> label
     
/// Parses a digit.
let parseDigit =
    let predicate = Char.IsDigit
    let label = "digit"
    satisfy predicate label

/// Parses an integer.
let parseInt =
    let resultToInt (sign, valueString) =
        let value = valueString |> int
        match sign with
        | Some _ -> -value
        | None -> value
    
    let label = "integer"
    let digits = oneOrMoreChars parseDigit
    let sign = opt (parseChar '-')
    
    sign .>>. digits
    |>> resultToInt
    <?> label
    
/// Parses a float.
let parseFloat = 
    let resultToFloat (((sign, beforeDot), dot), afterDot) =
        let value = sprintf "%s.%s" beforeDot afterDot |> float
        match sign with
        | Some _ -> -value
        | None -> value
    
    let label = "float"
    let digits = oneOrMoreChars parseDigit
    let dot = opt (parseChar '.')
    let sign = opt (parseChar '-')
    
    sign .>>. digits .>>. dot .>>. digits
    |>> resultToFloat
    <?> label
    
/// Parses a list with one or more elementsB.
let parseOneOrMoreList elementParser separator =
    let separatorParser = parseChar separator
    let separatorAndThenElementParser = separatorParser >>. elementParser
    elementParser .>>. (many separatorAndThenElementParser)
    |>> (fun (p1, pRest) -> p1 :: pRest)

/// Parses a list with zero or more elements. 
let parseList elementParser separator = 
    parseOneOrMoreList elementParser separator <|> returnP []

    
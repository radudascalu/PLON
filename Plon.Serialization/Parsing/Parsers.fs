// Implementation based on https://fsharpforfunandprofit.com/series/understanding-parser-combinators.html.
module Plon.Serialization.Parsing.Parsers

open Combinators
open System
open Plon.Serialization.Common

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
     
/// Parses a list with one or more elements.
let parseOneOrMoreList elementParser separator =
    let separatorParser = parseChar separator
    let separatorAndThenElementParser = separatorParser >>. elementParser
    elementParser .>>. (many separatorAndThenElementParser)
    |>> (fun (p1, pRest) -> p1 :: pRest)

/// Parses a list with zero or more elements. 
let parseList elementParser separator = 
    parseOneOrMoreList elementParser separator <|> returnP []

/// Applies the parser p, ignores the result and returns x.
let (>>%) p x =
    p |>> (fun _ -> x)

let pNull =
    parseString "null"
    >>% PNull
    <?> "null"

let pBool =
    let pTrue =
        parseString "true"
        >>% PBoolean true
    let pFalse =
        parseString "false"
        >>% PBoolean false
    pTrue <|> pFalse
    <?> "bool"

let pUnescapedChar =
    let label = "char"
    satisfy (fun ch -> ch <> '\\' && ch <> '"') label

let pEscapedChar =
    [
    ("\\\"", '\"')
    ("\\\\", '\\')
    ("\\\/", '/')
    ("\\b", '\b')
    ("\\f", '\f')
    ("\\n", '\n')
    ("\\r", '\r')
    ("\\t", '\t')
    ]
    |> List.map (fun (toMatch, result) -> parseString toMatch >>% result)
    |> choice
    <?> "escaped char"

let pUnicodeChar = 
    let backslash = parseChar '\\'
    let uChar = parseChar 'u'
    let hexdigit = anyOf (['0'..'9'] @ ['a'..'f'] @ ['A'..'F'])

    let convertToChar (((h1, h2), h3), h4) =
        let str = sprintf "%c%c%c%c" h1 h2 h3 h4
        Int32.Parse(str, Globalization.NumberStyles.HexNumber) |> char

    backslash >>. uChar >>. hexdigit .>>. hexdigit .>>. hexdigit .>>. hexdigit
    |>> convertToChar

let pString = 
    let quoteChar = parseChar '\"' <?> "quote"
    let pChar = pUnescapedChar <|> pEscapedChar <|> pUnicodeChar

    quoteChar >>. zeroOrMoreChars pChar .>> quoteChar
    |>> PString
    <?> "quoted string"

let pNumber =
    let minus = parseString "-"
    let zero = parseString "0"
    let digitOneToNine = satisfy (fun chr -> chr > '0' && chr <= '9') "1-9"
    let digit = satisfy Char.IsDigit "digit"
    let positiveNonZeroInt = 
        digitOneToNine .>>. zeroOrMoreChars digit
        |>> fun (first, rest) -> string first + rest
    let intPart = zero <|> positiveNonZeroInt
    
    let point = parseString "."
    let fractionPart = 
        point .>>. oneOrMoreChars digit
        |>> fun (point, digits) -> point + digits

    let e = parseChar 'e' <|> parseChar 'E'
    let optPlusOrMinus = opt (parseString "-" <|> parseString "+")
    let exponentPart = 
        e >>. optPlusOrMinus .>>. oneOrMoreChars digit
        |>> fun (sign, digits) -> "e" + (defaultArg sign "") + digits

    let convertToJNumber (((sign, intPart), fractionPart), exponentPart) =
        let signStr = defaultArg sign ""
        let fractionStr = defaultArg fractionPart ""
        let exponentStr = defaultArg exponentPart ""

        signStr + intPart + fractionStr + exponentStr 
        |> decimal
        |> PNumber

    opt minus .>>. intPart .>>. opt fractionPart .>>. opt exponentPart
    |>> convertToJNumber
    <?> "number"

let pValue, pValueRef = createParserForwardedToRef<PValue>()

let pArray = 
    let left = parseChar '['
    let right = parseChar ']'
    let separator = ','
    
    let values = parseList pValue separator

    between left values right
    |>> PArray
    <?> "array"

let pObject =
    let left = parseChar '{'
    let right = parseChar '}'
    let separator = ','

    let propertiesValues = parseList pValue separator
    
    between left propertiesValues right
    |>> PObject
    <?> "object" 

pValueRef := choice
    [
    pNull
    pNumber
    pBool
    pString
    pArray
    pObject
    ]


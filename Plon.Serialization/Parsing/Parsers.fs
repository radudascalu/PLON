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

let PlonNull =
    parseString "null"
    >>% PlonNull
    <?> "null"

let pBool =
    let pTrue =
        parseString "true"
        >>% PlonBoolean true
    let pFalse =
        parseString "false"
        >>% PlonBoolean false
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

let PlonString = 
    let quoteChar = parseChar '\"' <?> "quote"
    let pChar = pUnescapedChar <|> pEscapedChar <|> pUnicodeChar

    quoteChar >>. zeroOrMoreChars pChar .>> quoteChar
    |>> PlonString
    <?> "quoted string"

let PlonNumber =
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
        |> PlonNumber

    opt minus .>>. intPart .>>. opt fractionPart .>>. opt exponentPart
    |>> convertToJNumber
    <?> "number"

let PlonValue, PlonValueRef = createParserForwardedToRef<PlonValue>()

let PlonArray = 
    let left = parseChar '['
    let right = parseChar ']'
    let separator = ','
    
    let values = parseList PlonValue separator

    between left values right
    |>> PlonArray
    <?> "array"

let PlonObject =
    let left = parseChar '{'
    let right = parseChar '}'
    let separator = ','

    let propertiesValues = parseList PlonValue separator
    
    between left propertiesValues right
    |>> PlonObject
    <?> "object" 

PlonValueRef := choice
    [
    PlonNull
    PlonNumber
    pBool
    PlonString
    PlonArray
    PlonObject
    ]

let getPlonString =
    function 
    | PlonString str -> str
    | _ -> failwith "TODO"

let getMetadataType (str: string) =
    let reversedInput = String(str.ToCharArray() |> Array.rev)
    // TODO: Handle failure
    let (lst, rest) = parseZeroOrMore (parseString "[]") reversedInput
    let mutable metadataType = 
        match String(rest.ToCharArray() |> Array.rev) with
        | "bool" -> MetadataBool
        | "string" -> MetadataString
        | "number" -> MetadataNumber
        | customType -> MetadataObject customType
    lst
    |> List.iter (fun _ -> metadataType <- MetadataArray metadataType)
    metadataType

let parseType =
    PlonString
    |>> (fun str -> (getPlonString >> getMetadataType) str)

let parseTypeProperty = 
    parseString "{\"name\":"
    >>. PlonString
    .>> parseString ",\"type\":"
    .>>. parseType
    .>> parseChar '}'
    |>> (fun (propName, propType) -> { Name = getPlonString propName; Type = propType })

let parseTypeProperties = 
    parseChar '['
    >>. parseList parseTypeProperty ','
    .>> parseChar ']'

let parseMetadataType = 
    parseString "{\"name\":"
    >>. PlonString
    .>> parseString ",\"properties\":"
    .>>. parseTypeProperties
    .>> parseChar '}'
    |>> (fun (typeName, props) -> { Name = getPlonString typeName; Properties = props })

let parseMetadataTypes = 
    parseChar '[' 
    >>. parseList parseMetadataType ','
    .>> parseString "]"

let parseMetadata =
    parseString "\"types\":"
    >>. parseMetadataTypes
    .>> parseString ",\"rootType\":"
    .>>. parseType
    |>> (fun (types, rootType) -> { Types = types; RootType = rootType })
    <?> "metadata"

let parseValue = 
    parseString "\"value\":"
    >>. PlonValue
    <?> "value"

let parsePlonObject = 
    parseChar '{' 
    >>. parseMetadata
    .>> parseChar ','
    .>>. parseValue
    .>> parseChar '}'
    <?> "PLON object"
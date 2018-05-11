module Plon.Serialization.Deserializer

open Plon.Serialization.Parsing.Parsers
open Plon.Serialization.Parsing.Combinators

let deserialize<'a> plonStr =
    let parseResult = Plon.Serialization.Parsing.Combinators.run Plon.Serialization.Parsing.Parsers.parsePlonObject plonStr
    match parseResult with
    | Success ((metadata, value), rest) ->
        if (rest.Length > 0) then
            failwith "TODO"
        else
            Plon.Serialization.Value.Deserializer.deserialize<'a> value metadata
    | Failure (label, error) ->
        failwith "TODO"
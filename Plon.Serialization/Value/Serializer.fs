module Plon.Serialization.Value.Serializer

open Plon.Serialization.Common
open System.Globalization
open System.Collections
open System

let serializeNull =
    "null"
    
let serializeString str =
        "\"" + str + "\""

let serializeNumber (number: decimal) =
    System.String.Format(CultureInfo.InvariantCulture, "{0}", number) 

let serializeBool bool =
    if bool then "true" else "false"

let serializeArray serializeFn array =
    "[" +
    (array 
    |> Seq.map serializeFn
    |> String.concat ",") +
    "]"

let serializeObject serializeFn obj =   
    "{" +
    (obj 
    |> Seq.map serializeFn
    |> String.concat ",") +
    "}"

let rec serializePlonObj plonObj =
    match plonObj with
    | PlonString str -> serializeString str
    | PlonNumber num -> serializeNumber num
    | PlonBoolean bool -> serializeBool bool
    | PlonNull -> serializeNull
    | PlonArray arr -> serializeArray serializePlonObj arr
    | PlonObject obj -> serializeObject serializePlonObj obj
    
let rec objToPlonObj (obj: obj) =
    if obj = null then
        PlonNull
    else      
        match obj with
        | :? string as str -> PlonString str
        | :? bool as bool -> PlonBoolean bool
        | :? uint8 as num -> PlonNumber (num |> decimal)
        | :? uint16 as num -> PlonNumber (num |> decimal)
        | :? uint32 as num -> PlonNumber (num |> decimal)
        | :? uint64 as num -> PlonNumber (num |> decimal)
        | :? int8 as num -> PlonNumber (num |> decimal)
        | :? int16 as num -> PlonNumber (num |> decimal)
        | :? int32 as num -> PlonNumber (num |> decimal)
        | :? int64 as num -> PlonNumber (num |> decimal)
        | :? double as num -> PlonNumber (num |> decimal)
        | :? float32 as num -> PlonNumber (num |> decimal)
        | :? decimal as num -> PlonNumber (num |> decimal)
        | :? DateTime as dateTime -> PlonString (dateTime.ToString())
        | _ -> 
            if typeof<IEnumerable>.IsAssignableFrom(obj.GetType()) then           
                obj 
                :?> IEnumerable
                |> Seq.cast<obj>
                |> Seq.map objToPlonObj
                |> Seq.toList
                |> PlonArray
            else
                obj.GetType().GetProperties()
                |> Array.map (fun prop -> prop.GetValue(obj))
                |> Array.map objToPlonObj
                |> Seq.toList
                |> PlonObject      
        
let serialize obj =
    obj
    |> objToPlonObj
    |> serializePlonObj

module Plon.Serialization.Serializer

open Plon.Serialization.Common
open System.Globalization

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
    | PString str -> serializeString str
    | PNumber num -> serializeNumber num
    | PBoolean bool -> serializeBool bool
    | PNull -> serializeNull
    | PArray arr -> serializeArray serializePlonObj arr
    | PObject obj -> serializeObject serializePlonObj obj
    
let rec objToPlonObj (obj: obj) =
    if obj = null then
        PNull
    else      
        match obj with
        | :? string as str -> PString str
        | :? bool as bool -> PBoolean bool
        | :? uint8 as num -> PNumber (num |> decimal)
        | :? uint16 as num -> PNumber (num |> decimal)
        | :? uint32 as num -> PNumber (num |> decimal)
        | :? uint64 as num -> PNumber (num |> decimal)
        | :? int8 as num -> PNumber (num |> decimal)
        | :? int16 as num -> PNumber (num |> decimal)
        | :? int32 as num -> PNumber (num |> decimal)
        | :? int64 as num -> PNumber (num |> decimal)
        | :? double as num -> PNumber (num |> decimal)
        | :? float32 as num -> PNumber (num |> decimal)
        | :? decimal as num -> PNumber (num |> decimal)
        | :? array<obj> as arr -> 
            let values = arr |> Array.map objToPlonObj
            PArray values
        | _ -> 
            let propertiesValues = 
                obj.GetType().GetProperties()
                |> Array.map (fun prop -> prop.GetValue(obj))
                |> Array.map objToPlonObj
            PArray propertiesValues      
        
let serialize obj =
    obj
    |> objToPlonObj
    |> serializePlonObj

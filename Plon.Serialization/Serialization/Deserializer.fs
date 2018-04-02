module Plon.Serialization.Deserializer

open Plon.Serialization
open Metadata
open Common
open System
open System.Collections.Generic

let deserializeString obj instance =
    let value = 
        match obj with
        | PValue.PString str -> str
        | PValue.PNull -> null
        | _ -> raise (Exception("Expected string."))
    match box instance with 
    | :? string -> value |> box
    | :? DateTime -> if (value = null) then null else DateTime.Parse(value) |> box
    | _ -> raise (Exception("Expected string."))
    
let deserializeNumber obj instance =
    let value = 
        match obj with
        | PValue.PNumber num -> num
        | _ -> raise (Exception("Expected number."))
    match box instance with 
    | :? uint8 as num -> box value
    | :? uint16 as num -> box value
    | :? uint32 as num -> box value
    | :? uint64 as num -> box value
    | :? int8 as num -> box value
    | :? int16 as num -> box value
    | :? int32 as num -> box value
    | :? int64 as num -> box value
    | :? double as num -> box value
    | :? float32 as num -> box value
    | :? decimal as num -> box value
    | _ -> raise (Exception("Expected number."))

let deserializeBool obj instance =
    let value = 
        match obj with
        | PValue.PBoolean bool -> bool
        | _ -> raise (Exception("Expected bool."))
    match box instance with
    | :? bool -> value |> box
    | _ -> raise(Exception("Expected bool."))

let deserializeObjectProperty instance value (propertyMetadata: MetadataObjectProperty) types deserializeFn =
    let property = instance.GetType().GetProperty(propertyMetadata.Name)
    let propertyInstance = Activator.CreateInstance(property.PropertyType)
    let propertyValue = deserializeFn value propertyInstance propertyMetadata.Type types 
    property.SetValue(instance, propertyValue)

let deserializeObject obj instance (objType: string) (types: IDictionary<MetadataTypeName, MetadataObjectProperty list>) deserializeFn =
    let values = 
        match obj with
        | PValue.PObject values -> Some values
        | PValue.PNull -> None
        | _ -> raise (Exception("Expected object."))
    match values with 
    | Some values ->
        let properties = types.[objType]
        List.map2 (fun p v -> deserializeObjectProperty instance v p types deserializeFn) properties values
        |> ignore
        instance
    | None -> null
 
let deserializeArray obj instance metadataType types deserializeFn =
    let arr = 
        match obj with
        | PValue.PArray arr -> Some arr
        | PValue.PNull -> None
        | _ -> raise (Exception("Expected array."))
    match arr with 
    | Some arr ->
        match getCollectionType (instance.GetType()) with
        | None -> raise (Exception("Expected collection."))
        | Some elementType -> 
            arr
            |> List.map (fun elem -> deserializeFn elem (Activator.CreateInstance(elementType)) metadataType types)
            |> List.toSeq
            |> box
    | None -> null

let rec deserializeInternal (obj: PValue) instance metadataType types =  
    match metadataType with
    | MetadataType.MetadataString -> 
        deserializeString obj instance
    | MetadataType.MetadataNumber -> 
        deserializeNumber obj instance
    | MetadataType.MetadataBool -> 
        deserializeBool obj instance
    | MetadataType.MetadataArray arrayType -> 
        deserializeArray obj instance arrayType types deserializeInternal
    | MetadataType.MetadataObject objType ->
        deserializeObject obj instance objType types deserializeInternal

let deserialize<'a> obj (metadata: PlonMetadata) =
    let types = 
        metadata.Types
        |> List.map (fun t -> t.Name, t.Properties)
        |> dict
    let instance = Activator.CreateInstance(typeof<'a>) |> box
    deserializeInternal obj instance metadata.RootType types :?> 'a

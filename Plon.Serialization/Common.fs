module Plon.Serialization.Common

open System.Collections.Generic

type PlonValue = 
    | PlonString of string
    | PlonNumber of decimal
    | PlonObject of PlonValue list
    | PlonArray of PlonValue list
    | PlonBoolean of bool
    | PlonNull
    
type MetadataTypeName = string
type MetadataPropertyName = string

type MetadataType = 
    | MetadataBool
    | MetadataString
    | MetadataNumber
    | MetadataArray of MetadataType
    | MetadataObject of MetadataTypeName

type MetadataObjectProperty = {
    Name: MetadataPropertyName
    Type: MetadataType
}

type MetadataTypeDefinition = {
    Name: MetadataTypeName
    Properties: MetadataObjectProperty list
}

type PlonMetadata = {
    Types: MetadataTypeDefinition list
    RootType: MetadataType
}

type PlonObject = 
    PlonMetadata *
    PlonValue
    
let getCollectionType (modelType: System.Type) =
    if modelType.IsGenericType then
        let types = modelType.GetGenericArguments();
        match types.Length with
        | 1 -> Some types.[0]
        | _ -> modelType.GetInterfaces()
               |> Array.filter (fun t -> t.IsGenericType)
               |> Array.tryFind (fun t -> t.GetGenericTypeDefinition() = typeof<IEnumerable<_>>)
               |> Option.map (fun t -> t.GetGenericTypeDefinition())
    else if modelType.IsArray then
        Some (modelType.GetElementType())
    else 
        None

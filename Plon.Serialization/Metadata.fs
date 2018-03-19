module Plon.Serialization.Metadata

open System.Collections.Generic
open System.Collections
open System

type MetadataType = 
    | MetadataBool
    | MetadataString
    | MetadataNumber
    | MetadataArray of MetadataType
    | MetadataObject of MetadataPropertyElement[]
and MetadataPropertyElement = {
    Type: MetadataType
    Name: string
}

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

let rec getPlonMetadata (modelType: System.Type) =
    if modelType = typeof<string> then MetadataString
    else if modelType = typeof<bool> then MetadataBool
    else if modelType = typeof<uint8> then MetadataNumber
    else if modelType = typeof<uint16> then MetadataNumber
    else if modelType = typeof<uint32> then MetadataNumber
    else if modelType = typeof<uint64> then MetadataNumber
    else if modelType = typeof<int8> then MetadataNumber
    else if modelType = typeof<int16> then MetadataNumber
    else if modelType = typeof<int32> then MetadataNumber
    else if modelType = typeof<int64> then MetadataNumber
    else if modelType = typeof<double> then MetadataNumber
    else if modelType = typeof<float32> then MetadataNumber
    else if modelType = typeof<decimal> then MetadataNumber
    else if modelType = typeof<DateTime> then MetadataString
    else if typeof<IEnumerable>.IsAssignableFrom(modelType) then           
            let collectionType = getCollectionType modelType
            match collectionType with
            | Some x -> (getPlonMetadata >> MetadataArray) x
            | None -> raise (Exception())
        else
            modelType.GetProperties()
            |> Array.map (fun prop -> 
                { Name = prop.Name;
                  Type = prop.GetType() |> getPlonMetadata })
            |> MetadataObject

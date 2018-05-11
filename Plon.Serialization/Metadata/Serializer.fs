module Plon.Serialization.Metadata.Serializer

open System.Collections.Generic
open System.Collections
open System
open Plon.Serialization.Common

// TODO: Handle System.Object
let rec extractPlonMetadata modelType (definedTypes: Dictionary<MetadataTypeName, MetadataObjectProperty list>) =
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
            | Some x -> MetadataArray (extractPlonMetadata x definedTypes)
            | None -> raise (Exception())
        else
            let typeName = modelType.FullName
            if definedTypes.ContainsKey(typeName) then
                MetadataObject typeName
            else
                definedTypes.Add(typeName, [])
                modelType.GetProperties()
                |> Array.map (fun prop -> 
                    { Name = prop.Name;
                      Type = extractPlonMetadata prop.PropertyType definedTypes })
                |> Array.map (fun x -> definedTypes.[typeName] <- List.append definedTypes.[typeName] [x]) 
                |> ignore
                MetadataObject typeName

let rec serializeMetadataType metadataType =
    match metadataType with
    | MetadataBool -> "bool"
    | MetadataNumber -> "number"
    | MetadataString -> "string"
    | MetadataArray arrayElementType -> (serializeMetadataType arrayElementType) + "[]"
    | MetadataObject typeName -> typeName

let serializeProperty (property: MetadataObjectProperty) =
    "{\"name\":\"" + property.Name + "\"," +
    "\"type\":\"" + serializeMetadataType property.Type + "\"}"

let serializeProperties properties = 
    properties 
    |> List.map serializeProperty
    |> String.concat ","

let serializeDefinedType definedType =
    "{\"name\":\"" + definedType.Name + "\"," +
    "\"properties\":[" + serializeProperties definedType.Properties + "]}"

let serializeDefinedTypes definedTypes =
    definedTypes 
    |> List.map serializeDefinedType
    |> String.concat ","

let serializePlonMetadata metadata = 
    "\"types\":[" + serializeDefinedTypes metadata.Types +
    "],\"rootType\":\"" + serializeMetadataType metadata.RootType + "\""

let serializeMetadata modelType = 
    let definedTypes = new Dictionary<MetadataTypeName, MetadataObjectProperty list>()
    let rootType = extractPlonMetadata modelType definedTypes  
    let definedTypesList =
        definedTypes
        |> Seq.map (fun kvp -> {Name = kvp.Key; Properties = kvp.Value})
        |> Seq.toList
    { Types = definedTypesList;
      RootType = rootType }
    |> serializePlonMetadata

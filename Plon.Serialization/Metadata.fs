module Plon.Serialization.Metadata

open System.Collections.Generic
open System.Collections
open System

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

let serializeMetadata metadata = 
    "{\"types\":[" + serializeDefinedTypes metadata.Types +
    "],\"rootType\":\"" + serializeMetadataType metadata.RootType + "\"}"

let getPlonMetadata modelType = 
    let definedTypes = new Dictionary<MetadataTypeName, MetadataObjectProperty list>()
    let rootType = extractPlonMetadata modelType definedTypes  
    let definedTypesList =
        definedTypes
        |> Seq.map (fun kvp -> {Name = kvp.Key; Properties = kvp.Value})
        |> Seq.toList
    { Types = definedTypesList;
      RootType = rootType }
    |> serializeMetadata

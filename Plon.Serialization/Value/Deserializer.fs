module Plon.Serialization.Value.Deserializer

open Plon.Serialization
open Common
open System
open System.Collections.Generic
open System.Collections.Concurrent
open System.Reflection

type TypeCache() =
    let mutable cache = new ConcurrentDictionary<string, Dictionary<string, PropertyInfo>>()
    
    member self.getPropertyInfo (objType: Type) propertyName =
        if cache.ContainsKey(objType.FullName) = false then
            cache.TryAdd(objType.FullName, new Dictionary<string, PropertyInfo>()) |> ignore
        
        let props = ref Unchecked.defaultof<Dictionary<string, PropertyInfo>>
        cache.TryGetValue(objType.FullName, props)

        if props.Value.ContainsKey(propertyName) = false then
            props.Value.Add(propertyName, objType.GetProperty(propertyName))
        props.Value.Item(propertyName)

 let typeCache = TypeCache()

let deserializeString obj (objType: Type) =
    let value = 
        match obj with
        | PlonValue.PlonString str -> str
        | PlonValue.PlonNull -> null
        | _ -> raise (Exception("Expected string."))

    if (objType = typeof<string>) then value |> box
    else if (objType = typeof<DateTime>) then DateTime.Parse(value) |> box
    else raise (Exception("Expected string."))
    
let deserializeNumber obj objType =
    let value = 
        match obj with
        | PlonValue.PlonNumber num -> num
        | _ -> raise (Exception("Expected number."))

    if (objType = typeof<uint8>) then box (Convert.ToUInt16(value))
    else if (objType = typeof<uint16>) then box (Convert.ToUInt16(value))
    else if (objType = typeof<uint32>) then box (Convert.ToUInt32(value))
    else if (objType = typeof<uint64>) then box (Convert.ToUInt64(value))
    else if (objType = typeof<int8>) then box (Convert.ToInt16(value))
    else if (objType = typeof<int16>) then box (Convert.ToInt16(value))
    else if (objType = typeof<int32>) then box (Convert.ToInt32(value))
    else if (objType = typeof<int64>) then box (Convert.ToInt64(value))
    else if (objType = typeof<double>) then box (Convert.ToDouble(value))
    else if (objType = typeof<decimal>) then box value
    // TODO: Handle overflow exceptions, other number types?
    else raise (Exception("Expected number."))

let deserializeBool obj objType =
    let value = 
        match obj with
        | PlonValue.PlonBoolean bool -> bool
        | _ -> raise (Exception("Expected bool."))

    if (objType = typeof<bool>) then box value
    else raise(Exception("Expected bool."))

let deserializeObjectProperty instance (objType: Type) value (propertyMetadata: MetadataObjectProperty) types deserializeFn =
    let property = typeCache.getPropertyInfo objType propertyMetadata.Name
    let propertyValue = deserializeFn value property.PropertyType propertyMetadata.Type types 
    property.SetValue(instance, propertyValue)
    // TODO: Handle no property setter
    // TODO: Handle members as well

let deserializeObject obj (objType: Type) (metadataType: string) (metadataTypes: IDictionary<MetadataTypeName, MetadataObjectProperty list>) deserializeFn =
    let values = 
        match obj with
        | PlonValue.PlonObject values -> Some values
        | PlonValue.PlonArray values -> Some values //TODO: Remove once parser is fixed
        | PlonValue.PlonNull -> None
        | _ -> raise (Exception("Expected object."))
    
    let instance = Activator.CreateInstance(objType)
    match values with 
    | Some values ->
        let properties = metadataTypes.[metadataType]
        List.map2 (fun p v -> deserializeObjectProperty instance objType v p metadataTypes deserializeFn) properties values
        |> ignore
        instance
    | None -> null
 
type ListHelper() =
    member self.createList<'T> () = 
        new List<'T>()

    member self.createListOfType (elemType: System.Type) =
        typeof<ListHelper>.GetMethod("createList").MakeGenericMethod(elemType).Invoke(self, [||])
        
    member self.createFSharpList<'T> (list: obj) =
        List.ofSeq (list :?> IEnumerable<'T>)

    member self.createFSharpListOfType (elemType: System.Type) (list: obj) =
        typeof<ListHelper>.GetMethod("createFSharpList").MakeGenericMethod(elemType).Invoke(self, [|list|])
   
let deserializeArray obj objType metadataType metadataTypes deserializeFn =
    let arr = 
        match obj with
        | PlonValue.PlonArray arr -> Some arr
        | PlonValue.PlonNull -> None
        | _ -> raise (Exception("Expected array."))

    match getCollectionType (objType) with
        | None -> raise (Exception("Expected collection."))
        | Some elemType -> 
            match arr with
            | Some elems ->
                let result = ListHelper().createListOfType elemType :?> System.Collections.IList          
                elems
                |> List.map (fun elem -> deserializeFn elem elemType metadataType metadataTypes |> unbox)
                |> List.iter (fun elem -> result.Add(elem) |> ignore)

                if (objType.Name.Contains("FSharpList")) then
                    ListHelper().createFSharpListOfType elemType result
                else 
                    result |> box
            | None -> null

let rec deserializeInternal (obj: PlonValue) objType metadataType metadataTypes =  
    match metadataType with
    | MetadataType.MetadataString -> 
        Convert.ChangeType(deserializeString obj objType, objType)
    | MetadataType.MetadataNumber -> 
        Convert.ChangeType(deserializeNumber obj objType, objType)
    | MetadataType.MetadataBool -> 
        Convert.ChangeType(deserializeBool obj objType, objType)
    | MetadataType.MetadataArray arrayType -> 
        deserializeArray obj objType arrayType metadataTypes deserializeInternal
    | MetadataType.MetadataObject metadataType ->
        Convert.ChangeType(deserializeObject obj objType metadataType metadataTypes deserializeInternal, objType)
        
let deserialize<'a> obj (metadata: PlonMetadata) =
    let metadataTypes = 
        metadata.Types
        |> List.map (fun t -> t.Name, t.Properties)
        |> dict

    deserializeInternal obj typeof<'a> metadata.RootType metadataTypes :?> 'a
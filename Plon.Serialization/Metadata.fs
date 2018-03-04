module Plon.Serialization.Metadata

let rec getPlonMetadata (modelType: System.Type) =
    match modelType with
    | str when str = typeof<string> -> "\"string\""
    | int when int = typeof<int> -> "\"number\""
    | enum when enum = typeof<System.Collections.IEnumerable> -> 
        "[" + 
        getPlonMetadata (enum.GetGenericArguments().[0]) +
        "]"
    | obj -> "{" +
             (modelType.GetProperties()
             |> Seq.map (fun prop -> "\"" + prop.Name + "\" : " + (getPlonMetadata prop.PropertyType) + ",")
             |> System.String.Concat) +
             "}"

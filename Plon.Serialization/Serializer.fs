module Plon.Serialization.Serializer

open Plon.Serialization.Metadata
open Plon.Serialization.Value

let serialize obj =
    "{" + (Serializer.serializeMetadata (obj.GetType())) + 
    ",\"value\":" + (Serializer.serializeValue obj) + "}"
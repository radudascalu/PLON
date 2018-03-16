module Plon.Serialization.Common

type PValue = 
    | PString of string
    | PNumber of decimal
    | PObject of PValue[]
    | PArray of PValue[]
    | PBoolean of bool
    | PNull
    

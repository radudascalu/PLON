module Plon.Serialization.Tests.ComplexObjects

open System

type Passenger(name, birthday) =
    member this.Name : string = name
    member this.Birthday : DateTime = birthday

type Engine(model, horsePower) =
    member this.Model : string = model
    member this.HorsePower : int = horsePower

type Car(brand, engine, passengers) =
    member this.Brand : string = brand
    member this.Engine : Engine = engine
    member this.Passengers : Passenger[] = passengers
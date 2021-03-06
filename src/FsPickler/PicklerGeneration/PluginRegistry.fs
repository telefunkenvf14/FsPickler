﻿namespace Nessos.FsPickler

open System
open System.Collections.Concurrent

// Defines a registry for providing user-supplied runtime pickler declarations

type private PicklerPlugin =
    /// Treat type as if carrying SerializableAttribute
    | DeclareSerializable
    /// Registered IPicklerFactory<'T> instance
    | Factory of obj

type internal PicklerPluginRegistry private () =
    static let registry = new ConcurrentDictionary<Type, PicklerPlugin> ()

    static member RegisterFactory<'T>(factory : IPicklerFactory<'T>) =
        registry.TryAdd(typeof<'T>, Factory (factory :> obj))

    static member DeclareSerializable (t : Type) =
        registry.TryAdd(t, DeclareSerializable)

    static member ContainsFactory(t : Type) = 
        match registry.TryFind t with
        | Some(Factory _) -> true
        | _ -> false

    static member IsDeclaredSerializable(t : Type) =
        match registry.TryFind t with
        | Some DeclareSerializable -> true
        | _ -> false

    static member GetPicklerFactory<'T> () : IPicklerFactory<'T> =
        match registry.[typeof<'T>] with
        | Factory o -> o :?> IPicklerFactory<'T>
        | _ -> invalidOp "internal error: not a pickler factory."
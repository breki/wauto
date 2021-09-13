#r "paket:
nuget Fake.IO.FileSystem
nuget Fake.Dotnet.MSBuild
nuget Fake.Core.Target //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators

let buildDir = "./build/"

Target.create "Clean" (fun _ -> Shell.cleanDir buildDir)

Target.create
    "BuildApp"
    (fun _ ->
        !! "**/*.fsproj"
        |> MSBuild.runRelease id buildDir "Build"
        |> ignore)

Target.create "Default" (fun _ -> Trace.trace "Hello World from FAKE")

// Dependencies
open Fake.Core.TargetOperators

"Clean" ==> "BuildApp" ==> "Default"

Target.runOrDefault "Default"

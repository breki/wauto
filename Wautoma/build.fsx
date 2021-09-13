#r "paket:
nuget Fake.IO.FileSystem
nuget Fake.Dotnet.MSBuild
nuget Fake.Dotnet.Testing.XUnit2
nuget Fake.Core.Target //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.DotNet
open Fake.DotNet.Testing
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators

let buildDir = "./build/"

// note: we will separate the test directory from the build in the future
let testDir = buildDir

Target.create "Clean" (fun _ -> Shell.cleanDir buildDir)

Target.create
    "Build"
    (fun _ ->
        [ "Wautoma.sln" ]
        |> MSBuild.runRelease id buildDir "Build"
        |> ignore)

Target.create
    "Test"
    (fun _ ->
        !!(testDir @@ "Wautoma.Tests.dll")
        |> XUnit2.run
            (fun p ->
                { p with
                        ToolPath = ""
                      HtmlOutputPath = Some(testDir @@ "xunit.html"); }))

Target.create "Default" (fun _ -> Trace.trace "Hello World from FAKE")

// Dependencies
open Fake.Core.TargetOperators

"Clean" ==> "Build" ==> "Test" ==> "Default"

Target.runOrDefault "Default"

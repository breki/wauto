module Tests.``UI automation``

open Wautoma.UIAutomation.AutomationExamples
open Xunit

//[<Fact>]
let ``moving to GMail`` () = openGmail (fun x -> printf $"%s{x}")

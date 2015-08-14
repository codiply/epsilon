namespace Epsilon.Logic.FSharp

open System
open System.IO
open System.Net
open System.Web

[<RequireQualifiedAccess>]
module WebPage =
   
    let downloadAsync (url : string) =
        async {
            let uri = new System.Uri(url)
            let webClient = new WebClient()
            let! html = webClient.AsyncDownloadString(uri)
            return html }
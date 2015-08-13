namespace Epsilon.Logic.FSharp

open System
open System.IO
open System.Net
open System.Web

[<RequireQualifiedAccess>]
module WebPage =

    type WebClientWithTimeout(timeoutInMilliseconds: int) = 
       inherit WebClient()
       override x.GetWebRequest(uri: Uri) = 
           let request = base.GetWebRequest(uri)
           request.Timeout <- timeoutInMilliseconds;
           request
   
    let downloadAsync (url : string) =
        async {
            let uri = new System.Uri(url)
            let webClient = new WebClient()
            let! html = webClient.AsyncDownloadString(uri)
            return html }


    let downloadWithTimeoutAsync (url : string)  (timeoutInMilliseconds: int) =
        async {
            let uri = new System.Uri(url)
            let webClient = new WebClientWithTimeout(timeoutInMilliseconds)
            let! html = webClient.AsyncDownloadString(uri)
            return html }
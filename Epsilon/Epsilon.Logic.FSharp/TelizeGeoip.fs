namespace Epsilon.Logic.FSharp.TelizeGeoip

open System
open System.Web
open FSharp.Data
open Epsilon.Logic.FSharp

type TalizeGeoipInfo = 
    { CountryCode: string
      ContinentCode: string
      Latitude: decimal 
      Longitude: decimal }

[<RequireQualifiedAccess>]
module GeoipClient = 

    type private TelizeGeoipProvider = JsonProvider<"""{"longitude":4.9,"latitude":52.3667,"asn":"AS196752","offset":"2","ip":"46.19.37.108","area_code":"0","continent_code":"EU","dma_code":"0","timezone":"Europe\/Amsterdam","country_code":"NL","isp":"Tilaa B.V.","country":"Netherlands","country_code3":"NLD"}""">

    let private getResponseAsync (ipAddress: string) =
        async {

            let url = 
                sprintf "https://www.telize.com/geoip/%s" ipAddress
            return! WebPage.downloadAsync(url)
        }

    let getResponse(ipAddress: string) = 
        getResponseAsync ipAddress |> Async.StartAsTask

    let parseResponse(response: string) =
        let typedResponse = TelizeGeoipProvider.Parse(response)
        { CountryCode = typedResponse.CountryCode 
          ContinentCode = typedResponse.ContinentCode
          Latitude = typedResponse.Latitude
          Longitude = typedResponse.Longitude }

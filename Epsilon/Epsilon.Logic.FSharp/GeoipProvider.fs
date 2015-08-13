namespace Epsilon.Logic.FSharp.GeoipProvider

open System
open System.Web
open FSharp.Data
open Epsilon.Logic.FSharp

type GeoipProviderResponse = 
    { CountryCode: string
      Latitude: Nullable<double> 
      Longitude: Nullable<double> }

[<RequireQualifiedAccess>]
module Helpers =

    let decimalOptionToNullableDouble (x : decimal option) =
        match x with 
        | None -> new System.Nullable<_>()
        | Some x -> new System.Nullable<_>(x |> double)

    let stringOptionToEmptyString (x : string option) = 
        match x with
        | None -> ""
        | Some x -> x

    let decimalToNullableDouble (x : decimal) =
        new System.Nullable<_>(x |> double)

[<RequireQualifiedAccess>]
module TelizeFsGeoipClient = 

    type private TelizeGeoipDataProvider = JsonProvider<"""
    [{"longitude":4.9,"latitude":52.3667,"asn":"AS196752","offset":"2","ip":"46.19.37.108","area_code":"0","continent_code":"EU","dma_code":"0","timezone":"Europe\/Amsterdam","country_code":"NL","isp":"Tilaa B.V.","country":"Netherlands","country_code3":"NLD"},
     {"ip":"192.168.1.1"}]""", true>

    let private getResponseAsync (ipAddress: string) =
        async {

            let url = 
                sprintf "https://www.telize.com/geoip/%s" ipAddress
            return! WebPage.downloadAsync(url)
        }

    let getResponse(ipAddress: string) = 
        getResponseAsync ipAddress |> Async.StartAsTask

    let parseResponse(response: string) =
        let typedResponse = TelizeGeoipDataProvider.Parse(response)
        { CountryCode = typedResponse.CountryCode |> Helpers.stringOptionToEmptyString
          Latitude = typedResponse.Latitude |> Helpers.decimalOptionToNullableDouble
          Longitude = typedResponse.Longitude |> Helpers.decimalOptionToNullableDouble }

[<RequireQualifiedAccess>]
module FreegeoipFsGeoipClient = 

    type private FreegeoipGeoipDataProvider = JsonProvider<"""
    [{"ip":"8.8.8.8","country_code":"US","country_name":"United States","region_code":"CA","region_name":"California","city":"Mountain View","zip_code":"94040","time_zone":"America/Los_Angeles","latitude":37.386,"longitude":-122.084,"metro_code":807},
     {"ip":"192.168.1.1","country_code":"","country_name":"","region_code":"","region_name":"","city":"","zip_code":"","time_zone":"","latitude":0,"longitude":0,"metro_code":0}]""", true>

    let private getResponseAsync (ipAddress: string) =
        async {

            let url = 
                sprintf "https://freegeoip.net/json/%s" ipAddress
            return! WebPage.downloadAsync(url)
        }

    let getResponse(ipAddress: string) = 
        getResponseAsync ipAddress |> Async.StartAsTask

    let parseResponse(response: string) =
        let typedResponse = FreegeoipGeoipDataProvider.Parse(response)
        { CountryCode = typedResponse.CountryCode |> Helpers.stringOptionToEmptyString
          Latitude = typedResponse.Latitude |> Helpers.decimalToNullableDouble
          Longitude = typedResponse.Longitude |> Helpers.decimalToNullableDouble }



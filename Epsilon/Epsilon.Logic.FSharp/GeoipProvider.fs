﻿namespace Epsilon.Logic.FSharp.GeoipProvider

open System
open System.Web
open FSharp.Data
open Epsilon.Logic.FSharp

type GeoipProviderClientResponse = 
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
module FreegeoipGeoipProviderClient = 

    type private FreegeoipGeoipDataProvider = JsonProvider<"""
    [{"ip":"8.8.8.8","country_code":"US","country_name":"United States","region_code":"CA","region_name":"California","city":"Mountain View","zip_code":"94040","time_zone":"America/Los_Angeles","latitude":37.386,"longitude":-122.084,"metro_code":807},
     {"ip":"192.168.1.1","country_code":"","country_name":"","region_code":"","region_name":"","city":"","zip_code":"","time_zone":"","latitude":0,"longitude":0,"metro_code":0}]""", true>

    let ParseResponse(response: string) =
        let typedResponse = FreegeoipGeoipDataProvider.Parse(response)
        { CountryCode = typedResponse.CountryCode |> Helpers.stringOptionToEmptyString
          Latitude = typedResponse.Latitude |> Helpers.decimalToNullableDouble
          Longitude = typedResponse.Longitude |> Helpers.decimalToNullableDouble }

[<RequireQualifiedAccess>]
module IpapiGeoipProviderClient = 

    type private IpapiGeoipDataProvider = JsonProvider<"""
    [{"as":"AS15169 Google Inc.","city":"Mountain View","country":"United States","countryCode":"US","isp":"Google","lat":37.386,"lon":-122.0838,"org":"Google","query":"8.8.8.8","region":"CA","regionName":"California","status":"success","timezone":"America/Los_Angeles","zip":"94040"},
     {"message":"private range","query":"192.168.1.1","status":"fail"}]""", true>

    let ParseResponse(response: string) =
        let typedResponse = IpapiGeoipDataProvider.Parse(response)
        { CountryCode = typedResponse.CountryCode |> Helpers.stringOptionToEmptyString
          Latitude = typedResponse.Lat |> Helpers.decimalOptionToNullableDouble
          Longitude = typedResponse.Lon |> Helpers.decimalOptionToNullableDouble }

[<RequireQualifiedAccess>]
module NekudoGeoipProviderClient = 

    type private NekudoGeoipDataProvider = JsonProvider<"""
    [{"city":"Mountain View","country":{"name":"United States","code":"US"},"location":{"latitude":37.386,"longitude":-122.0838,"time_zone":"America\/Los_Angeles"},"ip":"8.8.8.8"},
     {"type":"error","msg":"No record found."}]""", true>

    let ParseResponse(response: string) =
        let typedResponse = NekudoGeoipDataProvider.Parse(response)
        { CountryCode = 
              match typedResponse.Country with
              | None -> ""
              | Some(x) -> x.Code
          Latitude = 
             match typedResponse.Location with
             | None -> new System.Nullable<_>()
             | Some(x) -> x.Latitude |> Helpers.decimalToNullableDouble
          Longitude = 
             match typedResponse.Location with
             | None -> new System.Nullable<_>()
             | Some(x) -> x.Longitude |> Helpers.decimalToNullableDouble }

[<RequireQualifiedAccess>]
module TelizeGeoipProviderClient = 

    type private TelizeGeoipDataProvider = JsonProvider<"""
    [{"longitude":4.9,"latitude":52.3667,"asn":"AS196752","offset":"2","ip":"46.19.37.108","area_code":"0","continent_code":"EU","dma_code":"0","timezone":"Europe\/Amsterdam","country_code":"NL","isp":"Tilaa B.V.","country":"Netherlands","country_code3":"NLD"},
     {"ip":"192.168.1.1"}]""", true>

    let ParseResponse(response: string) =
        let typedResponse = TelizeGeoipDataProvider.Parse(response)
        { CountryCode = typedResponse.CountryCode |> Helpers.stringOptionToEmptyString
          Latitude = typedResponse.Latitude |> Helpers.decimalOptionToNullableDouble
          Longitude = typedResponse.Longitude |> Helpers.decimalOptionToNullableDouble }





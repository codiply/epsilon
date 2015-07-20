namespace Epsilon.Logic.FSharp.GoogleGeocode

// !!!!! NOTE !!!!!
// The code here is only used to test the Google Geocode API in the admin controller.
// For actual geocoding we use the GeocodeSharp library.

open System
open System.Web
open FSharp.Data
open Epsilon.Logic.FSharp

type Location = { Longitude: decimal; Latitude: decimal }

type Viewport = { Northeast: Location; Southwest: Location }

type Geometry = { Location: Location; Viewport: Viewport}

[<RequireQualifiedAccess>]
module Geocoder = 

    type private GeocodeJsonProvider = JsonProvider<"""{
   "results" : [
      {
         "address_components" : [
            {
               "long_name" : "1600",
               "short_name" : "1600",
               "types" : [ "street_number" ]
            },
            {
               "long_name" : "Amphitheatre Parkway",
               "short_name" : "Amphitheatre Pkwy",
               "types" : [ "route" ]
            },
            {
               "long_name" : "Mountain View",
               "short_name" : "Mountain View",
               "types" : [ "locality", "political" ]
            },
            {
               "long_name" : "Santa Clara County",
               "short_name" : "Santa Clara County",
               "types" : [ "administrative_area_level_2", "political" ]
            },
            {
               "long_name" : "California",
               "short_name" : "CA",
               "types" : [ "administrative_area_level_1", "political" ]
            },
            {
               "long_name" : "United States",
               "short_name" : "US",
               "types" : [ "country", "political" ]
            },
            {
               "long_name" : "94043",
               "short_name" : "94043",
               "types" : [ "postal_code" ]
            }
         ],
         "formatted_address" : "1600 Amphitheatre Parkway, Mountain View, CA 94043, USA",
         "geometry" : {
            "location" : {
               "lat" : 37.4223699,
               "lng" : -122.0842162
            },
            "location_type" : "ROOFTOP",
            "viewport" : {
               "northeast" : {
                  "lat" : 37.4237188802915,
                  "lng" : -122.0828672197085
               },
               "southwest" : {
                  "lat" : 37.4210209197085,
                  "lng" : -122.0855651802915
               }
            }
         },
         "place_id" : "ChIJ2eUgeAK6j4ARbn5u_wAGqWA",
         "types" : [ "street_address" ]
      }
   ],
   "status" : "OK"
}""">

    let private getResponseAsync (address: string) (region: string) (googleApiKey: string) =
        async {
            let encodedAddress = HttpUtility.UrlEncode(address) 
            let encodedRegion = HttpUtility.UrlEncode(region)
            let url = 
                sprintf "https://maps.googleapis.com/maps/api/geocode/json?address=%s&region=%s&key=%s" encodedAddress encodedRegion googleApiKey 
            return! WebPage.downloadAsync(url)
        }

    let getResponse(address: string, region: string, googleApiKey: string) = 
        getResponseAsync address region googleApiKey |> Async.StartAsTask

    let parseGeometries(response: string) =
        let typedResponse = GeocodeJsonProvider.Parse(response)
        typedResponse.Results |> Seq.map(fun res ->
            let viewport = 
                { Northeast = 
                    { Longitude = res.Geometry.Viewport.Northeast.Lng
                      Latitude = res.Geometry.Viewport.Northeast.Lat }
                  Southwest = 
                    { Longitude = res.Geometry.Viewport.Southwest.Lng
                      Latitude = res.Geometry.Viewport.Southwest.Lat } }
            { Location = 
                { Longitude = res.Geometry.Location.Lng
                  Latitude = res.Geometry.Location.Lat }
              Viewport = viewport })

namespace Epsilon.Logic.FSharp

open System
open FSharp.Data

type Location = { Longitude: decimal; Latitude: decimal }

type Viewport = { Northeast: Location; Southwest: Location }

type Geometry = { Location: Location; Viewport: Viewport}

type Geocode = { Geometry: Geometry }

[<RequireQualifiedAccess>]
module GoogleGeocode = 

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
            let addressWords = 
                address.Split(' ') 
                |> Array.filter (fun x -> String.IsNullOrWhiteSpace(x) |> not)
            let joinedAddressWords = String.Join("+", addressWords)
            // TODO_PANOS: Sanitize the address so that the url is valid.
            let url = 
                sprintf "https://maps.googleapis.com/maps/api/geocode/json?address=%s&region=%s&key=%s" joinedAddressWords region googleApiKey 
            return! WebPage.downloadAsync(url)
        }

    let getResponse(address: string, region: string, googleApiKey: string) = 
        getResponseAsync address region googleApiKey |> Async.StartAsTask

    let parseResponse(response: string) =
        let typedResponse = GeocodeJsonProvider.Parse(response)
        // TODO_PANOS: This will blow up if there are no results.
        let firstResult = typedResponse.Results |> Array.head
        let viewport = 
            { Northeast = 
                { Longitude = firstResult.Geometry.Viewport.Northeast.Lng
                  Latitude = firstResult.Geometry.Viewport.Northeast.Lat }
              Southwest = 
                { Longitude = firstResult.Geometry.Viewport.Southwest.Lng
                  Latitude = firstResult.Geometry.Viewport.Southwest.Lat } }
        { Geometry = 
            { Location = 
                { Longitude = firstResult.Geometry.Location.Lng
                  Latitude = firstResult.Geometry.Location.Lat }
              Viewport = viewport }}

    let geocode(address: string, region: string, googleApiKey: string) =
        async {
            let! response = getResponseAsync address region googleApiKey
            let answer = GeocodeJsonProvider.Parse(response)
            return answer
        }
        |> Async.StartAsTask

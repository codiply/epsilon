namespace Epsilon.Logic.FSharp

open System
open FSharp.Data

[<RequireQualifiedAccess>]
module Google = 

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

    let geocode(addressWords: seq<string>, googleApiKey: string) =
        async {
            let joinedAddressWords = String.Join("+", addressWords)
            let url = sprintf "https://maps.googleapis.com/maps/api/geocode/json?address=%s&key=%s" joinedAddressWords googleApiKey 
            let! response = WebPage.downloadAsync(url)
            let answer = GeocodeJsonProvider.Parse(response)
            return answer
        }
        |> Async.StartAsTask

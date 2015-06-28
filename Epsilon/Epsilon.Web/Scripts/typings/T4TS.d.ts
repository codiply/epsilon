/****************************************************************************
  Generated by T4TS.tt - don't make any changes in this file
****************************************************************************/

declare module T4TS {
    /** Generated from Epsilon.Logic.JsonModels.AddressSearchRequest **/
    export interface AddressSearchRequest {
        countryId: string;
        postcode: string;
        terms: string;
    }
    /** Generated from Epsilon.Logic.JsonModels.AddressSearchResponse **/
    export interface AddressSearchResponse {
        Results: T4TS.AddressSearchResult[];
        ResultsLimit: number;
        IsResultsLimitReached: boolean;
    }
    /** Generated from Epsilon.Logic.JsonModels.AddressSearchResult **/
    export interface AddressSearchResult {
        addressId: string;
        fullAddress: string;
    }
}

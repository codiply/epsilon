/****************************************************************************
  Generated by T4TS.tt - don't make any changes in this file
****************************************************************************/

declare module T4TS {
    /** Generated from Epsilon.Logic.JsonModels.AddressGeometryRequest **/
    export interface AddressGeometryRequest {
        uniqueId: string;
    }
    /** Generated from Epsilon.Logic.JsonModels.AddressGeometryResponse **/
    export interface AddressGeometryResponse {
        latitude: number;
        longitude: number;
        viewportNortheastLatitude: number;
        viewportNortheastLongitude: number;
        viewportSouthwestLatitude: number;
        viewportSouthwestLongitude: number;
    }
    /** Generated from Epsilon.Logic.JsonModels.AddressSearchRequest **/
    export interface AddressSearchRequest {
        countryId: string;
        postcode: string;
        terms: string;
    }
    /** Generated from Epsilon.Logic.JsonModels.AddressSearchResponse **/
    export interface AddressSearchResponse {
        results: T4TS.AddressSearchResult[];
        resultsLimit: number;
        isResultsLimitExceeded: boolean;
    }
    /** Generated from Epsilon.Logic.JsonModels.AddressSearchResult **/
    export interface AddressSearchResult {
        addressUniqueId: string;
        fullAddress: string;
    }
    /** Generated from Epsilon.Logic.JsonModels.TenancyDetailsSubmissionInfo **/
    export interface TenancyDetailsSubmissionInfo {
        uniqueId: string;
        displayAddress: string;
        stepVerificationCodeSentOutDone: boolean;
        stepVerificationCodeEnteredDone: boolean;
        stepTenancyDetailsSubmittedDone: boolean;
        stepMoveOutDetailsSubmittedDone: boolean;
        canEnterVerificationCode: boolean;
        canSubmitTenancyDetails: boolean;
        canSubmitMoveOutDetails: boolean;
    }
    /** Generated from Epsilon.Logic.JsonModels.TokenBalanceResponse **/
    export interface TokenBalanceResponse {
        balance: number;
    }
    /** Generated from Epsilon.Logic.JsonModels.UserSubmissionsSummary **/
    export interface UserSubmissionsSummary {
        tenancyDetailsSubmissions: T4TS.TenancyDetailsSubmissionInfo[];
    }
}

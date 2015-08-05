module Epsilon.NgApp.Controllers {
    export interface PropertyInfoPropertySearchScope extends ng.IScope {
        countryId: string;
        postcode: string;
        terms: string;
        hasHitSearch: boolean;
        searchInProgress: boolean;
        selectedAddressUniqueId: string;
        propertySearchResponse: T4TS.PropertySearchResponse;
    }

    export class PropertyInfoPropertySearchController {
        constructor(
            private $scope: PropertyInfoPropertySearchScope,
            private $http: ng.IHttpService,
            private BASE_URL_WITH_LANGUAGE: string,
            public COUNTRY_VARIANT_RESOURCES: any) {
        }

        public countryIdChanged() {
            this.$scope.postcode = null;
            this.$scope.terms = null;
            this.$scope.hasHitSearch = false;
            this.$scope.selectedAddressUniqueId = null;
            this.$scope.propertySearchResponse = null;
        };

        public fetchSearchResults() {
            var scope = this.$scope;
            scope.hasHitSearch = true;
            scope.searchInProgress = true;
            var url = this.BASE_URL_WITH_LANGUAGE + '/api/address/searchproperty/';
            var request: T4TS.PropertySearchRequest = {
                countryId: scope.countryId,
                postcode: scope.postcode,
                terms: scope.terms
            };
            this.$http.post<T4TS.PropertySearchResponse>(url, request)
                .success(function (data, status, headers, config) {
                scope.propertySearchResponse = data;
            }).finally(function () {
                scope.searchInProgress = false;
            });
        }
    }
}
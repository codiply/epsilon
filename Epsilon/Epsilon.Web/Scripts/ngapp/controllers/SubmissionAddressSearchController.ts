interface SubmissionAddressSearchScope extends ng.IScope {
    countryId: string;
    postcode: string;
    terms: string;
    hasHitSearch: boolean;
    searchInProgress: boolean;
    selectedAddressUniqueId: string;
    addressSearchResponse: T4TS.AddressSearchResponse;
}

class SubmissionAddressSearchController {
    constructor(
        private $scope: SubmissionAddressSearchScope,
        private $http: ng.IHttpService,
        private BASE_URL_WITH_LANGUAGE: string,
        public COUNTRY_VARIANT_RESOURCES: any) {
    }

    public countryIdChanged() {
        this.$scope.postcode = null;
        this.$scope.terms = null;
        this.$scope.hasHitSearch = false;
        this.$scope.selectedAddressUniqueId = null;
        this.$scope.addressSearchResponse = null;
    };

    public fetchSearchResults() {
        var scope = this.$scope;
        scope.hasHitSearch = true;
        scope.searchInProgress = true;
        var url = this.BASE_URL_WITH_LANGUAGE + '/api/address/search/';
        var request: T4TS.AddressSearchRequest = {
            countryId: scope.countryId,
            postcode: scope.postcode,
            terms: scope.terms
        };
        this.$http.post<T4TS.AddressSearchResponse>(url, request)
            .success(function (data, status, headers, config) {
            scope.addressSearchResponse = data;
        }).finally(function () {
            scope.searchInProgress = false;
        });
    }
}
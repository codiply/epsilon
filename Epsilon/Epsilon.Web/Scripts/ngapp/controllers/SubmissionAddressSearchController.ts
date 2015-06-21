interface SubmissionAddressSearchScope extends ng.IScope {
    countryId: string;
    postcode: string;
    hasHitSearch: boolean;
    selectedAddressId: string;
    addressSearchResults: T4TS.AddressSearchResult[];
}

class SubmissionAddressSearchController {
    constructor(
        private $scope: SubmissionAddressSearchScope,
        private $http: ng.IHttpService,
        private BASE_URL_WITH_LANGUAGE: string) {
    }

    public FetchSearchResults() {
        var scope = this.$scope;
        scope.hasHitSearch = true;
        var url = this.BASE_URL_WITH_LANGUAGE + '/api/address/search/';
        var request: T4TS.AddressSearchRequest = {
            countryId: scope.countryId,
            postcode: scope.postcode
        };
        this.$http.post<T4TS.AddressSearchResult[]>(url, request)
            .success(function (data, status, headers, config) {
            scope.addressSearchResults = data;
        });
    }
}
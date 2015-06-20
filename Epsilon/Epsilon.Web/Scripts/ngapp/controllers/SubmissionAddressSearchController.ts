interface SubmissionAddressSearchScope extends ng.IScope {
    countryId: string;
    postcode: string;
    searchResults: T4TS.AddressSearchResult[];
}

class SubmissionAddressSearchController {
    constructor(
        private $scope: ng.IScope,
        private $http: ng.IHttpService,
        private BASE_URL_WITH_LANGUAGE: string) {
    }

    public FetchSearchResults() {
        alert(this.BASE_URL_WITH_LANGUAGE);
        //var url = '';
        //this.$http.post('data/posts.json').success(function (data, status, headers, config) {
        //        this.$scope.searchResults = data;
        //});
    }
}
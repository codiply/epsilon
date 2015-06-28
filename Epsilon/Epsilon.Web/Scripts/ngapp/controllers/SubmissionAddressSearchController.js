var SubmissionAddressSearchController = (function () {
    function SubmissionAddressSearchController($scope, $http, BASE_URL_WITH_LANGUAGE) {
        this.$scope = $scope;
        this.$http = $http;
        this.BASE_URL_WITH_LANGUAGE = BASE_URL_WITH_LANGUAGE;
    }
    SubmissionAddressSearchController.prototype.FetchSearchResults = function () {
        var scope = this.$scope;
        scope.hasHitSearch = true;
        var url = this.BASE_URL_WITH_LANGUAGE + '/api/address/search/';
        var request = {
            countryId: scope.countryId,
            postcode: scope.postcode,
            terms: scope.terms
        };
        this.$http.post(url, request)
            .success(function (data, status, headers, config) {
            scope.addressSearchResponse = data;
        });
    };
    return SubmissionAddressSearchController;
})();
//# sourceMappingURL=SubmissionAddressSearchController.js.map
var SubmissionAddressSearchController = (function () {
    function SubmissionAddressSearchController($scope, $http, BASE_URL_WITH_LANGUAGE) {
        this.$scope = $scope;
        this.$http = $http;
        this.BASE_URL_WITH_LANGUAGE = BASE_URL_WITH_LANGUAGE;
    }
    SubmissionAddressSearchController.prototype.FetchSearchResults = function () {
        alert(this.BASE_URL_WITH_LANGUAGE);
        //var url = '';
        //this.$http.post('data/posts.json').success(function (data, status, headers, config) {
        //        this.$scope.searchResults = data;
        //});
    };
    return SubmissionAddressSearchController;
})();
//# sourceMappingURL=SubmissionAddressSearchController.js.map
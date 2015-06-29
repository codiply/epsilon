var app = angular.module('ngEpsilon', ['ngEpsilon.config']);

app.controller('SubmissionAddressSearchController',
    ['$scope', '$http', 'BASE_URL_WITH_LANGUAGE', 'COUNTRY_VARIANT_RESOURCES', SubmissionAddressSearchController]);
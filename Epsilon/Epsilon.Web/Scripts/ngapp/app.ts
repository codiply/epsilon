angular.module('ngEpsilon', ['ngEpsilon.config'])
    .controller('SubmissionAddressSearchController',
        ['$scope', '$http', 'BASE_URL_WITH_LANGUAGE', 'COUNTRY_VARIANT_RESOURCES', SubmissionAddressSearchController]);
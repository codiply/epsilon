angular.module('ngEpsilon', ['ngEpsilon.config'])
    // Filters
    .filter("stringFormat", [NgFilters.stringFormat])
    // Controllers
    .controller('SubmissionAddressSearchController', ['$scope', '$http', 'BASE_URL_WITH_LANGUAGE', 'COUNTRY_VARIANT_RESOURCES', SubmissionAddressSearchController]);
import Controllers = Epsilon.NgApp.Controllers;
import Directives = Epsilon.NgApp.Directives;
import Filters = Epsilon.NgApp.Filters;


angular.module('ngEpsilon', ['ngEpsilon.config'])
    // Controllers
    .controller('SubmissionAddressSearchController',
        ['$scope', '$http', 'BASE_URL_WITH_LANGUAGE', 'COUNTRY_VARIANT_RESOURCES',
            Controllers.SubmissionAddressSearchController])
    // Directives
    .directive("clickOnce", ['$timeout', Directives.ClickOnce])
    // Filters
    .filter("stringFormat", [Filters.stringFormat]);

import Controllers = Epsilon.NgApp.Controllers;
import Directives = Epsilon.NgApp.Directives;
import Filters = Epsilon.NgApp.Filters;

angular.module('ngEpsilon', ['ngEpsilon.config'])
// Controllers
    .controller('SubmissionAddressSearchController',
        ['$scope', '$http', 'BASE_URL_WITH_LANGUAGE', 'COUNTRY_VARIANT_RESOURCES',
            Controllers.SubmissionAddressSearchController])
// Directives
    .directive("addressMap", ['$http', 'BASE_URL_WITH_LANGUAGE', Directives.AddressMap])
    .directive("clickOnce", ['$timeout', Directives.ClickOnce])
    .directive("mySubmissionSummary", ['$http', 'BASE_URL_WITH_LANGUAGE', 'DIRECTIVE_TEMPLATE_FOLDER_URL', Directives.MySubmissionSummary])
    .directive("tokenBalanceBadge", ['$http', 'BASE_URL_WITH_LANGUAGE', Directives.TokenBalanceBadge])
// Filters
    .filter("stringFormat", Filters.stringFormat);

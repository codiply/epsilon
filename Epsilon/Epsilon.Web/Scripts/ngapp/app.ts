import Controllers = Epsilon.NgApp.Controllers;
import Directives = Epsilon.NgApp.Directives;
import Filters = Epsilon.NgApp.Filters;

angular.module('ngEpsilon', ['ngEpsilon.config'])
// Controllers
    .controller('SubmissionAddressSearchController',
        ['$scope', '$http', 'BASE_URL_WITH_LANGUAGE', 'COUNTRY_VARIANT_RESOURCES', Controllers.SubmissionAddressSearchController])
// Directives
    .directive("addressMap", ['$http', 'BASE_URL_WITH_LANGUAGE', Directives.AddressMap])
    .directive("clickOnce", ['$timeout', Directives.ClickOnce])
    .directive("myOutgoingVerificationsSummary", ['$http', 'BASE_URL_WITH_LANGUAGE', 'DIRECTIVE_TEMPLATE_FOLDER_URL', Directives.MyOutgoingVerificationsSummary])
    .directive("mySubmissionsSummary", ['$http', 'BASE_URL_WITH_LANGUAGE', 'DIRECTIVE_TEMPLATE_FOLDER_URL', Directives.MySubmissionsSummary])
    .directive("myTokenTransactions", ['$http', 'BASE_URL_WITH_LANGUAGE', 'DIRECTIVE_TEMPLATE_FOLDER_URL', Directives.MyTokenTransactions])
    .directive("tokenBalanceBadge", ['$http', 'BASE_URL_WITH_LANGUAGE', Directives.TokenBalanceBadge])
    .directive("tokenRewardsSummary", ['$http', 'BASE_URL_WITH_LANGUAGE', 'DIRECTIVE_TEMPLATE_FOLDER_URL', Directives.TokenRewardsSummary])
// Filters
    .filter("localDateTime", Filters.localDateTime)
    .filter("stringFormat", Filters.stringFormat);

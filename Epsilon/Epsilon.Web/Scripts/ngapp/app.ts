import Controllers = Epsilon.NgApp.Controllers;
import Directives = Epsilon.NgApp.Directives;
import Filters = Epsilon.NgApp.Filters;

angular.module('ngEpsilon', ['ngEpsilon.config', 'infinite-scroll', 'angularMoment', 'ui.bootstrap.tpls', 'ui.bootstrap.rating'])
// Controllers
    .controller('PropertyInfoPropertySearchController',
        ['$scope', '$http', 'BASE_URL_WITH_LANGUAGE', 'COUNTRY_VARIANT_RESOURCES', Controllers.PropertyInfoPropertySearchController])
    .controller('SubmissionAddressSearchController',
        ['$scope', '$http', 'BASE_URL_WITH_LANGUAGE', 'COUNTRY_VARIANT_RESOURCES', Controllers.SubmissionAddressSearchController])
// Directives
    .directive("addressMap", ['$http', 'BASE_URL_WITH_LANGUAGE', Directives.AddressMap])
    .directive("clickOnce", ['$timeout', Directives.ClickOnce])
    .directive("myExploredPropertiesSummary", ['$http', 'BASE_URL_WITH_LANGUAGE', 'DIRECTIVE_TEMPLATE_FOLDER_URL', Directives.MyExploredPropertiesSummary])
    .directive("myOutgoingVerificationsSummary", ['$http', 'BASE_URL_WITH_LANGUAGE', 'DIRECTIVE_TEMPLATE_FOLDER_URL', Directives.MyOutgoingVerificationsSummary])
    .directive("mySubmissionsSummary", ['$http', 'BASE_URL_WITH_LANGUAGE', 'DIRECTIVE_TEMPLATE_FOLDER_URL', Directives.MySubmissionsSummary])
    .directive("myTokenTransactions", ['$http', 'BASE_URL_WITH_LANGUAGE', 'DIRECTIVE_TEMPLATE_FOLDER_URL', Directives.MyTokenTransactions])
    .directive("starRatingEditor", ['DIRECTIVE_TEMPLATE_FOLDER_URL', Directives.StarRatingEditor])
    .directive("tokenBalanceBadge", ['$http', 'BASE_URL_WITH_LANGUAGE', Directives.TokenBalanceBadge])
    .directive("tokenRewardsSummary", ['$http', 'BASE_URL_WITH_LANGUAGE', 'DIRECTIVE_TEMPLATE_FOLDER_URL', Directives.TokenRewardsSummary])
    .directive("tooltip", [Directives.Tooltip])
// Filters
    .filter("localDateTime", Filters.localDateTime)
    .filter("stringFormat", Filters.stringFormat);

angular.module('ngEpsilon')
    .run(['amMoment', 'LANGUAGE_ID', function (amMoment, languageId) {
        amMoment.changeLocale(languageId);
    }]);

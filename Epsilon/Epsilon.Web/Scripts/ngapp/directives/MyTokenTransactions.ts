module Epsilon.NgApp.Directives {
    export interface MyTokenTransactionsScope extends ng.IScope {
    }

    export class MyTokenTransactions implements ng.IDirective {
        constructor(
            $http: ng.IHttpService,
            BASE_URL_WITH_LANGUAGE: string,
            DIRECTIVE_TEMPLATE_FOLDER_URL: string) {
            return {
                restrict: 'E',
                scope: {
                },
                templateUrl: DIRECTIVE_TEMPLATE_FOLDER_URL + 'MyTokenTransactions',
                link: (scope: MyTokenTransactionsScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes) => {
                    new MyTokenTransactionsLink(scope, element, attributes, $http, BASE_URL_WITH_LANGUAGE);
                }
            };
        }
    }

    export class MyTokenTransactionsLink {
        constructor(private scope: MyTokenTransactionsScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes,
            private $http: ng.IHttpService, private BASE_URL_WITH_LANGUAGE: string) {
        }

        //private fetchSummary() {
        //    var url = this.BASE_URL_WITH_LANGUAGE + '/api/submission/mysubmissionssummary/';
        //    var scope = this.scope;
        //    var request: T4TS.MySubmissionsSummaryRequest = {
        //        limitItemsReturned: scope.limitItemsReturned && scope.limitItemsReturned.toLowerCase() === "true",
        //        allowCaching: scope.allowCaching && scope.allowCaching.toLowerCase() === "true"
        //    };
        //    this.$http.post<T4TS.MySubmissionsSummaryResponse>(url, request).success(function (data, status, headers, config) {
        //        scope.response = data;
        //    });
        //}
    }
}
module Epsilon.NgApp.Directives {
    export interface MySubmissionsSummaryScope extends ng.IScope {
        limitItemsReturned: string;
        response: T4TS.MySubmissionsSummaryResponse;
    }

    export class MySubmissionsSummary implements ng.IDirective {
        constructor(
            $http: ng.IHttpService,
            BASE_URL_WITH_LANGUAGE: string,
            DIRECTIVE_TEMPLATE_FOLDER_URL: string) {
            return {
                restrict: 'E',
                scope: {
                    limitItemsReturned: "@"
                },
                templateUrl: DIRECTIVE_TEMPLATE_FOLDER_URL + 'MySubmissionsSummary',
                link: (scope: MySubmissionsSummaryScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes) => {
                    new MySubmissionsSummaryLink(scope, element, attributes, $http, BASE_URL_WITH_LANGUAGE);
                }
            };
        }
    }

    export class MySubmissionsSummaryLink {
        constructor(private scope: MySubmissionsSummaryScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes,
            private $http: ng.IHttpService, private BASE_URL_WITH_LANGUAGE: string) {
            this.fetchSummary();
        }

        private fetchSummary() {
            var url = this.BASE_URL_WITH_LANGUAGE + '/api/submission/mysubmissionssummary/';
            var scope = this.scope;
            var request: T4TS.MySubmissionsSummaryRequest = {
                limitItemsReturned: scope.limitItemsReturned && scope.limitItemsReturned.toLowerCase() === "true"
            };
            this.$http.post<T4TS.MySubmissionsSummaryResponse>(url, request).success(function (data, status, headers, config) {
                scope.response = data;
            });
        }
    }
}
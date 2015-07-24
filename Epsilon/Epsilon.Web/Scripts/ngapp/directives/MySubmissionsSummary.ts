module Epsilon.NgApp.Directives {
    export interface MySubmissionsSummaryScope extends ng.IScope {
        userSubmissionsSummary: T4TS.UserSubmissionsSummary;
    }

    export class MySubmissionsSummary implements ng.IDirective {
        constructor(
            $http: ng.IHttpService,
            BASE_URL_WITH_LANGUAGE: string,
            DIRECTIVE_TEMPLATE_FOLDER_URL: string) {
            return {
                restrict: 'E',
                scope: {
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
            this.$http.get<T4TS.UserSubmissionsSummary>(url).success(function (data, status, headers, config) {
                scope.userSubmissionsSummary = data;
            });
        }
    }
}
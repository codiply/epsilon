module Epsilon.NgApp.Directives {
    export interface MySubmissionSummaryScope extends ng.IScope {
        userSubmissionSummary: T4TS.UserSubmissionSummary;
    }

    export class MySubmissionSummary implements ng.IDirective {
        constructor(
            $http: ng.IHttpService,
            BASE_URL_WITH_LANGUAGE: string,
            DIRECTIVE_TEMPLATE_FOLDER_URL: string) {
            return {
                restrict: 'E',
                scope: {
                },
                templateUrl: DIRECTIVE_TEMPLATE_FOLDER_URL + 'MySubmissionSummary',
                link: (scope: MySubmissionSummaryScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes) => {
                    new MySubmissionSummaryLink(scope, element, attributes, $http, BASE_URL_WITH_LANGUAGE);
                }
            };
        }
    }

    export class MySubmissionSummaryLink {
        constructor(private scope: MySubmissionSummaryScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes,
            private $http: ng.IHttpService, private BASE_URL_WITH_LANGUAGE: string) {
            this.fetchSummary();
        }

        private fetchSummary() {
            var url = this.BASE_URL_WITH_LANGUAGE + '/api/submission/mysubmissionsummary/';
            var scope = this.scope;
            this.$http.get<T4TS.UserSubmissionSummary>(url).success(function (data, status, headers, config) {
                scope.userSubmissionSummary = data;
            });
        }
    }
}
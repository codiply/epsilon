module Epsilon.NgApp.Directives {
    export interface MyExploredPropertiesSummaryScope extends ng.IScope {
        limitItemsReturned: string;
        isSummaryPage: string;
        allowCaching: string;
        response: T4TS.MyExploredPropertiesSummaryResponse;
    }

    export class MyExploredPropertiesSummary implements ng.IDirective {
        constructor(
            $http: ng.IHttpService,
            BASE_URL_WITH_LANGUAGE: string,
            DIRECTIVE_TEMPLATE_FOLDER_URL: string) {
            return {
                restrict: 'E',
                scope: {
                    limitItemsReturned: "@",
                    isSummaryPage: "@",
                    allowCaching: "@"
                },
                templateUrl: DIRECTIVE_TEMPLATE_FOLDER_URL + 'MyExploredPropertiesSummary',
                link: (scope: MyExploredPropertiesSummaryScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes) => {
                    new MyExploredPropertiesSummaryLink(scope, element, attributes, $http, BASE_URL_WITH_LANGUAGE);
                }
            };
        }
    }

    export class MyExploredPropertiesSummaryLink {
        constructor(private scope: MyExploredPropertiesSummaryScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes,
            private $http: ng.IHttpService, private BASE_URL_WITH_LANGUAGE: string) {
            this.fetchSummary();
        }

        private fetchSummary() {
            var url = this.BASE_URL_WITH_LANGUAGE + '/api/propertyinfo/myexploredpropertiessummary/';
            var scope = this.scope;
            var request: T4TS.MyExploredPropertiesSummaryRequest = {
                limitItemsReturned: scope.limitItemsReturned && scope.limitItemsReturned.toLowerCase() === "true",
                allowCaching: scope.allowCaching && scope.allowCaching.toLowerCase() === "true"
            };
            this.$http.post<T4TS.MyExploredPropertiesSummaryResponse>(url, request).success(function (data, status, headers, config) {
                scope.response = data;
            });
        }
    }
}
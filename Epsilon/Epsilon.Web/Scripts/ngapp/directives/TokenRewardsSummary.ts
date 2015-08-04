module Epsilon.NgApp.Directives {
    export interface TokenRewardsSummaryScope extends ng.IScope {
        earnTypeValues: T4TS.TokenRewardTypeValue[];
        spendTypeValues: T4TS.TokenRewardTypeValue[];
        tokenRewardTypeMetadata: linq.Dictionary<T4TS.TokenRewardTypeMetadata>;
    }

    export class TokenRewardsSummary implements ng.IDirective {
        constructor(
            $http: ng.IHttpService,
            BASE_URL_WITH_LANGUAGE: string,
            DIRECTIVE_TEMPLATE_FOLDER_URL: string) {
            return {
                restrict: 'E',
                scope: {
                },
                replace: true,
                templateUrl: DIRECTIVE_TEMPLATE_FOLDER_URL + 'TokenRewardsSummary',
                link: (scope: TokenRewardsSummaryScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes) => {
                    new TokenRewardsSummaryLink(scope, element, attributes, $http, BASE_URL_WITH_LANGUAGE);
                }
            };
        }
    }

    export class TokenRewardsSummaryLink {
        constructor(private scope: TokenRewardsSummaryScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes,
            private $http: ng.IHttpService, private BASE_URL_WITH_LANGUAGE: string) {

            var scope = this.scope;

            var url = this.BASE_URL_WITH_LANGUAGE + '/api/token/tokenrewardssummary/'

            $http.get<T4TS.TokenRewardsSummaryResponse>(url).success(function (data, status, headers, config) {
                scope.earnTypeValues = data.earnTypeValues;
                scope.spendTypeValues = data.spendTypeValues;
                scope.tokenRewardTypeMetadata = Enumerable.From(data.typeMetadata).ToDictionary(x => x.key, x => x);
            });
        }
    }
}
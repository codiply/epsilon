module Epsilon.NgApp.Directives {
    export interface TokenBalanceBadgeScope extends ng.IScope {
        tokenBalanceResponse: T4TS.TokenBalanceResponse;
    }

    export class TokenBalanceBadge implements ng.IDirective {
        constructor(
            $http: ng.IHttpService,
            BASE_URL_WITH_LANGUAGE: string) {
            return {
                restrict: 'E',
                scope: {
                },
                template: '<span class="badge" data-ng-cloak data-ng-show="tokenBalanceResponse">{{tokenBalanceResponse.balance}}</span>',
                link: (scope: TokenBalanceBadgeScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes) => {
                    new TokenBalanceBadgeLink(scope, element, attributes, $http, BASE_URL_WITH_LANGUAGE);
                }
            };
        }
    }

    export class TokenBalanceBadgeLink {
        constructor(private scope: TokenBalanceBadgeScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes,
            private $http: ng.IHttpService, private BASE_URL_WITH_LANGUAGE: string) {
            this.fetchBalance();
        }

        private fetchBalance() {
            var url = this.BASE_URL_WITH_LANGUAGE + '/api/token/balance/';
            var scope = this.scope;
            this.$http.get<T4TS.TokenBalanceResponse>(url).success(function (data, status, headers, config) {
                scope.tokenBalanceResponse = data;
            });
        }
    }
}
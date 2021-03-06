﻿module Epsilon.NgApp.Directives {
    export interface MyTokenTransactionsScope extends ng.IScope {
        items: T4TS.MyTokenTransactionsItem[],
        busy: boolean,
        earliestMadeOn: string,
        moreItemsExist: boolean,
        tokenRewardTypeMetadata: linq.Dictionary<T4TS.TokenRewardTypeMetadata>,
        fetchNextPage: () => void
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
                replace: true,
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
            var scope = this.scope;

            scope.moreItemsExist = true;
            scope.busy = false;
            scope.earliestMadeOn = null;
            scope.items = new Array<T4TS.MyTokenTransactionsItem>();

            var metadataUrl = this.BASE_URL_WITH_LANGUAGE + '/api/token/tokenrewardmetadata/'
            $http.get<T4TS.TokenRewardMetadata>(metadataUrl).success(function (data, status, headers, config) {
                scope.tokenRewardTypeMetadata = Enumerable.From(data.typeMetadata).ToDictionary(x => x.key, x => x);
            });

            var http = this.$http;
            var baseUrlWithLanguage = this.BASE_URL_WITH_LANGUAGE

            scope.fetchNextPage = function () {
                if (scope.moreItemsExist) {
                    var url = baseUrlWithLanguage + '/api/token/mytokentransactionsnextpage/';
                    scope.busy = true;
                    var request: T4TS.MyTokenTransactionsPageRequest = {
                        madeBefore: scope.earliestMadeOn
                    };
                    http.post<T4TS.MyTokenTransactionsPageResponse>(url, request).success(function (data, status, headers, config) {
                        for (var i = 0; i < data.items.length; i++) {
                            scope.items.push(data.items[i]); 
                        }
                        scope.earliestMadeOn = data.earliestMadeOn;
                        scope.moreItemsExist = data.moreItemsExist;
                    }).finally(function () {
                        scope.busy = false;
                    });
                }
            }
        }
    }
}
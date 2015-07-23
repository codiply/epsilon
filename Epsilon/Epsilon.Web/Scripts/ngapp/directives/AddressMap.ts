module Epsilon.NgApp.Directives {
    export interface AddressMapScope extends ng.IScope {

    }

    export class AddressMap implements ng.IDirective {
        constructor(
            $http: ng.IHttpService,
            BASE_URL_WITH_LANGUAGE: string) {
            return {
                restrict: 'A',
                link: (scope: AddressMapScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes) => {
                    new AddressMapLink(scope, element, attributes, $http, BASE_URL_WITH_LANGUAGE);
                }
            };
        }
    }

    export class AddressMapLink {
        scope: AddressMapScope;
        constructor(scope: AddressMapScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes,
            $http: ng.IHttpService,
            BASE_URL_WITH_LANGUAGE: string) {
            this.scope = scope;
        }
    }
}
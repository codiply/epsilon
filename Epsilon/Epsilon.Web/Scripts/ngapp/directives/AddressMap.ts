module Epsilon.NgApp.Directives {
    export interface AddressMapScope extends ng.IScope {
        addressUniqueId: string;
        addressGeometry: T4TS.AddressGeometryResponse;
    }

    export class AddressMap implements ng.IDirective {
        constructor(
            $http: ng.IHttpService,
            BASE_URL_WITH_LANGUAGE: string) {
            return {
                restrict: 'E',
                scope: {
                    addressUniqueId: "@"
                },
                template: '<p>{{addressGeometry.latitude}} - {{addressGeometry.longitude}}</p>',
                link: (scope: AddressMapScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes) => {
                    new AddressMapLink(scope, element, attributes, $http, BASE_URL_WITH_LANGUAGE);
                }
            };
        }
    }

    export class AddressMapLink {
        scope: AddressMapScope;

        constructor(scope: AddressMapScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes,
            private $http: ng.IHttpService,
            private BASE_URL_WITH_LANGUAGE: string) {
            this.scope = scope;
            this.fetchAddressGeometry();    
        }

        private fetchAddressGeometry() {
            var url = this.BASE_URL_WITH_LANGUAGE + '/api/address/geometry/';
            var request: T4TS.AddressGeometryRequest = {
                uniqueId: this.scope.addressUniqueId
            };
            var scope = this.scope;
            this.$http.post<T4TS.AddressGeometryResponse>(url, request)
                .success(function (data, status, headers, config) {
                scope.addressGeometry = data;
            });
        }
    }
}
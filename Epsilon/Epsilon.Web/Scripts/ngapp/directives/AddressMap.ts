module Epsilon.NgApp.Directives {
    export interface AddressMapScope extends ng.IScope {
        addressUniqueId: string;
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
                template: '<div id="address-map-container" class="google-map-container"><div id="address-map-canvas" class="google-map-canvas"></div></div>',
                link: (scope: AddressMapScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes) => {
                    new AddressMapLink(scope, element, attributes, $http, BASE_URL_WITH_LANGUAGE);
                }
            };
        }
    }

    export class AddressMapLink {
        constructor(private scope: AddressMapScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes,
            private $http: ng.IHttpService, private BASE_URL_WITH_LANGUAGE: string) {
            var mapDiv = document.getElementById("address-map-canvas");
            this.buildMap(mapDiv);
        }

        private buildMap(mapDiv: Element) {
            var url = this.BASE_URL_WITH_LANGUAGE + '/api/address/geometry/';
            var request: T4TS.AddressGeometryRequest = {
                uniqueId: this.scope.addressUniqueId
            };
            this.$http.post<T4TS.AddressGeometryResponse>(url, request).success(function (data, status, headers, config) {
                var geometry = data;
                var center = new google.maps.LatLng(geometry.latitude, geometry.longitude);
                var opts: google.maps.MapOptions = {
                    center: center,
                    mapTypeId: google.maps.MapTypeId.ROADMAP,
                };
                var map = new google.maps.Map(mapDiv, opts);
                var bounds = new google.maps.LatLngBounds();
                var northeast = new google.maps.LatLng(geometry.viewportNortheastLatitude, geometry.viewportNortheastLongitude);
                bounds.extend(northeast)
                var southwest = new google.maps.LatLng(geometry.viewportSouthwestLatitude, geometry.viewportSouthwestLongitude);
                bounds.extend(southwest);
                map.fitBounds(bounds);
                var marker = new google.maps.Marker({
                    position: center,
                    map: map
                })
            });
        }
    }
}
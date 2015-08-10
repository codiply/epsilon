module Epsilon.NgApp.Directives {
    export class StarRatingEditor implements ng.IDirective {
        constructor(
            DIRECTIVE_TEMPLATE_FOLDER_URL: string) {
            return {
                restrict: 'E',
                scope: {
                    passedInModel: "=ngModel"
                },
                templateUrl: DIRECTIVE_TEMPLATE_FOLDER_URL + 'StarRatingEditor'
            };
        }
    }
}
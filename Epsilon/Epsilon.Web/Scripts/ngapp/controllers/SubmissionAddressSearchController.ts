interface SubmissionAddressSearchScope extends ng.IScope {

}

class SubmissionAddressSearchController {
    constructor(private $scope: ng.IScope) {
    }

    public alert() {
        alert("I am an alert from SubmissionAddressSearchController");
    }
}
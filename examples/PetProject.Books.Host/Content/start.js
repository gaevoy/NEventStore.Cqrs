require(["knockout", "jquery", "App/AppViewModel", "ko.widget"], function (ko, $, AppViewModel) {

    var app = new AppViewModel();
    app.init();
    $(function () { ko.applyBindings(app); });

});
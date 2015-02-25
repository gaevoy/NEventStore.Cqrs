define(["knockout", "App/ServiceAgent"], function (ko, ServiceAgent) {

    return function BookEditorViewModel() {
        var self = this;
        var serviceAgent = new ServiceAgent();

        this.id = ko.observable();
        this.title = ko.observable();
        this.isbn = ko.observable();
        this.serverError = ko.observable();

        this.init = function (id) {
            if (id) {
                return serviceAgent.Book.Load(id).then(setModel);
            }
        };
        this.save = function () {
            self.serverError(null);
            var save = self.id() ? serviceAgent.Book.CorrectBook : serviceAgent.Book.RegisterBook;
            save(getModel())
                .done(function () {
                    location = "#";
                })
                .fail(function (result) {
                    self.serverError(result.responseJSON.ErrorMessage);
                });
        };

        function setModel(model) {
            self.id(model.Id);
            self.title(model.Title);
            self.isbn(model.ISBN);
        }
        function getModel() {
            return {
                Id: self.id(),
                Title: self.title(),
                ISBN: self.isbn()
            };
        }
    };

});
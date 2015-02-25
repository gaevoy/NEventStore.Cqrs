define(["knockout", "App/ServiceAgent"], function (ko, ServiceAgent) {

    return function BookListViewModel() {
        var self = this;
        var serviceAgent = new ServiceAgent();
        this.items = ko.observableArray([]);

        this.init = function () {
            return serviceAgent.Book.ListAll().then(self.items);
        };
        this.delete = function (model) {
            serviceAgent.Book.DeleteBook(model).done(function () {
                self.items.remove(model);
            });
        };

    };

});
define(["jquery"], function ($) {

    return function ServiceAgent() {
        this.Book = {
            ListAll: function () {
                return $.ajax({ url: "/api/Book.ListAll" });
            },
            Load: function (id) {
                return $.ajax({ url: "/api/Book.Load", data: { id: id } });
            },
            RegisterBook: function (book) {
                return $.ajax({ url: "/api/Book.RegisterBook", type: "POST", data: book });
            },
            CorrectBook: function (book) {
                return $.ajax({ url: "/api/Book.CorrectBook", type: "POST", data: book });
            },
            DeleteBook: function (book) {
                return $.ajax({ url: "/api/Book.DeleteBook", type: "POST", data: book });
            }
        };
    };

});
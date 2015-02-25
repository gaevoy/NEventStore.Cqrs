define(["knockout", "jquery", "./BookList/BookListWidget", "./BookEditor/BookEditorWidget", "rlite"], function (ko, $, BookListWidget, BookEditorWidget) {

    return function AppViewModel() {
        var self = this;

        this.page = ko.observable(null);

        this.init = function () {
            startRouting();
        };

        function startRouting() {
            var r = new Rlite();

            r.add('', function () { setPage(BookListWidget); });
            r.add('book/new', function () { setPage(BookEditorWidget, null); });
            r.add('book/:id', function (ctx) { setPage(BookEditorWidget, ctx.params.id); });

            // Hash-based routing
            function processHash() {
                var hash = window.location.hash || '#';
                r.run(hash.substr(1));
            }
            window.addEventListener('hashchange', processHash);
            processHash();
        }

        function setPage(PageWidget) {
            var page = new PageWidget();
            var args = Array.prototype.slice.call(arguments, 1);
            var loading = page.init.apply(page, args);
            return $.when(loading).then(function () {
                self.page(page);
            });
        }
    };

});
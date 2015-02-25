define(["ko.widget", "./BookListViewModel", "text!./BookList.htm"], function (Widget, ViewModel, View) {

    return function BookListWidget() {
        Widget.extend(this, [new ViewModel(), View]);
    };

});
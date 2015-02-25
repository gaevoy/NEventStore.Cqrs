define(["ko.widget", "./BookEditorViewModel", "text!./BookEditor.htm"], function (Widget, ViewModel, View) {

    return function BookEditorWidget() {
        Widget.extend(this, [new ViewModel(), View]);
    };

});
todoCompleted = (elm) => {
    fetch('/todo/' + elm.dataset.id, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            isCompleted: (elm.dataset.completed === 'true'),
            title: elm.dataset.title
        })
    }).then(() => {
        htmx.trigger(htmx.find('#getOnLoadDiv'), 'refresh');
    });
}

createTodo = () => {
    fetch('/todos', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            isCompleted: false,
            title: document.getElementById('newTodoTitle').value
        })
    }).then(() => {
        document.getElementById('newTodoTitle').value = '';
        document.getElementById('newTodoTitle').focus();
        htmx.trigger(htmx.find('#getOnLoadDiv'), 'refresh');
    });
}

document.body.addEventListener('htmx:afterRequest', function (evt) {
    if (evt.detail.elt.className.indexOf('btn-delete') > 0) {
        htmx.trigger(htmx.find('#getOnLoadDiv'), 'refresh');
    }
});
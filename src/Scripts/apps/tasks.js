﻿var wvy = wvy || {};
wvy.taskapp = (function ($) {

    var init = function (id, guid, appName, tasks) {
        var eventHub = new Vue();
        var appId = id;
        var appGuid = guid;
        var title = appName;

        var TaskStore = {
            state: {
                items: []
            },

            load: function () {
                this.state.items = tasks;
                return true;
            }
        };

        /* Form component */
        var Form = {
            template: '#tasks-form',
            data: function () {
                return {
                    newTask: ''
                };
            },
            methods: {
                addTask: function (e) {
                    var value = this.newTask && this.newTask.trim();
                    if (!value) {
                        return;
                    }

                    var taskApp = this;

                    $.ajax({
                        url: weavy.url.resolve('/apps/' + appId + '/' + appGuid + '/tasks'),
                        data: JSON.stringify({ name: value }),
                        method: 'POST',
                        contentType: 'application/json'
                    }).then(function (task) {
                        task.assigned_to = null;
                        task.assigned_to_user = null;
                        task.due_date = null;

                        eventHub.$emit('item-added', task);

                        taskApp.newTask = '';
                    });


                }
            }
        };

        /* Header Component */
        var Header = {
            template: '#tasks-header',
            data: function () {
                return {
                    taskListState: TaskStore.state,
                    appName: title
                };
            },
            filters: {
                pluralize: function (n) {
                    return n === 1 ? 'task' : 'tasks';
                }
            },
            computed: {
                taskDone: function () {
                    var total = 0;
                    if (this.taskListState.items.length > 0) {
                        for (var i = 0; i < this.taskListState.items.length; i++) {
                            if (this.taskListState.items[i].completed) {
                                total++;
                            }
                        }
                    }
                    return total;
                },
                taskTotal: function () {
                    return this.taskListState.items.length;
                }
            }
        };

        /* Task item component */
        var TaskItem = {
            template: '#task-item',
            props: ['model'],
            data: function () {
                return {
                    tempText: '',
                    isEditing: false
                };
            },
            filters: {
                datetime: function (d) {
                    return d ? new Date(d).toLocaleString() : "";
                }
            },

            computed: {
                isDone: function () {
                    return this.model.completed;
                },

                isDue: function () {
                    return this.model.due_date && new Date(this.model.due_date) <= new Date();
                }
            },

            methods: {

                toggleStarred: function () {
                    this.model.is_starred = !this.model.is_starred;
                },

                editTaskDetails: function (e) {
                    e.preventDefault();
                    eventHub.$emit('task-details', this.model);
                },

                saveTask: function () {
                    eventHub.$emit('item-save', this.model);
                },

                save: function () {
                    if (this.isEditing && this.tempText != '') {
                        this.model.name = this.tempText;
                        this.isEditing = false;

                        this.saveTask();
                    }
                },

                toggleCompleted: function (e) {
                    e.preventDefault();
                    this.model.completed = !this.model.completed;

                    $.ajax({
                        url: weavy.url.resolve('/apps/' + appId + '/' + appGuid + '/tasks/' + this.model.id + '/toggle'),
                        method: 'PUT',
                        contentType: 'application/json'
                    }).then(function (task) {

                    });
                },

                edit: function () {
                    this.isEditing = true;
                    this.$nextTick(function () {
                        $(this.$el).find('input').focus();
                    });
                    this.tempText = this.model.name;
                },

                cancelEdit: function () {
                    this.isEditing = false;
                },

                deleteItem: function () {
                    eventHub.$emit('item-deleted', this.model);
                },

                showComments: function () {
                    eventHub.$emit('task-comments', this.model);
                },

                showPriorities: function (event) {
                    event.stopPropagation();
                    var target = $(event.currentTarget);
                    var actionList = target.find('.priority-popup');

                    if (actionList.hasClass('show')) {
                        actionList.removeClass('show');
                    } else {
                        $('.priority-popup').removeClass('show');
                        actionList.addClass('show');
                    }
                },

                savePriority: function (type) {
                    this.model.priority = type;
                    this.saveTask();
                }
            }

        };

        /* Task list component */
        var TaskList = {
            template: '#tasks-list',
            props: ['collection'],
            components: {
                'task-item': TaskItem
            },
            methods: {
                saveTask: function (model) {

                    $.ajax({
                        url: weavy.url.resolve('/apps/' + appId + '/' + appGuid + '/tasks'),
                        data: JSON.stringify({ id: model.id, name: model.name, completed: model.completed, dueDate: model.due_date, assignedTo: model.assigned_to, priority: model.priority }),
                        method: 'PUT',
                        contentType: 'application/json'
                    }).then(function (updatedTask) {
                        // update user assigned object
                        model.assigned_to = updatedTask.assigned_to;
                        model.assigned_to_user = updatedTask.assigned_to_user;
                    });
                },

                removeTask: function (task) {
                    var taskList = this;

                    $.ajax({
                        url: weavy.url.resolve('/apps/' + appId + '/' + appGuid + '/tasks/' + task.id),
                        method: 'DELETE',
                        contentType: 'application/json'
                    }).then(function (response) {
                        taskList.collection.splice(taskList.collection.indexOf(task), 1);
                    });
                }

            },

            created: function () {
                var taskList = this;

                eventHub.$on('item-save', function (model) {
                    taskList.saveTask(model);
                });

                eventHub.$on('item-deleted', function (model) {
                    taskList.removeTask(model);
                });

                eventHub.$on('item-added', function (model) {
                    taskList.collection.push(model);
                });
            }
        };

        /* Task details modal */
        var TaskItemModal = {
            template: '#tasks-modal',
            data: function () {
                return {
                    model: null,
                    tmpName: '',
                    tmpDueDate: null,
                    tmpAssignedTo: null,
                    options: [],
                    picker: null
                };
            },
            methods: {
                show: function () {
                    $('#task-details-modal').modal('show');
                },

                hide: function () {
                    $('#task-details-modal').modal('hide');
                },

                cleanup: function () {

                    // clear select2
                    this.options = [];
                    $("select[data-role=user-picker]").children().remove();
                    this.picker.select2('destroy');
                    this.picker = null;
                },

                update: function () {

                    if (this.tmpName != '') {
                        this.model.name = this.tmpName;
                        this.model.due_date = this.tmpDueDate;
                        this.model.assigned_to = this.tmpAssignedTo;

                        eventHub.$emit('item-save', this.model);

                        this.hide();
                    }
                }
            },

            created: function () {
                var modal = this;
                eventHub.$on('task-details', function (model) {

                    // set values
                    modal.model = model;
                    modal.tmpName = model.name;
                    modal.tmpDueDate = model.due_date;
                    modal.tmpAssignedTo = model.assigned_to;
                    modal.options = [];

                    if (model.assigned_to_user != null) {
                        modal.options.push({ text: model.assigned_to_user.profile.name, value: model.assigned_to_user.id });
                    } else {
                        modal.tmpAssignedTo = null;
                        modal.options = [];
                    }

                    // show modal
                    modal.show();
                });
            },

            mounted: function () {
                var modal = this;
                $(document).on("shown.bs.modal", "#task-details-modal", function (e) {
                    setTimeout(function () {
                        modal.picker = weavy.userspicker.init("select[data-role='user-picker']");
                        modal.picker.on("change", function () {
                            modal.tmpAssignedTo = this.value;
                        });
                    }, 1);

                });

                $(document).on("hidden.bs.modal", "#task-details-modal", function (e) {
                    modal.cleanup();
                });
            }
        };

        /* Task comments modal */
        var TaskItemComments = {
            template: '#tasks-comments',
            data: function () {
                return {
                    model: null
                };
            },
            methods: {
                show: function () {
                    $('#task-comments-modal').modal('show');
                },

                hide: function () {
                    $('#task-comments-modal').modal('hide');
                }
            },

            created: function () {
                var modal = this;

                eventHub.$on('task-comments', function (model) {
                    modal.model = model;

                    modal.show();
                });
            },

            mounted: function () {
                var taskComments = this;

                $(document).on("show.bs.modal", "#task-comments-modal", function (e) {
                    // clear modal content and show spinner
                    var $modal = $(this);
                    $(".spinner", $modal).addClass("spin").show();
                    $(".modal-body", $modal).empty();
                });

                $(document).on("shown.bs.modal", "#task-comments-modal", function (e) {

                    var $modal = $(this);
                    var $spinner = $(".spinner", $modal);
                    var $body = $(".modal-body", $modal);

                    $.ajax({
                        url: weavy.url.resolve('/apps/' + appId + '/' + appGuid + '/tasks/' + taskComments.model.id + '/comments'),
                        type: "GET"
                    }).then(function (html) {

                        $body.html(html);

                        weavy.comments.initCommentEditor($("textarea.comments-form", $body));
                        weavy.urlContext.init();

                    }).always(function () {
                        // hide spinner
                        $spinner.removeClass("spin").hide();
                    });
                });
            }
        };

        /* App */
        new Vue({
            el: '#app',
            data: function () {
                return {
                    taskListState: TaskStore.state
                };
            },

            mounted: function () {
                TaskStore.load();

                // rtm tasks
                weavy.realtime.on("task_toggle_completed", function (e, data) {

                    var t = TaskStore.state.items.find(function (task) {
                        return task.id === data.task_id;
                    });

                    if (t) {
                        t.completed = data.completed;
                    }
                });
            },

            created: function () {
                window.addEventListener('click', this.hidePriorities);
            },

            methods: {
                hidePriorities: function () {
                    $('.priority-popup').removeClass('show');
                }
            },

            components: {
                'tasks-header': Header,
                'tasks-form': Form,
                'tasks-list': TaskList,
                'tasks-modal': TaskItemModal,
                'tasks-comments': TaskItemComments
            }
        });

    };

    var destroy = function () {

    };

    return {
        init: init,
        destroy: destroy
    };

})(jQuery);

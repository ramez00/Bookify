const filterSearch = document.querySelector('#searchDataTable');
filterSearch.addEventListener('keyup', function (e) {
    datatable.search(e.target.value).draw();
});

$(document).ready(function () {
    datatable = $('#Books').DataTable({
        serverSide: true,
        processing: true,
        stateSave: true,
        language: {
            processing: '<div class="d-flex justify-content-center text-primary align-items-center dataTableSpinnerDiv"><div class="spinner-border" role = "status" ></div><span class="text-muted ps-2">Loading...</span</div>'
        },
        ajax: {
            url: '/Books/GetBooks',
            type: 'POST'
        },
        order: [[1, 'asc']],
        columnDefs: [{
            targets: [0],
            visible: false,
            searchable: false
        }],
        columns: [
            { "data": "id", "name": "Id", "className": "d-none" },
            {
                "name": "Title",
                "className": "d-flex align-item-center",
                "render": function (data, type, row) {
                    return `<div class="symbol symbol-50px overflow-hidden me-3">
                                                        <a href="/Books/Detials/${row.id}">
                                                            <div class="symbol-label">
                                                               <img class="w-100" alt="" src="${(row.imageThumbUrl === null ? '/images/books/no-img.png' : row.imageThumbUrl)} " />
                                                            </div>
                                                        </a>
                                                    </div>
                                                    <div class="d-flex flex-column">
                                                        <a href="/Books/Detials/${row.id}" class="text-primary fw-bolder">${row.title}</a>
                                                        <span>${row.author}</span>
                                                    </div>`;
                }
            },
            { "data": "publisher", "name": "Publisher" },
            {
                "name": "PublishingDate", "render": function (data, type, row) {
                    return moment(row.publishingDate).format('ll');
                }
            },
            { "data": "hall", "name": "Hall" },
            { "data": "categories", "name": "Categories", "orderable": false },
            {
                "name": "IsAvailableForRent",
                "render": function (data, type, row) {
                    return ` <span class="badge badge-light-${(row.isAvailableForRent ? "success" : "warning")} ">
                                                        ${(row.isAvailableForRent ? "Avaliable" : "Not Avaliable")}
                                             </span>`;
                }
            },
            {
                "name": "IsDeleted",
                "render": function (data, type, row) {
                    return ` <span class="badge badge-light-${(row.isDeleted ? "danger" : "success")} js-status">
                                                  ${(row.isDeleted ? "Deleted" : "Avaliable")}
                                             </span>`;
                }
            },
            {
                "name": "action", "orderable": false,
                "render": function (data, type, row) {
                    return ` <div class="btn-group">
                            <button class="btn btn-link btn-sm show" type="button" data-bs-toggle="dropdown" aria-expanded="false">
                                <span class="svg-icon svg-icon-primary svg-icon-2x">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="24px" height="24px" viewBox="0 0 24 24">
                                        <g stroke="none" stroke-width="1" fill="none" fill-rule="evenodd">
                                            <rect x="5" y="5" width="5" height="5" rx="1" fill="currentColor" />
                                            <rect x="14" y="5" width="5" height="5" rx="1" fill="currentColor" opacity="0.3" />
                                            <rect x="5" y="14" width="5" height="5" rx="1" fill="currentColor" opacity="0.3" />
                                            <rect x="14" y="14" width="5" height="5" rx="1" fill="currentColor" opacity="0.3" />
                                        </g>
                                    </svg>
                                </span>
                            </button>
                            <ul class="dropdown-menu">
                                <li>
                                        <a href="/Books/Detials/${row.id}" class="btn btn-sm btn-outline-primary">
                                        <i class="bi bi-pencil"></i>
                                        Edit
                                    </a>
                                </li>
                                <li>
                                    <a href="javascript:;" class="btn btn-sm btn-outline-success js-toggle-status"
                                               data-url="/Books/ToggleStatus/${row.id}">
                                        Toggle Status
                                    </a>
                                </li>
                            </ul>
                        </div>`
                }
            }
        ]
    })
})
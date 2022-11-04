var dataTable;

$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("pending")) {
        loadDataTable("pending");
    } else if (url.includes("confirmed")) {
        loadDataTable("confirmed");
    } else if (url.includes("processing")) {
        loadDataTable("processing");
    } else if (url.includes("shipped")) {
        loadDataTable("shipped");
    } else if (url.includes("all")) {
        loadDataTable("all");
    }
});

function loadDataTable(status) {
    dataTable = $("#orderDataTable").DataTable({
        "ajax": {
            "url": "/Admin/Order/GetAll?status=" + status
        },
        "columns": [
            { "data": "id", "width": "5%"},
            { "data": "name", "width": "12.5%" },
            { "data": "phoneNumber", "width": "12.5%" },
            { "data": "applicationUser.email", "width": "12.5%" },
            { "data": "orderStatus", "width": "12.5%" },
            { "data": "paymentStatus", "width": "12.5%" },
            {
                "data": "orderTotal",
                "render": function (data) {
                    return data.toFixed(2);
                }
            },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="btn-group" role="group">
							<a href="/Admin/Order/Details?orderId=${data}"
							    class="btn btn-primary mx-2">Go to order details</a>
						</div>
                        `
                },
                "width": "12.5%"
            }
        ]
    });
}

function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: "Delete",
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload();
                        Swal.fire(
                            "Deleted!",
                            data.message,
                            "success"
                        )
                    } else {
                        Swal.fire(
                            "Delete unsuccessful",
                            data.message,
                            "error"
                        )
                    }
                }
            })
            
        }
    })
}
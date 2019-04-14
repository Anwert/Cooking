$(document).ready(function() {
	$(".delete-recipe").click(function () {
		var currentRow = $(this).closest('tr');
		var favourId = currentRow.children('td:first').text();
		$.ajax({
			type: 'DELETE',
			url: '/Manager/DeleteFavour',
			data: {
				id: favourId
			},
			success: function (result) {
				if (result.error) {
					alert("Произошла ошибка: " + result.message);
				}
				currentRow.remove();
			},
			error: function() {
				alert("Произошла ошибка.");
			}
		});
	})
});

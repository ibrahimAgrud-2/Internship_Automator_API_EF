// Stajyer detay sayfası: görevdeki "Sil" butonuyla açılan onay penceresi.
// intern-detail.js içindeki internTasks ve loadDetails(), intern-complete-task.js içindeki
// tasksListElement burada kullanılır.

const deleteTaskModalElement = document.getElementById("deleteTaskModal");
const deleteTaskModal = new bootstrap.Modal(deleteTaskModalElement);
const deleteTaskName = document.getElementById("deleteTaskName");
const deleteTaskError = document.getElementById("deleteTaskError");
const deleteTaskConfirm = document.getElementById("deleteTaskConfirm");
const deleteTaskCancel = document.getElementById("deleteTaskCancel");
const deleteTaskNoticeBox = document.getElementById("noticeBox");

let taskToDeleteID = null;

function openDeleteTaskModal(taskID) {
    const task = internTasks.find(item => item.taskID === taskID);
    if (!task) return;

    taskToDeleteID = taskID;
    deleteTaskName.textContent = task.title;
    hideMessage(deleteTaskError);
    deleteTaskConfirm.disabled = false;
    deleteTaskCancel.disabled = false;

    deleteTaskModal.show();
}

async function confirmDeleteTask() {
    if (deleteTaskConfirm.disabled || taskToDeleteID === null) return;   // çift tıklama koruması

    const title = internTasks.find(item => item.taskID === taskToDeleteID)?.title ?? "";
    hideMessage(deleteTaskError);
    deleteTaskConfirm.disabled = true;
    deleteTaskCancel.disabled = true;

    try {
        await deleteTask(taskToDeleteID);

        deleteTaskModal.hide();
        showNotice(deleteTaskNoticeBox, "success", `"${title}" görevi silindi.`);
        await loadDetails();
    } catch (error) {
        showError(deleteTaskError, error);   // API'nin mesajı olduğu gibi gösterilir

        // Görev zaten yoksa ekrandaki liste eskimiş demektir; arkada tazelenir
        if (error instanceof ApiError && error.status === 404) {
            loadDetails();
        }
    } finally {
        deleteTaskConfirm.disabled = false;
        deleteTaskCancel.disabled = false;
    }
}

// ---------- Olay dinleyicileri ----------
tasksListElement.addEventListener("click", (event) => {
    const button = event.target.closest("button[data-action=delete]");
    if (!button || button.disabled) return;

    openDeleteTaskModal(Number(button.dataset.taskId));
});

deleteTaskConfirm.addEventListener("click", confirmDeleteTask);
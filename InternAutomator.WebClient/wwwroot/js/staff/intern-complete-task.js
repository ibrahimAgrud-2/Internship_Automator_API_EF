// Stajyer detay sayfası: görevdeki "Tamamla" butonuyla açılan onay penceresi.
// intern-detail.js içindeki internTasks ve loadDetails() burada kullanılır.

const tasksListElement = document.getElementById("tasksList");
const completeModalElement = document.getElementById("completeTaskModal");
const completeModal = new bootstrap.Modal(completeModalElement);
const completeName = document.getElementById("completeTaskName");
const completeError = document.getElementById("completeTaskError");
const completeConfirm = document.getElementById("completeTaskConfirm");
const completeCancel = document.getElementById("completeTaskCancel");
const completeNoticeBox = document.getElementById("noticeBox");

let taskToCompleteID = null;

function openCompleteModal(taskID) {
    const task = internTasks.find(item => item.taskID === taskID);
    if (!task) return;

    taskToCompleteID = taskID;
    completeName.textContent = task.title;
    hideMessage(completeError);
    completeConfirm.disabled = false;
    completeCancel.disabled = false;

    completeModal.show();
}

async function confirmComplete() {
    if (completeConfirm.disabled || taskToCompleteID === null) return;   // çift tıklama koruması

    const title = internTasks.find(item => item.taskID === taskToCompleteID)?.title ?? "";
    hideMessage(completeError);
    completeConfirm.disabled = true;
    completeCancel.disabled = true;

    try {
        await completeTask(taskToCompleteID);

        completeModal.hide();
        showNotice(completeNoticeBox, "success", `"${title}" görevi tamamlandı olarak işaretlendi.`);
        await loadDetails();
    } catch (error) {
        showError(completeError, error);   // API'nin mesajı olduğu gibi gösterilir

        // Görev artık yoksa ekrandaki liste eskimiş demektir; arkada tazelenir
        if (error instanceof ApiError && error.status === 404) {
            loadDetails();
        }
    } finally {
        completeConfirm.disabled = false;
        completeCancel.disabled = false;
    }
}

// ---------- Olay dinleyicileri ----------
tasksListElement.addEventListener("click", (event) => {
    const button = event.target.closest("button[data-action=complete]");
    if (!button || button.disabled) return;

    openCompleteModal(Number(button.dataset.taskId));
});

completeConfirm.addEventListener("click", confirmComplete);
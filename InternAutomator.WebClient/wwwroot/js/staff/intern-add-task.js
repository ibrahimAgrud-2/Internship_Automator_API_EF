// Stajyer detay sayfası: "Görevler > Ekle" ile açılan görev ekleme penceresi.
// intern-detail.js içindeki internID, internProjects ve loadDetails() burada kullanılır.

const taskAddButton = document.getElementById("tasksAdd");
const taskModalElement = document.getElementById("addTaskModal");
const taskModal = new bootstrap.Modal(taskModalElement);
const taskForm = document.getElementById("addTaskForm");
const taskError = document.getElementById("addTaskError");
const taskNoticeBox = document.getElementById("noticeBox");
const taskProjectSelect = document.getElementById("taskProjectSelect");
const taskTitleInput = document.getElementById("taskTitleInput");
const taskDescriptionInput = document.getElementById("taskDescriptionInput");
const taskDueDateInput = document.getElementById("taskDueDateInput");
const taskSubmit = document.getElementById("addTaskSubmit");
const taskCancel = document.getElementById("addTaskCancel");

taskTitleInput.maxLength = TASK_LIMITS.title;
taskDescriptionInput.maxLength = TASK_LIMITS.description;

// Görevler gün bazlıdır; saat sorulmaz. Seçilen günün sonu (23:59:59) gönderilir, böylece
// API'nin "teslim zamanı şu andan sonra olmalı" kuralı bugün seçildiğinde de sağlanır.
// Saat bilgisi eklenmek istenirse sadece bu fonksiyon değişir.
function toTaskDueDate(dayText) {
    return `${dayText}T23:59:59`;
}

function setTaskBusy(isBusy) {
    taskProjectSelect.disabled = isBusy;
    taskTitleInput.disabled = isBusy;
    taskDescriptionInput.disabled = isBusy;
    taskDueDateInput.disabled = isBusy;
    taskCancel.disabled = isBusy;
    taskSubmit.disabled = isBusy;
}

// Proje listesi: stajyerin atanmış projeleri (görev ongoingProjectID ile bağlanır)
function fillTaskProjectSelect() {
    taskProjectSelect.replaceChildren();

    // Birden fazla proje varsa kullanıcı bilerek seçsin; tek proje varsa hazır gelir
    if (internProjects.length !== 1) {
        const placeholder = document.createElement("option");
        placeholder.value = "";
        placeholder.textContent = "Proje seçin";
        taskProjectSelect.appendChild(placeholder);
    }

    internProjects.forEach(project => {
        const option = document.createElement("option");
        option.value = project.ongoingProjectID;
        option.textContent = project.projectTitle;
        taskProjectSelect.appendChild(option);
    });
}

function openTaskModal() {
    taskForm.reset();
    hideMessage(taskError);
    setTaskBusy(false);

    fillTaskProjectSelect();
    taskDueDateInput.min = getTodayText();   // sayfa gece yarısını geçerse diye her açılışta yenilenir

    taskModal.show();
}

async function submitTask(event) {
    event.preventDefault();
    if (taskSubmit.disabled) return;   // çift gönderim koruması

    const ongoingProjectID = Number(taskProjectSelect.value);
    const title = taskTitleInput.value.trim();
    const dayText = taskDueDateInput.value;

    // Asıl doğrulama API'dadır; buradakiler kullanıcıyı baştan uyarır.
    // min özelliği elle yazılan tarihi engellemez, bu yüzden tarih burada da kontrol edilir.
    let message = "";
    let invalidField = null;

    if (!ongoingProjectID) {
        message = "Lütfen bir proje seçin.";
        invalidField = taskProjectSelect;
    } else if (!title) {
        message = "Görev adı zorunludur.";
        invalidField = taskTitleInput;
    } else if (!dayText) {
        message = "Teslim tarihi zorunludur.";
        invalidField = taskDueDateInput;
    } else if (dayText < getTodayText()) {
        message = "Teslim tarihi bugünden önce olamaz.";
        invalidField = taskDueDateInput;
    } else if (!session.userID) {
        message = "Oturum bilgisi eksik. Lütfen çıkış yapıp tekrar giriş yapın.";
    }

    if (message) {
        showMessage(taskError, "danger", message);
        invalidField?.focus();
        return;
    }

    hideMessage(taskError);
    setTaskBusy(true);

    try {
        await addTask({
            createdByUserID: session.userID,
            ongoingProjectID,
            title,
            description: taskDescriptionInput.value.trim(),
            dueDate: toTaskDueDate(dayText)
        });

        taskModal.hide();
        showNotice(taskNoticeBox, "success", `"${title}" görevi eklendi.`);
        await loadDetails();
    } catch (error) {
        showError(taskError, error);   // API'nin mesajı olduğu gibi gösterilir
    } finally {
        setTaskBusy(false);
    }
}

// ---------- Olay dinleyicileri ----------
taskAddButton.addEventListener("click", openTaskModal);
taskForm.addEventListener("submit", submitTask);

taskModalElement.addEventListener("shown.bs.modal", () => {
    // Tek proje varsa doğrudan görev adına odaklan
    (taskProjectSelect.value ? taskTitleInput : taskProjectSelect).focus();
});